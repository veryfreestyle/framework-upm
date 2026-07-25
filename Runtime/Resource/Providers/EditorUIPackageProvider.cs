// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Resource
{
    public class EditorUIPackageProvider : UIPackageProviderBase
    {
        private readonly string mPath;

        public EditorUIPackageProvider(ResourcePackage package,
            string guid, string path) : base(package,guid)
        {
            mPath = path;
#if !UNITY_EDITOR
            SetError("Invalid Operation");
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

            try
            {
                var obj = FairyGUI.UIPackage.AddPackage(mPath,
                    (string name, string extension, System.Type type,
                        out FairyGUI.DestroyMethod destroyMethod) =>
                    {
                        destroyMethod = FairyGUI.DestroyMethod.Unload;
                        return UnityEditor.AssetDatabase.LoadAssetAtPath(
                            name + extension, type);
                    });

                Debug.Assert(obj != null);
                Finish(obj);
            }
            catch (Exception e)
            {
                SetError(e.Message);
            }
        }
#endif
    }
}
