// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine;

namespace VeryFS.Framework.Runtime.Utilities
{
    /// <summary>
    /// 非泛型守卫：RuntimeInitializeOnLoadMethod 不会在泛型类上触发，
    /// 退出标记与跨会话重置收口在此（Enter Play Mode Options 关闭 Domain Reload 时静态字段不重置）。
    /// </summary>
    internal static class MonoSingletonGuard
    {
        internal static bool AppQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            AppQuitting = false;
        }
    }

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // 退出期间不复活单例：返回 null，调用方需判空
                if (_instance == null && !MonoSingletonGuard.AppQuitting)
                {
                    var inst = (T)FindObjectOfType(typeof(T));
                    if (inst == null)
                    {
                        GameObject gameObject = new GameObject(typeof(T).Name);
                        inst = gameObject.AddComponent<T>();
                    }

                    _instance = inst;
                }

                return _instance;
            }
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // 场景中已有实例，销毁当前对象
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }

                return;
            }

            _instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);

            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        private void OnApplicationQuit()
        {
            MonoSingletonGuard.AppQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
