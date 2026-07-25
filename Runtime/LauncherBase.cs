// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if USE_URP
using UnityEngine.Rendering.Universal;
#endif
using FairyGUI;
using VeryFS.Framework.Runtime.Resource;
using VeryFS.Framework.Runtime.UI;

namespace VeryFS.Framework.Runtime
{
    public abstract class LauncherBase : MonoBehaviour
    {
        public static bool isPaused { get; private set; }
        public static bool started { get; protected set; }
        public static bool isRootUser { get; private set; }


        public bool loadEditorAsset;

        // 测试有无写权限
        protected static bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                string tmpFilePath = Path.Combine(folderPath, Path.GetRandomFileName());
                using (
                    FileStream fs = new FileStream(tmpFilePath, FileMode.CreateNew, FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write("1");
                }

                File.Delete(tmpFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }



        // private readonly FPSProfiler    mUpdateProfiler = new ("Update");
        // private readonly MemoryProfiler mMemProfiler    = new ();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            isRootUser = HasWriteAccessToFolder(Application.dataPath);

            // #if USE_DEBUG
            //             mUpdateProfiler.enable = true;
            //             mMemProfiler.enable = true;
            //             transform.Find("Reporter").gameObject.SetActive(true);
            // #else
            //             mUpdateProfiler.enable=false;
            //             mMemProfiler.enable = false;
            // #endif

#if DEBUG
            transform.Find("Reporter").gameObject.SetActive(true);
#else
            transform.Find("Reporter").gameObject.SetActive(false);
#endif
            Debug.Log("==================================================================");
            // Debug.Log($"Version = {AppDefines.Version}");
            Debug.Log($"Platform = {UnityEngine.Application.platform}");
            Debug.Log($"DeviceModel = {SystemInfo.deviceModel}");
            Debug.Log($"DeviceUniqueIdentifier = {SystemInfo.deviceUniqueIdentifier}");
            //if (Debug.isDebugBuild)

            {
                //canWritePersistentData = Utils.HasWriteAccessToFolder(UnityEngine.Application.persistentDataPath);
#if DEBUG
                Debug.Log($"DataPath = {UnityEngine.Application.dataPath} , WritePermission: {isRootUser}");
                Debug.Log($"StreamingAssetsPath = {UnityEngine.Application.streamingAssetsPath} , " +
                          $"WritePermission: {HasWriteAccessToFolder(UnityEngine.Application.streamingAssetsPath)}");
                Debug.Log($"PersistentDataPath = {UnityEngine.Application.persistentDataPath} , " +
                          $"WritePermission: {HasWriteAccessToFolder(UnityEngine.Application.persistentDataPath)}");
                Debug.Log($"TemporaryCachePath = {UnityEngine.Application.temporaryCachePath} , " +
                          $"WritePermission: {HasWriteAccessToFolder(UnityEngine.Application.temporaryCachePath)}");
#endif
                Debug.Log($"UnityVersion = {UnityEngine.Application.unityVersion}");
                Debug.Log($"GraphicsDeviceVersion = {SystemInfo.graphicsDeviceVersion}");
                //Log.Info("==================================================================");
            }

            // if (!Application.isEditor)
            // {
            //     Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // }
        }

        public static SystemLanguage Language { get; private set; }

        public static void SetLanguage(SystemLanguage language)
        {
            Language = language;
            // VeryFacade.Instance.SendNotification(AppEvent.LANGUAGE_CHANGED, language);
        }

        /// <summary>
        /// 设置目标帧率，0表示与显示器垂直同步
        /// </summary>
        /// <param name="fps"></param>
        public static void SetTargetFrameRate(int fps)
        {
            if (fps <= 0)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = fps;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            SetApplicationPaused(!hasFocus);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            SetApplicationPaused(pauseStatus);
        }



        protected void SetApplicationPaused(bool value)
        {
            if (started && isPaused != value)
            {
                isPaused = value;
                if (isPaused)
                {
                    // VeryFacade.Instance.SendNotification(AppEvent.APP_PAUSE);
                    OnAppPause();
                }
                else
                {
                    // VeryFacade.Instance.SendNotification(AppEvent.APP_RESUME);
                    OnAppResume();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (!started)
            {
                return;
            }
            started = false;
            // VeryFacade.Instance.SendNotification(AppEvent.APP_SHUTDOWN);
            OnAppQuit();
        }

        protected abstract void OnAppPause();

        protected abstract void OnAppResume();

        protected virtual void OnAppQuit()
        {
            // 释放所有UI资源，包括卸载所有UI包和清理ViewRouter。
            ViewRouter.Instance.Dispose();
            UIModule.UnloadAllUIPackages();
            ResourceModule.Default.Dispose();
        }

        //URP下使用
        public static void InitStageCamera()
        {
#if USE_URP
            StageCamera.CheckMainCamera();
            var stageCamera = FairyGUI.StageCamera.main;
            Debug.Assert(stageCamera != null);
            var cameraData = stageCamera.GetUniversalAdditionalCameraData();
            cameraData.renderType = CameraRenderType.Overlay;
            if (Camera.main != null)
            {
                Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(stageCamera);
            }
            else
            {
                Debug.LogError("InitStageCamera(): Can't find MainCamera");
            }
#endif
        }








    }
}