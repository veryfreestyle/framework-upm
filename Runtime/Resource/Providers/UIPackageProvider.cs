// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Resource
{
    
    public abstract class UIPackageProviderBase : ProviderBase
    {
        protected UIPackageProviderBase(ResourcePackage package, string guid) : base(package,guid)
        {
            
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Status == EStatus.Succeed)
            {
                // Editor 退出时 StageEngine.OnApplicationQuit 会先跑 UIPackage.RemoveAllPackages()，
                // 注册表已清空，此时再 RemovePackage 必炸，跳过即可，非脏包残留
                if (FairyGUI.StageEngine.beingQuit)
                    return;

                var pkg = (FairyGUI.UIPackage)this.Result;
                try
                {
                    FairyGUI.UIPackage.RemovePackage( pkg.id );
                }
                catch (Exception e)
                {
                    // RemovePackage 半途失败会在 FairyGUI 注册表残留脏包
                    Debug.LogWarning(e);
                }
            }
        }
    }
    public class UIPackageProvider : UIPackageProviderBase
    {
        
        public UIPackageProvider(ResourcePackage package, string guid, AssetBundleLoader ab) : base(package,guid)
        {
            ab.Reference();
            mLoader = ab;
        }

        protected override IEnumerator GetLoadRoutine()
        {
            return StartLoad();
        }

        private IEnumerator StartLoad()
        {
            while (!mLoader.IsDone)
                yield return null;
            // Debug.LogError(mLoader.Status);
            if (mLoader.Status == EStatus.Succeed)
            {
                try
                {
                    // bundle 生命周期归 AssetBundleLoader 引用计数管，FairyGUI 不得在 RemovePackage 时代卸，
                    // 否则与 ProviderBase.Destroy → mLoader.Release() 形成双重 Unload(true)
                    FairyGUI.UIPackage.unloadBundleByFGUI = false;

                    var obj = FairyGUI.UIPackage.AddPackage(mLoader.Bundle);
                    Debug.Assert(obj != null);
                    Finish(obj);
                }
                catch (Exception e)
                {
                    SetError(e.Message);
                }
            }
            else
            {
                SetError(mLoader.LastError);
            }
        }


    }
}