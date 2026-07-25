using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VeryFS.Framework.Runtime.Resource;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Editor.Resource
{

    // public class BuildAssetInfo
    // {
    //     // private readonly HashSet<string> _referenceBundleNames = new HashSet<string>();
    //     
    //     public string BundleName { get; }
    //     public string Address { get; }
    //     public string AssetPath { get; }
    //     public bool IsAddressBundle { get; set; }
    //
    //     public BuildAssetInfo(string bundleName,string assetPath,string address)
    //     {
    //         this.BundleName = bundleName;
    //         this.AssetPath = assetPath;
    //         this.Address = address;
    //     }
    // }

    // public interface IBuildBundleContext
    // {
    //
    //     void PackAsset(BuildAssetInfo assetInfo);
    //     
    //     bool IsContainsBundle(string bundleName);
    //     
    // }
    
    public class BuildBundleInfo
    {
        
        public string BundleName { get; }
        public readonly List<CollectAssetInfo> MainAssets = new ();

 
        public BuildBundleInfo(string name)
        {
            this.BundleName = name;
        }

        
        
        /// <summary>
        /// 创建AssetBundleBuild类
        /// </summary>
        public UnityEditor.AssetBundleBuild CreatePipelineBuild()
        {
            // 注意：我们不在支持AssetBundle的变种机制
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = BundleName;
            build.assetBundleVariant = string.Empty;
            build.assetNames = MainAssets.Select(t => t.AssetPath).ToArray();
            return build;
        }
        

        public bool IsContainsAsset(string assetPath)
        {
            foreach (var assetInfo in MainAssets)
            {
                if (assetInfo.AssetPath == assetPath)
                {
                    return true;
                }
            }
            return false;
        }

        public void PackAsset(CollectAssetInfo assetInfo)
        {
            if (IsContainsAsset(assetInfo.AssetPath))
                throw new System.Exception($"Should never get here ! Asset is existed : {assetInfo.AssetPath}");

            MainAssets.Add(assetInfo);
        }

        public BundleManifest CreateManifest(string pipelineOutputDirectory)
        {
            
            string outputFilePath = Path.Join(pipelineOutputDirectory, BundleName);

            uint fileCrc=0;
            string fileHash = "";
            long fileSize = 0;
            if (BuildPipeline.GetCRCForAssetBundle(outputFilePath, out uint unityCRC))
            {
                using (FileStream get_file = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileSize = get_file.Length;
                    fileCrc = Utils.CRC32_Stream(get_file);
                    get_file.Seek(0, SeekOrigin.Begin);
                    fileHash = Utils.Hash_Stream(get_file,"SHA1");
                }
            }

            var manifest = new BundleManifest()
            {
                bundleName = BundleName,
                unityCRC = unityCRC,
                fileCRC = fileCrc,
                fileHash = fileHash,
                fileSize = fileSize
            };

            return manifest;
        }
    }


    public class BuildBundleMap 
    {
        private readonly Dictionary<string, BuildBundleInfo> _bundleInfoDic =new (10000);

        // public void PackAsset(string bundleName, string assetPath)
        // {
        //     PackAsset(new BuildAssetInfo(bundleName, assetPath));
        // }
        
        public void PackAsset(CollectAssetInfo assetInfo)
        {
            string bundleName = assetInfo.BundleName;
            if (string.IsNullOrEmpty(bundleName))
                throw new Exception("Should never get here !");
            
            if (!_bundleInfoDic.TryGetValue(bundleName, out var bundleInfo))
            {
                bundleInfo = new BuildBundleInfo(bundleName);
                _bundleInfoDic.Add(bundleName, bundleInfo);
            }
            //Debug.LogError("pack " +bundleInfo.BundleName +", " + assetInfo.AssetPath);
            bundleInfo.PackAsset(assetInfo);
        }

        public bool IsContainsBundle(string bundleName)
        {
            return _bundleInfoDic.ContainsKey(bundleName);
        }


        private AssetBundleBuild[] GetBuildMap()
        {
            List<AssetBundleBuild> bundleBuilds = new();
            foreach (var pair in _bundleInfoDic)
            {
                var build = pair.Value.CreatePipelineBuild();
                bundleBuilds.Add(build);
            }

            return bundleBuilds.ToArray();

        }

        public AssetBundleManifest BuildAssetBundles(
            string pipelineOutputDirectory, ResourceBuildMode mode, BuildTarget target)
        {
            const BuildAssetBundleOptions BundleOption =
                BuildAssetBundleOptions.ChunkBasedCompression |
                BuildAssetBundleOptions.StrictMode |
                //BuildAssetBundleOptions.DisableLoadAssetByFileName | //Disables Asset Bundle LoadAsset by file name.
                BuildAssetBundleOptions
                    .DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.		

            //  BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            //  ;

            BuildAssetBundleOptions options = BundleOption;


            // if (DisableWriteTypeTree)
            //     opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            // if (IgnoreTypeTreeChanges)
            //     opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

            // if (BuildMode == EBuildMode.ForceRebuild)
            //     opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
            //

            if (mode == ResourceBuildMode.ForceRebuild)
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            else if (mode == ResourceBuildMode.DryRunBuild)
                options |= BuildAssetBundleOptions.DryRunBuild;

            var buildMap = GetBuildMap();

            #if ENABLE_ASSETBUNDLE_PROTECTION
            // if (!Directory.Exists(pipelineOutputDirectory))
            //     Directory.CreateDirectory(pipelineOutputDirectory);
            // 密钥缺失时立刻炸：否则打出未加密的 bundle 而运行时按加密解，故障推迟到加载期
            if (string.IsNullOrEmpty(ResourcePath.AssetBundleKey))
                throw new System.InvalidOperationException(
                    "ENABLE_ASSETBUNDLE_PROTECTION 已开启但 ResourcePath.AssetBundleKey 未赋值，" +
                    "打出的 bundle 不会被加密。宿主须在打包前设置密钥。");
            options |= BuildAssetBundleOptions.EnableProtection;
            BuildPipeline.SetAssetBundleEncryptKey(ResourcePath.AssetBundleKey);

            #endif
            var manifest = BuildPipeline.BuildAssetBundles(
                pipelineOutputDirectory, buildMap, options, target);

#if ENABLE_ASSETBUNDLE_PROTECTION
            BuildPipeline.SetAssetBundleEncryptKey(null);
#endif
            Debug.Assert(manifest != null);

            return manifest;
        }

        public bool VerifyBuildResult(AssetBundleManifest unityManifest)
        {
            //验证Bundle
            string[] unityCreateBundles = unityManifest.GetAllAssetBundles();
            string[] mapBundles = _bundleInfoDic.Select(pair => pair.Value.BundleName).ToArray();

            List<string> exceptBundleList1 = unityCreateBundles.Except(mapBundles).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    Debug.LogError($"Found unintended build bundle : {exceptBundle}");
                }
            }

            List<string> exceptBundleList2 = mapBundles.Except(unityCreateBundles).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    Debug.LogError($"Found unintended build bundle 2 : {exceptBundle}");
                }
            }
            return exceptBundleList1.Count == 0 && exceptBundleList2.Count == 0;
        }
        
        public   PackageManifest CreatePackageManifest(
            string pipelineOutputDirectory,string packageName,
            AssetBundleManifest unityManifest,AssetBundleNameStyle style,List<CollectAssetInfo> rawList)
        {
            // UnityEditor.BuildPipeline.GetCRCForAssetBundle()
            

            List<BundleManifest> bundleList = new();
            
            Dictionary<string, int> bundleNameIndexes = new();
            
            foreach (var pair in _bundleInfoDic)
            {
                
                BundleManifest bundle = pair.Value.CreateManifest(pipelineOutputDirectory);
                bundleList.Add(bundle);
                bundleNameIndexes.Add(bundle.bundleName, bundleList.Count - 1);
            }

            List<AssetManifest> assetList = new();
            Dictionary<string, PackageManifest.BundleInfo> bundleAddressMap = new();

            foreach (var bundle in bundleList)
            {
                BuildBundleInfo bundleInfo = _bundleInfoDic[bundle.bundleName];
                int bundleIdx = bundleNameIndexes[bundle.bundleName];
                // Debug.LogError(bundleInfo.BundleName+", "+bundleInfo.MainAssets.Count);
                foreach (var collectAsset in bundleInfo.MainAssets)
                {
                    //Debug.LogError(bundleInfo.BundleName +", "+ collectAsset.AssetPath + ", " + collectAsset.IsAddressBundle);
                    if (collectAsset.IsAddressBundle) //主要是对UI打包
                    {
                        //Debug.LogError(collectAsset.AssetPath);
                        if (!bundleAddressMap.ContainsKey(collectAsset.Address) &&
                            collectAsset.AssetPath.EndsWith("_fui.bytes"))
                        {
                            bundleAddressMap[collectAsset.Address] = new PackageManifest.BundleInfo()
                            {
                                bundleId = bundleIdx,
                                tag = collectAsset.AssetPath.Replace("_fui.bytes", "")
                            };
                        }
                        
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(collectAsset.Address) == false)
                    {
                        var assetManifest = new AssetManifest()
                        {
                            address = collectAsset.Address,
                            // guid = "",//Guid.NewGuid().ToString("N")
                            path = collectAsset.AssetPath,
                            bundleId = bundleIdx,
                        };
                        assetList.Add(assetManifest);
                    }
                }
                
                //计算依赖AB包

                var deps = unityManifest.GetAllDependencies(bundle.bundleName);
                bundle.dependencies = new int[deps.Length];
                for (int i = 0; i < deps.Length; i++)
                {
                    bundle.dependencies[i] = bundleNameIndexes[deps[i]];
                }
            }

            List<RawFileManifest> raws = new();

            //生成Raw格式assetManifest
            foreach (var collectAsset in rawList)
            {
                uint fileCrc=0;
                string fileHash = "";
                long fileSize = 0;
                using (FileStream get_file = new FileStream(collectAsset.AssetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileSize = get_file.Length;
                    fileCrc = Utils.CRC32_Stream(get_file);
                    get_file.Seek(0, SeekOrigin.Begin);
                    fileHash = Utils.Hash_Stream(get_file,"SHA1");
                }

                var rawManifest = new RawFileManifest()
                {
                    address = collectAsset.Address,
                    path = collectAsset.AssetPath,
                    crc = fileCrc,
                    hash = fileHash,
                    size = fileSize
                };
                
                raws.Add(rawManifest);
            }
            
            
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            PackageManifest manifest = new PackageManifest()
            {
                packageName = packageName,
                buildVersion="NULL",
                buildTime = TimeSource.UnixTime(),
                version =  1,
                assets = assetList,
                bundles = bundleList,
                bundleMap = bundleAddressMap,
                raws = raws,
                style = style
            };
            
            return manifest;
        }

        
    }

}