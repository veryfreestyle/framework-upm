using System;

namespace VeryFS.Framework.Runtime.Utilities
{
    public static class TimeSource
    {
        //public static TimeSource Current { get; } = new TimeSource();

        // private TimeSource()
        // {
        // }

        private static int mLastTicks = -1;
        private static DateTime mLastTime = DateTime.Now;

        public static DateTime Time
        {
            get
            {
                int tickCount = Environment.TickCount;
                if (tickCount == mLastTicks)
                    return mLastTime;
                mLastTicks = tickCount;
                mLastTime = DateTime.Now;
                return mLastTime;
            }
        }

        public static long UnixTimeMilliseconds()
        {
            return new DateTimeOffset(Time).ToUnixTimeMilliseconds();
        }

        public static long UnixTime()
        {
            return new DateTimeOffset(Time).ToUnixTimeSeconds();
        }

        /// 返回本地时间（Kind=Local），与 Time 属性同域可直接比较
        public static DateTime FromUnixTime(long t)
        {
            return DateTimeOffset.FromUnixTimeSeconds(t).LocalDateTime;
        }
        
        //protected virtual DateTime NowTime => DateTime.Now;


#if UNITY_EDITOR || UNITY_STANDALONE

        public static int FrameCount => UnityEngine.Time.frameCount;

#else
        private static int mFrameCount = 0;
        public static int FrameCount => mFrameCount;
        
        public static void IncreaseFrameCount()
        {
            mFrameCount++;
        }
#endif

    }
}