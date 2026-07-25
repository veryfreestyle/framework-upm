// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine.SceneManagement;
using System.Collections;

namespace VeryFS.Framework.Runtime.Resource
{
    public class EditorSceneProvider : ProviderBase,ISceneProvider
    {
        protected readonly AssetManifest mManifest;
        public readonly LoadSceneMode mode;
        public bool AllowSceneActivation { get; set; } = true;

        public LoadSceneMode SceneMode { get; }
        
        public EditorSceneProvider(ResourcePackage package, string guid, AssetManifest manifest,
            LoadSceneMode mode) : base(package, guid)
        {
            mManifest = manifest;
            SceneMode = mode;
#if !UNITY_EDITOR
            SetError("Invalid operation");
#endif
        }
#if UNITY_EDITOR
        protected override IEnumerator GetLoadRoutine()
        {
            return StartLoad();
        }

        private IEnumerator StartLoad()
        {
            var parameters = new LoadSceneParameters();
            parameters.loadSceneMode = SceneMode;

            var op=UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(mManifest.path, parameters);
            
            // var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(mManifest.path);
            // if (asset == null)
            // {
            //     SetError("load error");
            //     yield break;
            // }
            //var op = SceneManager.LoadSceneAsync(mManifest.path, LoadSceneMode.Single);
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

            // 与 Player 路径（SceneProvider）保持一致：Result 必须是 Scene，
            // 否则 SceneHandle.Scene 在编辑器模拟模式下必抛
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
#endif
        protected override void OnDestroy()
        {
            // if (Status== EStatus.Succeed && this.Result is UnityEngine.Object  asset)
            // {
            //     //UnityEngine.Resources.UnloadAsset(asset);
            //     UnityEngine.Object.DestroyImmediate(asset, true);
            // }
        }
    }
}