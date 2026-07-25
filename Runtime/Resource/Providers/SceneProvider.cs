// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VeryFS.Framework.Runtime.Resource
{
    public interface ISceneProvider
    {
        LoadSceneMode SceneMode { get; }
        
        /// <summary>
        /// Allow Scenes to be activated as soon as it is ready.
        /// </summary>
        bool AllowSceneActivation { get;set; }
    }
    
    public class SceneProvider : AssetProvider,ISceneProvider
    {
        public LoadSceneMode SceneMode { get; }
        public bool AllowSceneActivation { get; set; } = true;


        public SceneProvider(ResourcePackage package,
            string guid, AssetManifest manifest,
            LoadSceneMode mode) : base(package, guid, manifest)
        {
            SceneMode = mode;
        }

        protected override IEnumerator StartLoad()
        {
            while (!mLoader.IsDone)
                yield return null;
            // Debug.LogError(mLoader.Status);
            if (mLoader.Status != EStatus.Succeed)
            {
                SetError(mLoader.LastError);
                yield break;
            }
            
            // 如果加载的是主场景，则卸载所有缓存的场景
            // if (sceneMode == LoadSceneMode.Single)
            // {
            //     UnloadAllScene();
            // }
            
            Debug.Assert(mLoader.Bundle.isStreamedSceneAssetBundle);
            
            // var abAssetName = mLoader.Bundle.GetAllAssetNames();
            // Debug.LogError("length = " +abAssetName.Length);
            // var request = mLoader.Bundle.LoadAssetAsync<Object>(mManifest.path);
            // yield return request;

            // await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            
            var op = SceneManager.LoadSceneAsync(mManifest.path, this.SceneMode);
            if (op == null)
            {
                SetError("SceneManager.LoadSceneAsync error. " + mManifest.path);
                yield break;
            }
            op.allowSceneActivation = AllowSceneActivation;
            while (!op.isDone)
            {
                yield return null;
            }
            
            
            // 按路径定位刚加载的场景，不能用 GetSceneAt(sceneCount-1) 猜位置——
            // 同帧多个场景加载完成时会张冠李戴
            var scene = SceneManager.GetSceneByPath(mManifest.path);
            if (scene.IsValid())
            {
                Finish(scene);
            }
            else
            {
                SetError("Invalid Scene: " + mManifest.path);
            }
        }


    }
}