// Author: JiangHao <jianghao01@hetao101.com>

namespace VeryFS.Framework.Runtime.Utilities
{
    public class FixedFrameScheduler
    {
        public readonly float frameInterval ;
        // private const int FramePerSecond = 30;
        private float mAccumulatedTime;

        public FixedFrameScheduler(int framePerSecond = 10)
        {
            frameInterval =  1f / (float)framePerSecond;;
            mAccumulatedTime = 0f;
        }
        
        public bool ShouldExecute(float deltaTime)
        {
            mAccumulatedTime += deltaTime;
            if (mAccumulatedTime >= frameInterval)
            {
                mAccumulatedTime -= frameInterval;
                if (mAccumulatedTime > frameInterval)
                {
                    // 积压上限一帧：长期低帧率后恢复时补一帧即可，不做爆发式追帧
                    mAccumulatedTime = frameInterval;
                }
                return true; // Time to execute the fixed update logic
            }
            return false; // Not yet time to execute
        }
    }
}