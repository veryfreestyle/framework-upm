// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace VeryFS.Framework.Runtime.Resource
{
    public class ResourceHandle : IEnumerator, IDisposable
    {
        protected ProviderBase Provider { get; private set; }

        public bool IsDone => Provider.IsDone;

        // public float Progress => Provider.Progress;

        public bool IsError => Provider.Status == Referencable.EStatus.Failed;

        public string LastError => Provider.LastError;

        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        void IEnumerator.Reset()
        {
        }

        object IEnumerator.Current => null;

        internal void SetProvider(ProviderBase p)
        {
            Provider = p;
        }

        private bool mDisposed;

        public virtual void Dispose()
        {
            if (mDisposed)
                return;
            mDisposed = true;
            Provider.ReleaseHandle(this);
        }
    }


    public class AssetHandle : ResourceHandle
    {
        public Object Asset
        {
            get
            {
                if (Provider.Result is Object obj)
                {
                    return obj;
                }
                return null;
            }
        }

        public GameObject Instantiate()
        {
            if (Provider.Result is GameObject prefab)
            {
                return Object.Instantiate(prefab);
            }

            return null;
        }
    }

    public class UIPackageHandle : ResourceHandle
    {
        public UIPackage Package
        {
            get
            {
                if (this.Provider.Result is UIPackage up)
                {
                    return up;
                }

                throw new Exception("UIPackageHandle.Package error");
            }
        }

        public GObject CreateObject(string resName)
        {
            return Package.CreateObject(resName);
        }
    }


    public class SceneHandle : ResourceHandle
    {
        public Scene Scene
        {
            get
            {
                if (this.Provider.Result is Scene up)
                {
                    return up;
                }
                throw new Exception("SceneHandle.Scene error." + this.Provider.Result);
            }
        }


        /// <summary>
        /// 激活场景（当同时存在多个场景时用于切换激活场景）
        /// </summary>
        public bool ActivateScene()
        {
            return SceneManager.SetActiveScene(Scene);
        }

        public bool IsMainScene()
        {
            if (Provider is ISceneProvider p)
            {
                return p.SceneMode == LoadSceneMode.Single;
            }

            return false;
        }

        public async UniTask UnloadAsync()
        {
            if (IsMainScene())
                throw new Exception("Can't unload main scene.");

            if (Provider.Result is Scene scene && scene.isLoaded)
                await SceneManager.UnloadSceneAsync(scene).ToUniTask();

            base.Dispose();   // 场景已卸，释放 provider/bundle，缓存随之失效
        }

        private bool mUnloadScheduled;

        public override void Dispose()
        {
            if (mUnloadScheduled)
                return;   // 卸载已在途，base.Dispose 由回调触发

            // 兜底：场景还在层级时直接释放 bundle 会撕毁在用资源。
            // 模式匹配天然跳过加载中/失败/已强销（Result 非 Scene）的 handle。
            if (Provider.Result is Scene scene && scene.isLoaded)
            {
                if (IsMainScene())
                {
                    // 主场景无法主动卸载（Unity 禁止卸最后一个场景），只能警告后放行
                    Debug.LogWarning($"SceneHandle('{scene.path}') disposed while main scene active; switch scene before disposing.");
                    base.Dispose();
                    return;
                }

                Debug.LogWarning($"SceneHandle('{scene.path}') disposed without UnloadAsync; unloading first.");
                mUnloadScheduled = true;
                SceneManager.UnloadSceneAsync(scene).completed += _ => base.Dispose();
                return;
            }
            base.Dispose();
        }
    }


}