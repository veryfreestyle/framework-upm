/** 
* Scorpio
* @author JiangHao 
* @date 2019.3
*/

using System;
using System.Collections.Generic;

namespace VeryFS.Framework.Runtime.Utilities
{
    public abstract class PooledObject
    {
        internal IObjectPool objectPool;
        
        public long instanceID;
        
        // private int mRefCount;
        // public int RefCount => mRefCount;

        
        public virtual void OnCreate()
        {
            
        }

        public virtual void OnRelease()
        {
            
        }

        /// <summary>
        /// Releases pool item back into pool
        /// </summary>
        public void Release()
        {
            objectPool?.Release(this);
        }
        

        // public void IncRef()
        // {
        //     mRefCount++;
        // }

        
    }

    public interface IObjectPool
    {
        void Release(PooledObject obj);
    }

    
    
    /// <summary>
    /// 对象池。仅主线程使用——静态懒初始化与内部队列均无锁。
    /// </summary>
    public class ObjectPool<T> : IObjectPool where T : PooledObject, new()
    {
        private static ObjectPool<T> sInstance = null;

        public static T Acquire()
        {
            if (sInstance == null)
            {
                sInstance = new ObjectPool<T>();
            }
            return sInstance.DoAcquire();
        }
        
        protected readonly Queue<PooledObject> mFreelist = new Queue<PooledObject>(64);

        protected  int mInstanceCount;

        public static  int FreeCount => sInstance?.mFreelist.Count ?? 0;

        public static int InstanceCount => sInstance?.mInstanceCount ?? 0;

        protected virtual T DoAcquire() 
        {
            T obj ;
            if (mFreelist.Count > 0)
            {
                obj = mFreelist.Dequeue() as T;
            }
            else
            {
                obj = new T {instanceID = ++mInstanceCount};
            }

            obj.objectPool = this;
            obj.OnCreate();
            return obj;
        }

        public virtual void Release(PooledObject obj)
        {
            obj.OnRelease();
            obj.objectPool = null;
            mFreelist.Enqueue(obj);
        }

        public static void Cleanup(Action<T> action)
        {
            if (sInstance == null || sInstance.mFreelist.Count==0)
                return;
            while (sInstance.mFreelist.TryDequeue(out var obj))
            {
                action(obj as T);
            }
        }
    }
}