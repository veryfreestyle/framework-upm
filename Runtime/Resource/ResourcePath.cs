// Author: JiangHao <jianghao01@hetao101.com>

using System.IO;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Resource
{

    public enum ResourcePlatform
    {
        Windows = 0,
        MacOS = 1,
        Android = 2,
        iOS = 3,
        WebGL = 4,
    }

    public static class ResourcePath
    {

        public static ResourcePlatform Platform { get; private set; }

        // public static string FileProtocol { get; private set; }

        public static string PersistentDataURL { get; private set; }

        public static string BundlesPathRelative { get; private set; }
        
        // public static string ApplicationPath { get; private set; }
        
        // public static string ApplicationPath 
        public static string StreamingAssetsURL { get; private set; }

        // public static string StreamingAssetsPath { get; private set; }
        
        public const string BundleExtension = ".ab";

        /// <summary>
        /// AssetBundle 加解密密钥，由宿主在 <see cref="Initialize"/> 之前赋值。
        /// 库侧不持有项目专属密钥——一个项目一份密钥，避免跨项目复用同一份。
        /// 仅当定义了 ENABLE_ASSETBUNDLE_PROTECTION 时被使用。
        /// </summary>
        public static string AssetBundleKey { get; set; }


        public static string GetResourceURL(string relativePath)
        {
            return GetResourceURL(relativePath, out _);
        }
        
        public static string GetResourceURL(string relativePath,out bool isPersistent)
        {
            isPersistent = false;
            string path =   "/" + BundlesPathRelative + "/" + relativePath;

            var uri = new System.Uri(PersistentDataURL + path);
            
            if (File.Exists(uri.LocalPath))
            {
                isPersistent = true;
                return uri.ToString();
            }
            uri = new System.Uri(StreamingAssetsURL + path);
            return uri.ToString();
            // // 注意，StreamingAssetsPath在Android平台時，壓縮在apk里面，不要做文件檢查了
            // if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
            // {
            //     return true;
            // }
            // return File.Exists(uri.LocalPath);
        }

        public static string GetStreamingResourceURL(string relativePath)
        {
            string path =   "/" + BundlesPathRelative + "/" + relativePath;
            var uri = new System.Uri(StreamingAssetsURL + path);
            return uri.ToString();
        }
        
        public static string GetPersistentResourceURL(string relativePath)
        {
            string path =   "/" + BundlesPathRelative + "/" + relativePath;
            var uri = new System.Uri(PersistentDataURL + path);
            return uri.ToString();
        }

        public static string GetRawResourceURL(string relativePath)
        {
            string path =   "/" + relativePath;

            var uri = new System.Uri(PersistentDataURL + path);
            
            if (File.Exists(uri.LocalPath))
            {
                return uri.ToString();
            }
            
            uri = new System.Uri(Application.streamingAssetsPath + path);
            return uri.ToString();
        }
        
        // public static byte[] LoadFileFromStreamingAssets(string relativePath)
        // {
        //     byte[] bytes = null;
        //     if (Application.platform == RuntimePlatform.Android)
        //     {
        //         string path = Path.Combine(Application.dataPath + "!/assets/", relativePath);
        //         
        //         if (!AndroidPlugin.IsAssetExists(path))
        //         {
        //             Debug.LogError("LoadDataFromStreamingAssets error. " + relativePath);
        //         }
        //         else
        //         {
        //             return AndroidPlugin.GetAssetBytes(path);
        //         }
        //     }
        //     else
        //     {
        //         var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        //         using (FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //         {
        //             bytes = new byte[fs.Length];
        //             fs.Read(bytes, 0, (int) fs.Length);
        //         }
        //     }
        //     return bytes;
        // }
        
        public static void Initialize()
        {

            Platform = GetPlatform();

            // FileProtocol = Application.platform == RuntimePlatform.WindowsEditor ||
            //                Application.platform == RuntimePlatform.WindowsPlayer
            //     ? "file:///"
            //     : "file://";

            PersistentDataURL = new System.Uri(Application.persistentDataPath).ToString();
            // PersistentDataPath = PersistentDataPath.Replace("\\", "/");

            BundlesPathRelative = "Bundles/" + Platform;

            if (Application.isEditor)
            {
                var uri = new System.Uri(Path.GetFullPath("Product"));
                StreamingAssetsURL =  uri.ToString();
            }
            else
            {
                var uri = new System.Uri(Application.streamingAssetsPath);
                StreamingAssetsURL = uri.ToString();
            }
            
            #if ENABLE_ASSETBUNDLE_PROTECTION
            // 密钥缺失时立刻炸：否则 bundle 加密了却不解密，故障会推迟到加载期且报错无线索
            if (string.IsNullOrEmpty(AssetBundleKey))
                throw new System.InvalidOperationException(
                    "ENABLE_ASSETBUNDLE_PROTECTION 已开启但 ResourcePath.AssetBundleKey 未赋值，" +
                    "加密的 bundle 将无法解密。宿主须在 ResourcePath.Initialize() 之前设置密钥。");
            AssetBundle.SetAssetBundleDecryptKey(AssetBundleKey);
            #endif
            // switch (Application.platform)
            // {
            //     case RuntimePlatform.WindowsEditor:
            //     case RuntimePlatform.OSXEditor:
            //     {
            //        
            //         streamingAssetsURL =  new System.Uri(Path.GetFullPath("Product").Replace("\\", "/")).ToString();
            //         // FileProtocol + Path.GetFullPath("Product").Replace("\\", "/");
            //         break;
            //     }
            //     de
            //     case RuntimePlatform.WindowsPlayer:
            //     case RuntimePlatform.OSXPlayer:
            //     {
            //         // ApplicationPath =  FileProtocol+ Application.dataPath.Replace("\\", "/");
            //         //FileProtocol + Application.streamingAssetsPath.Replace('\\', '/');
            //         streamingAssetsURL = new System.Uri(Application.streamingAssetsPath).ToString();
            //         break;
            //     }
            //     case RuntimePlatform.Android:
            //     {
            //         streamingAssetsURL = new System.Uri(path)
            //         // "jar:" + FileProtocol + Application.dataPath + "!/assets";
            //         break;
            //     }
            //     case RuntimePlatform.IPhonePlayer:
            //     {
            //         streamingAssetsURL = System.Uri.EscapeUriString(FileProtocol + Application.streamingAssetsPath);
            //         break;
            //     }
            //     case RuntimePlatform.WebGLPlayer:
            //     {
            //         
            //         break;
            //     }
            //     default:
            //         Debug.Assert(false);
            //         break;
            // }
            
        }


        /// <summary>
        /// UnityEditor.EditorUserBuildSettings.activeBuildTarget, Can Run in any platform~
        /// </summary>
        // private static string GetEditorUserBuildSetting(string propertyName)
        // {
        //     var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        //     foreach (var a in assemblies)
        //     {
        //         if (a.GetName().Name == "UnityEditor")
        //         {
        //             var lockType = a.GetType("UnityEditor.EditorUserBuildSettings");
        //             //var retObj = lockType.GetMethod(staticMethodName,
        //             //    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
        //             //    .Invoke(null, args);
        //             //return retObj;
        //             var p = lockType.GetProperty(propertyName);
        //
        //             var em = p.GetGetMethod().Invoke(null, new object[] { }).ToString();
        //             return em;
        //         }
        //     }
        //
        //     return null;
        // }

#if UNITY_EDITOR
        public static ResourcePlatform GetPlatformByBuildTarget(UnityEditor.BuildTarget target)
        {
            ResourcePlatform platform = ResourcePlatform.Windows;
            switch (target)
            {
                case UnityEditor.BuildTarget.StandaloneOSX:
                    platform = ResourcePlatform.MacOS;
                    break;
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    platform = ResourcePlatform.Windows;
                    break;
                case UnityEditor.BuildTarget.Android:
                    platform = ResourcePlatform.Android;
                    break;
                case UnityEditor.BuildTarget.iOS:
                    platform = ResourcePlatform.iOS;
                    break;
                case UnityEditor.BuildTarget.WebGL:
                    platform = ResourcePlatform.WebGL;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return platform;
        }
        #endif
        
        private static ResourcePlatform GetPlatform()
        {
            ResourcePlatform platform = ResourcePlatform.Windows;
#if UNITY_EDITOR
            platform = GetPlatformByBuildTarget(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            
            //var buildTarget = GetEditorUserBuildSetting("activeBuildTarget");
            //UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            
#else
            switch (Application.platform)
            {
                case RuntimePlatform.OSXPlayer:
                    platform = ResourcePlatform.MacOS;
                    break;
                case RuntimePlatform.Android:
                    platform = ResourcePlatform.Android;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platform = ResourcePlatform.iOS;
                    break;
                case RuntimePlatform.WindowsPlayer:
                    platform = ResourcePlatform.Windows;
                    break;
                case RuntimePlatform.WebGLPlayer:
                    platform = ResourcePlatform.WebGL;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
#endif
            return platform;
        }
    }
}