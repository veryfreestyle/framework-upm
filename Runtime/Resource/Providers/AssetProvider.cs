// Author: JiangHao <jianghao01@hetao101.com>

using System.Collections;
using UnityEngine;


namespace VeryFS.Framework.Runtime.Resource
{
    public class AssetProvider : ProviderBase
    {
        protected readonly AssetManifest mManifest;

        public AssetProvider(ResourcePackage package,
            string guid, AssetManifest manifest) : base(package, guid)
        {
            mManifest = manifest;
            mLoader = package.GetAssetBundleLoader(manifest.bundleId);
            mLoader.Reference();
        }

        protected override IEnumerator GetLoadRoutine()
        {
            return StartLoad();
        }

        protected virtual IEnumerator StartLoad()
        {
            while (!mLoader.IsDone)
                yield return null;
            // Debug.LogError(mLoader.Status);
            if (mLoader.Status == EStatus.Succeed)
            {
                // var loadAsset = mLoader.Bundle.LoadAssetAsync<Object>(mManifest.path);
                // yield return loadAsset;
                // Debug.Assert(loadAsset.asset != null);
                // this.Result = loadAsset.asset;
                var obj = mLoader.Bundle.LoadAsset<Object>(mManifest.path);
                if (obj != null)
                {
                    Finish(obj);
                }
                else
                {
                    SetError("Load error");
                }
                
            }
            else
            {
                SetError(mLoader.LastError);
            }
        }

        
    }

    
    
}