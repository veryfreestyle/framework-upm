// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VeryFS.Framework.Runtime.Resource
{


    public class ResourcePackage
    {

        private readonly Dictionary<string, AssetManifest> mAssetManifestsDict = new();

        // private readonly List<AssetBundleLoader> mLoaderList = new();
        private readonly Dictionary<string, AssetBundleLoader> mLoaderDict = new();


        private readonly Dictionary<string, ProviderBase> mProviderDict = new();

        private readonly List<ProviderBase> mProviderList = new();

        public bool simulationOnEditor;

        public string Name => mManifest.packageName;

        public string BuildVersion => mManifest.buildVersion;

        public long BuildTime => mManifest.buildTime;

        public AssetBundleNameStyle BundleNameStyle => mManifest.style;

        private PackageManifest mManifest;

        public PackageManifest Manifest => mManifest;

        public bool IsEditorMode => Application.isEditor && simulationOnEditor;

        public ResourcePackage(PackageManifest manifest)
        {
            mManifest = manifest;

            if (mManifest.assets != null)
            {
                foreach (var item in mManifest.assets)
                {
                    mAssetManifestsDict.Add(item.address, item);
                }
            }
        }

        // public void UpdateManifest(PackageManifest manifest)
        // {
        //     if (manifest.packageName != Name)
        //     {
        //         Debug.LogError("UpdateManifest error.not same name");
        //         return;
        //     }
        //     //计算需要下载的项目
        //     BundleManifest[] differences = manifest.CalculateDifferences(
        //         this.mManifest);
        //
        //     foreach (var bm in differences)
        //     {
        //         if (mLoaderDict.TryGetValue(bm.bundleName, out var loader))
        //         {
        //             Debug.LogWarning($"Update {Name} Manifest: {loader.Name} already loaded.");
        //         }
        //     }
        //     
        //     mManifest = manifest;
        //     
        //     if (mManifest.assets != null)
        //     {
        //         foreach (var item in mManifest.assets)
        //         {
        //             if (mAssetManifestsDict.TryAdd(item.address, item))
        //             {
        //                 
        //             }
        //         }
        //     }
        // }

        public void Dispose()
        {
            // foreach (var provider in mProviderList)
            // {
            //     Debug.LogError($"try to dispose {Name}: {provider.ID}");
            //     // provider.Destroy();
            // }

            while (mProviderList.Count > 0)
            {
                var provider = mProviderList[0];
                // Debug.LogError($"try to dispose {Name}: {provider.ID}");
                provider.Destroy();
            }
        }


        private AssetManifest FindAssetManifest(string address)
        {
            if (mAssetManifestsDict.TryGetValue(address, out var manifest))
            {
                return manifest;
            }

            return null;
        }

        internal AssetBundleLoader GetAssetBundleLoader(int bundleId)
        {
            var bm = mManifest.GetBundle(bundleId);
            if (mLoaderDict.TryGetValue(bm.bundleName, out var loader))
            {
                return loader;
            }

            loader = new AssetBundleLoader(this, bm);
            mLoaderDict.Add(loader.Name, loader);
            // 先注册后解析依赖，环形依赖时字典命中即终止递归
            loader.ResolveDependencies();
            ResourceDebugger.Create("AssetBundleLoader", loader);

            var runner = ResourceCoroutineRunner.Instance;
            if (runner != null)   // 退出中不再启动加载
            {
                runner.StartCoroutine(loader.StartLoad());
            }

            return loader;
        }

        internal void RemoveAssetBundleLoader(AssetBundleLoader loader)
        {
            mLoaderDict.Remove(loader.Name);
        }

        private void AddProvider(ProviderBase provider)
        {
            ResourceDebugger.Create("Provider", provider);

            this.mProviderDict.Add(provider.ID, provider);
            this.mProviderList.Add(provider);

            // 构造完成后才启动加载协程，避免构造期虚调用（见 ProviderBase.BeginLoad 注释）
            provider.BeginLoad();
        }

        internal void RemoveProvider(ProviderBase provider)
        {
            this.mProviderDict.Remove(provider.ID);
            this.mProviderList.Remove(provider);
        }


        internal string GetResourceURL(string relativePath)
        {
            string url = Name;
            // if (Application.isEditor)
            // {
            //     url += "/" + BuildVersion;
            // }

            url = ResourcePath.GetResourceURL(url + "/" + relativePath);
            return url;
        }

        public async UniTask<AssetHandle> LoadAssetAsync(string address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifest = FindAssetManifest(address);
            if (manifest == null)
            {
                throw new Exception("invalid address: " + address);
            }

            string guid = "[Asset]" + address;
            if (!mProviderDict.TryGetValue(guid, out var provider))
            {
                if (IsEditorMode)
                {
                    provider = new EditorAssetProvider(this, guid, manifest);
                }
                else
                {
                    provider = new AssetProvider(this, guid, manifest);
                }

                AddProvider(provider);

            }

            var handle = provider.CreateHandle<AssetHandle>();
            try
            {
                await handle.ToUniTask(PlayerLoopTiming.Update, cancellationToken);
            }
            catch
            {
                handle.Dispose();   // 取消/失败时归还引用计数
                throw;
            }

            // await 对 Failed 正常完成（MoveNext 只看 IsDone），失败需在此显式引爆
            if (handle.IsError)
            {
                string err = handle.LastError;
                handle.Dispose();
                throw new Exception($"LoadAssetAsync '{address}' failed: {err}");
            }
            return handle;
        }



        public async UniTask<UIPackageHandle> LoadUIPackageAsync(string packageName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!mManifest.bundleMap.TryGetValue(packageName, out var bundleInfo))
            {
                throw new Exception("LoadUIPackageAsync(),invalid address: " + packageName);
            }

            string guid = "[UIPackage]" + packageName;
            if (!mProviderDict.TryGetValue(guid, out var provider))
            {
                if (IsEditorMode)
                {
                    provider = new EditorUIPackageProvider(this, guid, bundleInfo.tag);
                }
                else
                {
                    var loader = GetAssetBundleLoader(bundleInfo.bundleId);

                    provider = new UIPackageProvider(this, guid, loader);
                }

                AddProvider(provider);
            }

            var handle = provider.CreateHandle<UIPackageHandle>();
            try
            {
                await handle.ToUniTask(PlayerLoopTiming.Update, cancellationToken);
            }
            catch
            {
                handle.Dispose();   // 取消/失败时归还引用计数
                throw;
            }

            if (handle.IsError)
            {
                string err = handle.LastError;
                handle.Dispose();
                throw new Exception($"LoadUIPackageAsync '{packageName}' failed: {err}");
            }
            return handle;
        }





        public async UniTask<SceneHandle> LoadSceneAsync(string address,
            bool singleMode = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var mode = singleMode ? LoadSceneMode.Single : LoadSceneMode.Additive;
            var manifest = FindAssetManifest(address);
            if (manifest == null)
            {
                throw new Exception("LoadSceneAsync(),invalid address: " + address);
            }

            string guid = "[Scene]" + address;
            if (!mProviderDict.TryGetValue(guid, out var provider))
            {
                if (IsEditorMode)
                {
                    provider = new EditorSceneProvider(this, guid, manifest, mode);
                }
                else
                {
                    provider = new SceneProvider(this, guid, manifest, mode);
                }

                ((ISceneProvider)provider).AllowSceneActivation = true;
                AddProvider(provider);

            }

            var handle = provider.CreateHandle<SceneHandle>();
            try
            {
                await handle.ToUniTask(PlayerLoopTiming.Update, cancellationToken);
            }
            catch
            {
                handle.Dispose();   // 取消/失败时归还引用计数
                throw;
            }

            if (handle.IsError)
            {
                string err = handle.LastError;
                handle.Dispose();
                throw new Exception($"LoadSceneAsync '{address}' failed: {err}");
            }
            return handle;
        }


        public async UniTask<string> LoadTextAssetTextAsync(string address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string ret = string.Empty;
            try
            {
                var handle = await LoadAssetAsync(address, cancellationToken);
                var asset = handle.Asset as TextAsset;
                if (asset != null)
                {
                    ret = asset.text;
                }
                handle.Dispose();
            }
            catch (OperationCanceledException)
            {
                throw;  // 取消向上传播，不吞成空结果
            }
            catch (Exception e)
            {
                Debug.LogError("LoadTextAssetTextAsync() error. " + e.Message);
            }
            return ret;
        }

        public async UniTask<byte[]> LoadTextAssetDataAsync(string address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] ret = Array.Empty<byte>();
            try
            {
                var handle = await LoadAssetAsync(address, cancellationToken);
                var asset = handle.Asset as TextAsset;
                if (asset != null)
                {
                    ret = asset.bytes;
                }
                handle.Dispose();
            }
            catch (OperationCanceledException)
            {
                throw;  // 取消向上传播，不吞成空结果
            }
            catch (Exception e)
            {
                Debug.LogError("LoadTextAssetDataAsync() error. " + e.Message);
            }
            return ret;
        }

        // public void Update()
        // {
        //
        // }
    }
}