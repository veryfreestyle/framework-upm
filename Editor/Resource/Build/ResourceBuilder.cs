
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using VeryFS.Framework.Runtime.Resource;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Editor.Resource
{

    
    
    public enum ResourceBuildMode
    {
        DefaultBuild = 0,
        ForceRebuild,
        DryRunBuild,
    }
    
    // IncrementalBuild,
    
    public  class ResourceBuilder
    {

        public BuildTarget BuildTarget
        {
            get
            {
                switch (Platform)
                {
                    case ResourcePlatform.MacOS:
                        return BuildTarget.StandaloneOSX;
                    case ResourcePlatform.Android:
                        return BuildTarget.Android;
                    case ResourcePlatform.iOS:
                        return BuildTarget.iOS;
                    case ResourcePlatform.WebGL:
                        return BuildTarget.WebGL;
                    default:
                        return BuildTarget.StandaloneWindows;
                }
            }
        }


        public string OutputPath { get; private set; }

        public string CachePath { get; private set; }
        
        public string RawPath { get; private set; }

        // 本次 build 实际写进 manifest 的版本号；仅内存暴露给 UI 显示，不回写 tracked 的 .asset
        public string LastBuildVersion { get; private set; }

        private readonly string mOutputDir;

        // public static string StreamingAssetsPath => "Assets/StreamingAssets/" + ResourceModule.BundlesDirName;


        public ResourceBuildMode Mode { get; set; } = ResourceBuildMode.DefaultBuild;

        private ResourcePackageBuildSettings mSettings;

        public ResourcePackageBuildSettings Settings
        {
            get => mSettings;
            set
            {
                if (value != mSettings)
                {
                    mSettings = value;
                    Setup();
                }
            }
        }

        private ResourcePlatform _platform = ResourcePlatform.Windows;

        public ResourcePlatform Platform
        {
            get => _platform;
            set
            {
                if (_platform != value)
                {
                    _platform = value;
                    Setup();
                }
            }
        }


        public ResourceBuilder(string outputDir)
        {
            mOutputDir = Path.GetFullPath(outputDir);
            Setup();
        }


        private void Setup()
        {
            if (Settings != null)
            {
                OutputPath = Path.Join(mOutputDir,"Bundles/"+Platform , Settings.PackageName);
                CachePath = Path.Join(mOutputDir, "BundleBuildCache/"+Platform, Settings.PackageName);
                RawPath = Path.Join(mOutputDir,"Bundles/Raw" , Settings.PackageName);
                if (File.Exists(OutputPath))
                {
                    throw new System.Exception("路径配置错误");
                }
            }
            else
            {
                OutputPath = "";
                RawPath = "";
                CachePath = "";//Path.Join(OutputPath, "CacheBundleBuild");
            }
        }

        // private string oldPackagePath;
        
        public  void PerformBuild()
        {
            Debug.Log($"PerformBuild: {Mode}, {Platform}, {Settings.BundleNameStyle}");
            
            List<CollectAssetInfo> outputList = new();
            mSettings.CollectAssets(outputList);

            //AssetPath去重
            Dictionary<string, CollectAssetInfo> collectAssetDict = new();
            List<CollectAssetInfo> collectAssetList = new();
            foreach (var info in outputList)
            {
                if (!collectAssetDict.TryAdd(info.AssetPath, info))
                {
                    Debug.LogError("Skip repeat asset: " + info.AssetPath);
                }
                else
                {
                    collectAssetList.Add(info);
                }
            }

            var buildMap = new BuildBundleMap();
            List<CollectAssetInfo> rawList = new();
            foreach (var assetInfo in collectAssetList)
            {
                //Debug.LogError(assetInfo.AssetPath);
                if (!assetInfo.isRaw)
                    buildMap.PackAsset(assetInfo);
                else
                    rawList.Add(assetInfo);
                    // Debug.LogError(assetInfo.AssetPath + ","+assetInfo.isRaw);
            }
            
            EditorTools.CreateDirectory(OutputPath);
            EditorTools.CreateDirectory(CachePath);
            // EditorTools.CreateDirectory(RawPath);
            // string cachePath = Path.GetFullPath($"Product/CacheBundles/{platform}");
            //EmptyDirectory(outputPath);

            var unityManifest = buildMap.BuildAssetBundles(CachePath, Mode, BuildTarget);
            
            
            
            if (unityManifest == null)
            {
                Debug.LogError("BuildAssetBundles error.");
                return;
            }
            
            if (buildMap.VerifyBuildResult(unityManifest))
            {
                var manifest = buildMap.CreatePackageManifest(CachePath, mSettings.PackageName, 
                    unityManifest,Settings.BundleNameStyle,rawList);
                FinishBuild(manifest);
            }
        }

        private void FinishBuild(PackageManifest manifest )
        {
            if (this.Mode == ResourceBuildMode.DryRunBuild)
                return;

            //复制和处理Raw文件

            RawFileManifest[] rawDiffs = null;
            BundleManifest[] diffs = null;
            string oldVersion = "";
            if (Directory.Exists(OutputPath))
            {
                try
                {
                    string packageFile = Path.Join(OutputPath, mSettings.PackageName);
                    if (File.Exists(packageFile))
                    {
                        using (var fs = new FileStream(packageFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var cache = new byte[fs.Length];
                            fs.Read(cache, 0, (int)fs.Length);
                            var oldManifest = PackageManifest.CreateFromData(cache);

                            //比较差异
                            diffs = manifest.CalculateBundleDifferences(oldManifest);
                            rawDiffs = manifest.CalculateRawDifferences(oldManifest);
                            if (this.Mode != ResourceBuildMode.ForceRebuild &&
                                (diffs == null || diffs.Length == 0) && (rawDiffs==null || rawDiffs.Length==0))
                            {
                                Debug.LogWarning("没有变化，打包跳过更新");
                                return;
                            }

                            oldVersion = oldManifest.buildVersion;
                        }
                        
                        string oldPackagePath = Path.Join(mOutputDir,
                            $"{Settings.PackageName}_{oldVersion}_{Platform}");

                        Debug.Log("备份旧版本到" + oldPackagePath);
                        Directory.Move(OutputPath, oldPackagePath);
                        EditorTools.CreateDirectory(OutputPath);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }


            manifest.style = mSettings.BundleNameStyle;
            // 版本号仅算给 manifest，不回写 tracked 的 .asset（否则每次编译都改 asset，产生无谓 git 改动）
            LastBuildVersion = Settings.AutoUpdateVersion
                ? ResourcePackageBuildSettings.FormatVersion(DateTime.Now)
                : mSettings.BuildVersion;
            manifest.buildVersion = LastBuildVersion;

            string outputDir = OutputPath;// Path.Join(OutputPath, Settings.BuildVersion);
            string[] files = Directory.GetFiles(outputDir);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            // if (!EditorTools.CreateDirectory(outputDir))
            // {
            //     EditorTools.EmptyDirectory(outputDir, new []{"CacheBuild"});
            // } 

            
            foreach (var bundle in manifest.bundles)
            {
                string srcFile = Path.Join( CachePath,  bundle.bundleName);
                string destFile = bundle.GetBundleFileName(Settings.BundleNameStyle);
                destFile = Path.Join(outputDir, destFile);
                EditorTools.CopyFile(srcFile, destFile, false);
            }

            //复制Raw文件
            if (!EditorTools.CreateDirectory(RawPath))
            {
                EditorTools.EmptyDirectory(RawPath);
            }
            
            foreach (var raw in manifest.raws)
            {
                string destFile = Path.Join(RawPath, raw.address);

                char[] delimiterChars = { '/', '\\' };
                var ss = raw.address.Split(delimiterChars);
                if (ss.Length > 1)
                {
                    string subDir = "";
                    for (int i =0; i < ss.Length-1; i++)
                    {
                        subDir += ss[i] + "/";
                        string s = Path.Join(RawPath, subDir);
                        // Debug.Log(s);
                        EditorTools.CreateDirectory(s);
                    }
                }
                Debug.Log($"copy {raw.path} to {destFile}");
                EditorTools.CopyFile(raw.path,destFile,true);
            }
            
            string path = Path.Join(outputDir, mSettings.PackageName);
            
            Utils.ToJsonFile(path + ".json",manifest,true);
            manifest.WriteToFile(path);

            if (!string.IsNullOrEmpty(oldVersion) )
            {
                string str = $"scp Bundles/{Platform}/{mSettings.PackageName}/";
                str += "{0}";
                str += $" game:~/www/game/1.0.0/Bundles/{Platform}/{mSettings.PackageName}/\n";
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(str,mSettings.PackageName);
                sb.AppendFormat(str,mSettings.PackageName +".json");
                
                if (diffs != null && diffs.Length > 0)
                {
                    foreach (var diff in diffs)
                    {
                        sb.AppendFormat(str,diff.GetBundleFileName(manifest.style));
                    }
                }

                if (rawDiffs != null && rawDiffs.Length > 0)
                {
                    str = $"scp Bundles/Raw/{mSettings.PackageName}/";
                    str += "{0}";
                    str += $" game:~/www/game/1.0.0/Bundles/Raw/{mSettings.PackageName}/\n";
                    
                    foreach (var diff in rawDiffs)
                    {
                        sb.AppendFormat(str,diff.address);
                    }
                }

                File.WriteAllText(Path.Join(outputDir, "upload_diff") + ".sh", sb.ToString());
                
                string diffFile = Path.Join(outputDir,"diff_"+oldVersion) + ".json";
                Utils.ToJsonFile(diffFile,diffs,true);
                
                string diffFile2 = Path.Join(outputDir,"diff_raw_"+oldVersion) + ".json";
                Utils.ToJsonFile(diffFile2,rawDiffs,true);
            }
        }
    }
}