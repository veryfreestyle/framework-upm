// Author: JiangHao <jianghao01@hetao101.com>

using System;

namespace VeryFS.Framework.Runtime.Utilities
{
    public class Singleton<T> where T : class, new()
    {
        private static T s_instance = null;

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    CreateInstance();
                }
                return s_instance;
            }
        }

        public static T GetInstance()
        {
            if (s_instance == null)
            {
                CreateInstance();
            }
            return s_instance;
        }

        protected Singleton()
        {
        }


        public static void CreateInstance()
        {
            if (s_instance == null)
            {
                s_instance = Activator.CreateInstance<T>();
                (s_instance as Singleton<T>)?.OnCreate();
            }
        }

        public static void DestroyInstance()
        {
            if (s_instance != null)
            {
                (s_instance as Singleton<T>)?.OnDestory();
                s_instance = null;
            }
        }


        public virtual void OnCreate()
        {

        }

        public virtual void OnDestory()
        {

        }
    }
}
