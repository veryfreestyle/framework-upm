// Author: JiangHao <jianghao01@hetao101.com>

using System.Collections;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Resource
{
    public class EditorAssetProvider : ProviderBase
    {
        private readonly AssetManifest mManifest;

        public EditorAssetProvider(ResourcePackage package, string guid,AssetManifest manifest):base(package,guid)
        {
            mManifest = manifest;
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
            // 对齐 Player 路径的"至少一帧后完成"，避免 Editor 同步完成掩盖帧序依赖问题
            yield return null;

            var path = mManifest.path;
            Debug.Log("Load " + path);

            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj != null)
            {
                Finish(obj);
            }
            else
            {
                SetError("load error." + path);
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
