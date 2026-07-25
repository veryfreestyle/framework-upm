// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Runtime.Resource
{
    public class AssetBundleLoader : Referencable
    {
        // public float Progress => mRequest == null ? 0 : mRequest.downloadProgress;
        public List<AssetBundleLoader> Dependencies { get; } = new();
        public AssetBundle Bundle { get; private set; }
        
        private readonly BundleManifest mInfo;
        private readonly ResourcePackage mOwner;

        public string Name => mInfo.bundleName;

        
        
        public AssetBundleLoader(ResourcePackage package, BundleManifest info)
        {
            mInfo = info;
            mOwner = package;
        }

        /// <summary>
        /// 依赖解析与构造分离：调用方先把本 loader 注册进 mLoaderDict 再调此方法，
        /// 环形依赖时字典已有条目、递归天然终止（否则构造期递归会 StackOverflow）。
        /// 环仍会表现为加载相互死等，属打包错误，可从日志诊断。
        /// </summary>
        internal void ResolveDependencies()
        {
            if (mInfo.dependencies != null)
            {
                foreach (var id in mInfo.dependencies)
                {
                    var dep = mOwner.GetAssetBundleLoader(id);
                    dep.Reference();
                    Dependencies.Add(dep);
                }
            }
        }

        public IEnumerator StartLoad()
        {
            string url = mOwner.GetResourceURL(mInfo.GetBundleFileName(mOwner.BundleNameStyle));
            Debug.Log(this +", Load " + url);
            
            //等待所有依赖加载完成，任一失败则本体失败
            for (int i = 0; i < Dependencies.Count; i++)
            {
                var loader = Dependencies[i];
                while (!loader.IsDone)
                {
                    yield return null;
                }

                if (loader.Status != EStatus.Succeed)
                {
                    SetError($"Dependency error. {loader.Name}: {loader.LastError}");
                    yield break;
                }
            }

            
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                yield return request.SendWebRequest();

                if (!request.isDone)
                {
                    SetError("Cannot get content from an unfinished UnityWebRequest object");
                    yield break;
                }

                if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    SetError(request.error);
                    yield break;
                }

                var handler = (DownloadHandlerAssetBundle)request.downloadHandler;
                Bundle = handler.assetBundle;
                if (Bundle == null)
                {
                    SetError("Invalid AssetBundle." + handler.error);
                }
                else
                {
                    FinishTime = TimeSource.UnixTimeMilliseconds();
                    Status = EStatus.Succeed;
                }
            }
        }


        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        public void Reference()
        {
            //Debug.LogError(this + ", Reference " + RefCount );
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        public void Release()
        {
            RefCount--;

            if (CanDestroy())
            {
                Destroy();
            }
        }

        public override string ToString()
        {
            return $"[AB]{this.Name}";
        }

        public override void Destroy()
        {
            Debug.Log(this +" is destroyed");
            
            IsDestroyed = true;

            if (Bundle != null)
            {
                Bundle.Unload(true);
                Bundle = null;
            }
           
            // Check fatal
            if (RefCount > 0)
                throw new Exception($"Bundle file loader ref is not zero ");
            if (IsDone == false)
                throw new Exception($"Bundle file loader is not done");

            mOwner.RemoveAssetBundleLoader(this);
            
            
            foreach (var dependency in Dependencies)
            {
                dependency.Release();
            }
        }
    }
}