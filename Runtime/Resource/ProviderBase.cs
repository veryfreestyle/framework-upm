// Author: JiangHao <jianghao01@hetao101.com>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Runtime.Resource
{
    public abstract class ProviderBase : Referencable
    {
        public string ID { get; }
        
        private readonly ResourcePackage mOwner;
        
        protected AssetBundleLoader mLoader;

        // public IResourceLocation Location { get; }
        private readonly List<ResourceHandle> mHandles = new();
        
        public object Result { get; protected set; }

        protected void Finish(object obj)
        {
            Result = obj;
            Status = EStatus.Succeed;
            FinishTime = TimeSource.UnixTimeMilliseconds();
        }

        protected ProviderBase(ResourcePackage package, string guid)
        {
            ID = guid;
            mOwner = package;
            Debug.Log($"[{ID}] is created");
        }

        private Coroutine mLoadRoutine;

        /// <summary>
        /// 启动加载协程。必须在构造完成后由外部调用（AddProvider），
        /// 不能在构造函数里启动——StartCoroutine 会同步执行到第一个 yield，
        /// 虚方法在派生类字段就绪前被调用（曾导致 SceneProvider.SceneMode 读到 default）。
        /// </summary>
        internal void BeginLoad()
        {
            var routine = GetLoadRoutine();
            if (routine != null)
            {
                var runner = ResourceCoroutineRunner.Instance;
                if (runner == null)   // 应用退出中，单例不再复活
                {
                    SetError("App is quitting, load aborted.");
                    return;
                }
                mLoadRoutine = runner.StartCoroutine(routine);
            }
        }

        /// <summary>加载协程；同步完成的 provider（Editor 系）返回 null。</summary>
        protected virtual IEnumerator GetLoadRoutine()
        {
            return null;
        }

        public override string ToString()
        {
            return ID;
        }

        public T CreateHandle<T>() where T : ResourceHandle, new()
        {
            var handle = new T();
            handle.SetProvider(this);
            this.RefCount++;
            this.mHandles.Add(handle);
            return handle;
        }

        public void ReleaseHandle(ResourceHandle handle)
        {
            if (RefCount <= 0)
                Debug.LogWarning("Asset provider reference count is already zero. There may be resource leaks !");

            if (mHandles.Remove(handle) == false)
                throw new System.Exception("Should never get here !");
            RefCount--;
            if (CanDestroy())
                Destroy();
        }
        
        
        
        protected virtual  void OnDestroy()
        {
            
        }
        
        public override void Destroy()
        {
            Debug.Log($"{ID} is destroyed");
            IsDestroyed = true;

            // 加载中被销毁（如 package.Dispose 强制清理）：停协程并置 Failed，
            // 否则协程下一帧读已置空的 mLoader 抛 NRE，Status 永停 InProgress，
            // 所有 await handle 的调用方永久挂起。
            if (mLoadRoutine != null)
            {
                var runner = ResourceCoroutineRunner.Instance;
                if (runner != null)   // 退出中 runner 已随场景销毁，协程无需手动停
                {
                    runner.StopCoroutine(mLoadRoutine);
                }
                mLoadRoutine = null;
            }
            if (!IsDone)
            {
                SetError("Provider destroyed while loading.");
            }

            OnDestroy();
            Result = null;
            mLoader?.Release();
            mLoader = null;
            mOwner.RemoveProvider(this);
        }
    }
    
    
}