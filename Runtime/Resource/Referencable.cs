// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Runtime.Resource
{
    public abstract class Referencable
    {
        public int RefCount { get; protected set; }
        
        public enum EStatus
        {
            InProgress = 0,
            Succeed,
            Failed
        }

        public EStatus Status { get;protected set;  } = EStatus.InProgress;
        
        public string LastError { protected set; get; } = string.Empty;

        public bool IsDestroyed { get; protected set; } = false;

        // public bool IsDestroying { get; protected set; } = false;

        public bool IsDone => Status != EStatus.InProgress;

        public long CreateTime { get; } = TimeSource.UnixTimeMilliseconds();
        public long FinishTime { get; protected set; }

        public long CostTime => FinishTime - CreateTime;

        protected void SetError(string msg)
        {
            Debug.LogError(this+": "+msg);
            Status = EStatus.Failed;
            LastError = msg;
            FinishTime = TimeSource.UnixTimeMilliseconds();
        }
        
        // public abstract void Update();

        public abstract void Destroy();

        public bool CanDestroy()
        {
            if (IsDestroyed)
                return false;
            
            if (IsDone == false)
                return false;
            
            if (RefCount > 0)
                return false;

            return true;
        }
        

    }
}