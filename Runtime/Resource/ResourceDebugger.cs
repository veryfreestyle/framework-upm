// Author: JiangHao <jianghao01@hetao101.com>

using UnityEngine;

namespace VeryFS.Framework.Runtime.Resource
{
    /// <summary>
    /// 只在编辑器下出现，分别对应一个Loader~生成一个GameObject对象，为了方便调试！
    /// </summary>
    public class ResourceDebugger : MonoBehaviour
    {
        public Referencable target;
        public int RefCount;
        public long FinishUsedTime; // 参考，完成所需时间
        public static bool IsApplicationQuit = false;

        const string bigType = "ResourceDebugger";

        private string type;

        public static void Create(string type, Referencable loader)
        {
            if (!Application.isEditor) return;   // 调试器只在编辑器存在，守卫收口在此，调用方无需自查
            if (IsApplicationQuit) return;

            //Func<string> getName = () => $"{type}:{loader.url} {loader.desc}";

            var newHelpGameObject = new GameObject(loader.ToString());
            DebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);
            var newHelp = newHelpGameObject.AddComponent<ResourceDebugger>();
            newHelp.target = loader;
            newHelp.type = type;
            newHelp.RefCount = loader.RefCount;


            // loader.setDescEvent += (newDesc) =>
            // {
            //     if (loader.refCount > 0)
            //         newHelpGameObject.name = getName();
            // };
            //
            //
            // loader.disposeEvent += () =>
            // {
            //     if (!IsApplicationQuit)
            //         DebuggerObjectTool.RemoveFromParent(bigType, type, newHelpGameObject);
            // };
        }



        private void Update()
        {
            if (IsApplicationQuit)
                return;

            if (target.RefCount != RefCount)
                RefCount = target.RefCount;
            if (target.CostTime != FinishUsedTime)
                FinishUsedTime = target.CostTime;

            if (target.IsDestroyed)
            {
                target = null;
                DebuggerObjectTool.RemoveFromParent(bigType, type, this.gameObject);
            }


        }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

    }
}