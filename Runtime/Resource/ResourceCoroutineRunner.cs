// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Runtime.Resource
{
    internal class ResourceCoroutineRunner : MonoSingleton<ResourceCoroutineRunner>
    {
        protected override void OnAwake()
        {
            // Don't show in scene hierarchy
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}