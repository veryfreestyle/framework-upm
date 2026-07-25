// Author: JiangHao <jianghao01@hetao101.com>
using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VeryFS.Framework.Runtime.Resource;

namespace VeryFS.Framework.Runtime
{
    public static class ResourceModule
    {
        
        private static readonly List<ResourcePackage> mPackages = new();

        public static ResourcePackage Default { get; private set; }

        public const string DefaultPackageName = "Default";

        public static ResourcePackage GetPackage(string name)
        {
            foreach (var package in mPackages)
            {
                if (package.Name == name)
                    return package;
            }
            return null;
        }

        // public static ResourcePackage GetPackage(string name)
        // {
        //     var package = GetPackageInternal(name);
        //     if (package == null)
        //     {
        //         Debug.LogWarning($"ResourceModule.GetPackage({name}) error.");
        //         return null;
        //     }
        //
        //     return package;
        // }

        public static ResourcePackage AddPackage(
            PackageManifest manifest)
        {
            Debug.Log($"Add ResourcePackage '{manifest.packageName}' {manifest.buildVersion}, {manifest.buildTime}");

            var package = GetPackage(manifest.packageName);
            if (package != null)
            {
                Debug.LogWarning($"ResourceModule.AddPackage() . {manifest.packageName} is existed,replaced");
                mPackages.Remove(package);
                if (Default != null && Default.Name == manifest.packageName)
                {
                    Default = null;
                }
                package.Dispose();
            }

            package = new ResourcePackage(manifest);
            mPackages.Add(package);
            if (Default == null && manifest.packageName == DefaultPackageName)
            {
                Default = package;
            }
            return package;
        }

        public static async UniTask<ResourcePackage> LoadPackageAsync(string packageName=DefaultPackageName)
        {
            string relativePath = packageName+"/"+packageName;
            //加载DefaultPackage
            string url = ResourcePath.GetResourceURL(relativePath,out bool isPersistent);
            var manifest = await GetPackageManifest(url);

            if (isPersistent)
            {
                //检查旧版本资源并清理：包体内置资源比 persistent 热更资源新（用户装了新版 App）时，清掉过期热更文件。
                //StreamingAssets 在 Android 上位于 apk 内（jar:file://），File.Exists 探测不到，
                //统一用 UnityWebRequest 拉取内置 manifest，拉不到视为无内置资源、跳过清理。
                string url2 = ResourcePath.GetStreamingResourceURL(relativePath);
                PackageManifest manifest2 = null;
                try
                {
                    manifest2 = await GetPackageManifest(url2);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"No streaming manifest, skip cleanup. {e.Message}");
                }

                if (manifest2 != null && manifest2.buildTime > manifest.buildTime)
                {
                    Debug.LogWarning(
                        $"Cleanup old resources, current: '{manifest2.buildVersion}', old: '{manifest.buildVersion}'");

                    manifest = manifest2;

                    string rootDir = new Uri(ResourcePath.GetPersistentResourceURL(packageName)).LocalPath;

                    foreach (string dir in Directory.GetDirectories(rootDir))
                    {
                        Directory.Delete(dir, true);
                    }

                    foreach (string file in Directory.GetFiles(rootDir))
                    {
                        File.Delete(file);
                    }
                }
            }

            var package = AddPackage(manifest);
            return package;
        }


        public static async UniTask<PackageManifest> GetPackageManifest(string url)
        {
            Debug.Log("GetPackageManifest: " + url);
            
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 10;
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var data = request.downloadHandler.data;
                    var manifest = PackageManifest.CreateFromData(data);
                    Debug.Log($"PackageManifest: {manifest.packageName}, {manifest.buildVersion}, {manifest.buildTime}");
                    return manifest;
                }
                //Debug.LogError($"GetPackageManifest: [{request.result}] {request.error}");
                throw new Exception($"GetPackageManifest: [{request.result}] {request.error}");
            }
        }
        
        
        public static async UniTask<byte[]> LoadRaw(string relativePath)
        {
            string path = ResourcePath.GetRawResourceURL(relativePath); 
            Debug.Log($"Load '{path}'");
            using (var request = UnityWebRequest.Get(path))
            {
                request.timeout = 10;
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                throw new Exception($"Load '{path}' error. [{request.result}] {request.error}");
            }
        }

    }
}