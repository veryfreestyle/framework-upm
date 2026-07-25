using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using UnityEngine;
using System.Text.RegularExpressions;

namespace VeryFS.Framework.Runtime.Utilities
{
    public static class Utils
    {

        public static bool IsNetworkReachability 
        {
            get
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        //print("当前使用的是：WiFi，请放心更新！");
                        return true;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        //print("当前使用的是移动网络，是否继续更新？");
                        return true;
                    default:
                        //print("当前没有联网，请您先联网后再进行操作！");
                        return false;
                }
            }
        }
        
        private static readonly string[] ReadableSizeUnits = {"B", "KB", "MB", "GB", "TB", "PB"};

        public static string HumanReadableSize(long size)
        {
            double mod = 1024.0;
            double dSize = Math.Abs(size);
           
            int i = 0;
            while (dSize >= mod)
            {
                dSize /= mod;
                i++;
            }
            return $"{ (size < 0 ? -1 : 1) * dSize:N2}{ReadableSizeUnits[i]}";
        }
        
        /// <summary>
        /// Hash算法函数
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="hashName">SHA1, MD5, SHA256</param>
        /// <returns></returns>
        public static string Hash_Stream(Stream stream,string hashName)
        {
            var hash= HashAlgorithm.Create(hashName);
            byte[] bytes = hash.ComputeHash(stream);
            string result = BitConverter.ToString(bytes);
            result = result.Replace("-", "");
            return result;
        }

        public static string Hash_File(string filePath,string hashName="SHA1")
        {
            try
            {
                using (FileStream get_file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return Hash_Stream(get_file,hashName);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static string Hash_Bytes(byte[] bytes,string hashName="SHA1")
        {
            var hash= HashAlgorithm.Create(hashName);
            byte[] output = hash.ComputeHash(bytes);
            string result = BitConverter.ToString(output);
            result = result.Replace("-", "");
            return result;
        }

        public static string Hash_String(string str,string hashName="SHA1")
        {
            return Hash_Bytes(Encoding.Unicode.GetBytes(str),hashName);
        }
        

        public static T ToEnum<T>(string e)
        {
            return (T) Enum.Parse(typeof(T), e);
        }

        

        public static void GetIpAddress(string host, Action<IPAddress> callback )
        {
            IPAddress ipAddress = null;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                Dns.BeginGetHostAddresses(host, asyncResult =>
                {
                    IPAddress[] addrs = Dns.EndGetHostAddresses(asyncResult);
                    if (addrs.Length > 0)
                        ipAddress = addrs[0];
                    callback(ipAddress);
                }, null);
            }
            else
            {
                callback(ipAddress);
            }
        }

      

        #region CRC32
        
        public static uint CRC32_File(string filePath)
        {
            try
            {
                using (FileStream get_file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return CRC32_Stream(get_file);
                }
            }
            catch (Exception )
            {
                return 0;
            }
        }

        public static uint CRC32_String(string str)
        {
            return CRC32_Bytes(Encoding.Unicode.GetBytes(str));
        }
        
        public static uint CRC32_Stream(Stream stream)
        {
            byte[] bytes = new byte[2048];

            uint crc = 0xFFFFFFFF;
            int nRead = 0;
            while ((nRead = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                for (uint i = 0; i < nRead; i++)
                {
                    //crc = (crc << 8) ^ s_crcTable[(crc >> 24) ^ bytes[i]];
                    crc = ((crc >> 8) & 0x00FFFFFF) ^ s_crcTable[(crc ^ bytes[i]) & 0xFF];
                }
            }
            UInt32 temp = crc ^ 0xFFFFFFFF;
            return temp;
        }
        
        public static uint CRC32_Bytes(byte[] bytes)
        {
            uint iCount = (uint)bytes.Length;
            uint crc = 0xFFFFFFFF;
 
            for (uint i = 0; i < iCount; i++)
            {                
                //crc = (crc << 8) ^ s_crcTable[(crc >> 24) ^ bytes[i]];
                crc = ((crc >> 8) & 0x00FFFFFF) ^ s_crcTable[(crc ^ bytes[i]) & 0xFF];
            }            
            UInt32 temp = crc ^ 0xFFFFFFFF;
            return temp;
        }

        private static UInt32[] s_crcTable =
        {
            0x0, 0x77073096, 0xee0e612c, 0x990951ba, 0x76dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
            0xedb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x9b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
            0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
            0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
            0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
            0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
            0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
            0x76dc4190, 0x1db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x6b6b51f, 0x9fbfe4a5, 0xe8b8d433,
            0x7807c9a2, 0xf00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x86d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
            0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
            0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
            0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
            0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
            0xedb88320, 0x9abfb3b6, 0x3b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x4db2615, 0x73dc1683,
            0xe3630b12, 0x94643b84, 0xd6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0xa00ae27, 0x7d079eb1,
            0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
            0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
            0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
            0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
            0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
            0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x26d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x5005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0xcb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0xbdbdf21,
            0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
            0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
            0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
            0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
            0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d,
        };

        #endregion


        #region JSON

        

        public static string ToJson(object obj,bool pretty=true)
        {
            StringBuilder output = new StringBuilder();
            var writer = new JsonWriter(output);
            writer.PrettyPrint = pretty;
            JsonMapper.ToJson(obj,writer);
            /*using (JsonWriter writer = new JsonWriter(output,
                new JsonWriterSettings
                {
                    PrettyPrint = pretty,
                    Tab = "    ",
                    NewLine = "\n"
                }))
            {
                writer.Write(obj);
            }*/
            return Regex.Unescape(output.ToString());
        }

        public static void ToJsonFile(string filePath, object obj, bool pretty = true)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                string output = ToJson(obj, pretty);
                byte[] bytes = Encoding.UTF8.GetBytes(output);
                fs.Write(bytes,0,bytes.Length);
            }
        }

        public static JsonData ReadJsonFromFile(string filePath)
        {
            var cache = File.ReadAllBytes(filePath);
            string input = Encoding.UTF8.GetString(cache);
            return JsonMapper.ToObject(input);
            // using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            // {
            //     var cache = new byte[fs.Length];
            //     fs.Read(cache, 0, (int) fs.Length);
            //     string input = Encoding.UTF8.GetString(cache);
            //     
            // }
        }
        
        public static T FromJson<T>(string input)
        {
            return JsonMapper.ToObject<T>(input);
            //return JsonReader.Deserialize<T>(input);
        }
        
        public static T FromJsonFile<T>(string filePath)
        {
            return FromJsonFile<T>(filePath, null);
        }
        
        public static T FromJsonFile<T>(string filePath, byte[] cache)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (cache == null || cache.Length < fs.Length)
                {
                    cache = new byte[fs.Length];
                }
                
                int read = fs.Read(cache, 0, (int) fs.Length);
                // 只解码本次读到的字节：复用的大缓存尾部可能残留上次内容
                string input = Encoding.UTF8.GetString(cache, 0, read);
                return JsonMapper.ToObject<T>(input);
            }
        }
        
        
        #endregion 
        
        
        
        
        public static bool CompareApproximately (float f0, float f1, float epsilon = 0.000001F)
        {
            float dist = (f0 - f1);
            dist = Mathf.Abs (dist);
            return dist < epsilon;
        }
        
        
        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string HashToMD5Hex(string sourceStr)
        {
            byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                var result = md5.ComputeHash(Bytes);
                StringBuilder builder = new StringBuilder();
                for (var i = 0; i < result.Length; i++)
                    builder.Append(result[i].ToString("x2"));
                return builder.ToString();
            }
        }
        
        public static string Float2Str(float value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }

        public static float Str2Float(string value)
        {
            float f;
            if (!float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out  f))
            {
                f = 0;
            }

            return f;
        }
        
        
        public static String StringXOR(String str , string key)
        {
            byte[] mwChar = Encoding.Unicode.GetBytes(str);
            byte[] keyChar = Encoding.Unicode.GetBytes(key);
            
            
            for(int i = 0;i < mwChar.Length ; i++)
            {
                mwChar[i] = (byte)(mwChar[i] ^ keyChar[i % keyChar.Length]);
            }
            string after= Encoding.Unicode.GetString(mwChar);
            //
            // Debug.LogError(str);
            // Debug.LogError(after.Length);
            return after;
        }

        
        // #region 数据存取
        //
        // /// <summary>
        // /// 取得整型
        // /// </summary>
        // public static int GetPlayerInt(string key)
        // {
        //     return PlayerPrefs.GetInt(key);
        // }
        //
        // /// <summary>
        // /// 有没有值
        // /// </summary>
        // public static bool HasPlayerKey(string key)
        // {
        //     return PlayerPrefs.HasKey(key);
        // }
        //
        // /// <summary>
        // /// 保存整型
        // /// </summary>
        // public static void SetPlayerInt(string key, int value)
        // {
        //     PlayerPrefs.DeleteKey(key);
        //     PlayerPrefs.SetInt(key, value);
        //     PlayerPrefs.Save();
        // }
        //
        // /// <summary>
        // /// 保存浮点型
        // /// </summary>
        // /// <param name="key"></param>
        // /// <param name="value"></param>
        // public static void SetPlayerFloat(string key, float value)
        // {
        //     PlayerPrefs.DeleteKey(key);
        //     PlayerPrefs.SetFloat(key, value);
        //     PlayerPrefs.Save();
        // }
        //
        // public static float GetPlayerFloat(string key)
        // {
        //     return PlayerPrefs.GetFloat(key);
        // }
        //
        // /// <summary>
        // /// 取得数据
        // /// </summary>
        // public static string GetPlayerString(string key)
        // {
        //     return PlayerPrefs.GetString(key);
        // }
        //
        // /// <summary>
        // /// 保存数据
        // /// </summary>
        // public static void SetPlayerString(string key, string value)
        // {
        //     PlayerPrefs.DeleteKey(key);
        //     PlayerPrefs.SetString(key, value);
        //     PlayerPrefs.Save();
        // }
        //
        // /// <summary>
        // /// 删除数据
        // /// </summary>
        // public static void RemoveData(string key)
        // {
        //     PlayerPrefs.DeleteKey(key);
        //     PlayerPrefs.Save();
        // }
        //
        // /// <summary>
        // /// 删除数据
        // /// </summary>
        // public static void RemoveAllData()
        // {
        //     PlayerPrefs.DeleteAll();
        // }
        //
        // #endregion 数据存取
    }
}