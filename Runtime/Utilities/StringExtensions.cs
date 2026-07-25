using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace VeryFS.Framework.Runtime.Utilities
{
    public static class StringExtensions
    {
        public static Color ToColor(this string htmlString)
        {
            return ToColor(htmlString, Color.magenta);
        }
        
        public static Color ToColor(this string htmlString, Color defaultColor)
        {
            if (string.IsNullOrEmpty(htmlString))
                return defaultColor;

            // ColorUtility 标准约定：RRGGBB / RRGGBBAA，与 NamedColors.ToJsonData 输出一致；
            // 不抛异常，非法输入落 defaultColor
            string html = htmlString.StartsWith("#") ? htmlString : "#" + htmlString;
            if (ColorUtility.TryParseHtmlString(html, out var color))
                return color;

            Debug.LogWarning($"Could not convert '{htmlString}' to a color.");
            return defaultColor;
        }

        public static T ToEnum<T>(this string str)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Type '{typeof(T)}' is not an enum.");
            }

            var enumString = str.Replace("-", "_");
            T value;

            try
            {
                value = (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch
            {
                Debug.LogError($"Could not convert '{enumString}' to enum type '{typeof(T).Name}'.");
                value = default(T);
            }

            return value;
        }

        public static float ToFloat(this string str)
        {
            float result;
            if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                Debug.LogError($"Could not convert '{str}' to float.");
            }

            return result;
        }

        public static int ToInt(this string str)
        {
            int result;
            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                Debug.LogError($"Could not convert '{str}' to int.");
            }

            return result;
        }

        public static bool ToBool(this string str)
        {
            if (str.Equals("1") || str.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (str.Equals("0") || str.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Debug.LogError($"Could not convert '{str}' to bool.");
            return false;
        }


        /// <summary>
        /// 是否为数字
        /// </summary>
        public static bool IsNumber(this string strValue)
        {
            var regex = new Regex("[^0-9]");
            return !regex.IsMatch(strValue);
        }

        //判断是否为数字或字母
        public static bool IsDigitOrLetter(this string strValue)
        {
            return Regex.IsMatch(strValue, "^[a-zA-Z0-9]*$");
        }

        //判断是否为邮箱
        public static bool IsEmail(this string strValue)
        {
            return Regex.IsMatch(strValue, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.IgnoreCase);
        }



        private static StringBuilder _stringBuilder;

        private static StringBuilder StringBuilder
        {
            get
            {
                if (_stringBuilder == null)
                {
                    _stringBuilder = new StringBuilder();
                }
                else
                {
                    _stringBuilder.Length = 0;
                }

                return _stringBuilder;
            }
        }

        public static string Repeat(this string str, int count)
        {
            if (count <= 0)
                return "";

            int totalLen = str.Length * count;
            var sb = StringBuilder;
            if (sb.Capacity < totalLen)
            {
                sb.Capacity = totalLen;
            }

            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }

    }
}