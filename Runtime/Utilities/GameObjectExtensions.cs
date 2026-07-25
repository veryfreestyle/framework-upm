using UnityEngine;

namespace VeryFS.Framework.Runtime.Utilities
{
    public static class GameObjectExtensions
    {
        // public static GameObject FindChild(string rootName,string name)
        // {
        //     GameObject parent= GameObject.Find(rootName);
        //     if (parent!=null)
        //     {
        //         Transform trans=parent.transform.Find(name);
        //         if (trans!=null)
        //         {
        //             return trans.gameObject;
        //         }
        //     }
        //     return null;
        // }
        
        public static void RemoveChild(this GameObject me, string name)
        {
            Transform trans=  me.transform.Find(name);
            if (trans != null)
            {
                trans.SetParent(null);
                GameObject.DestroyImmediate(trans.gameObject);
            }
        }
        
        public static void AsChild(this GameObject child, GameObject parent, bool selfRotation = false,
            bool selfScale = false)
        {
            AsChild(child.transform, parent.transform, selfRotation, selfScale);
        }

        public static void AsChild(this Transform child, Transform parent, bool selfRotation = false,
            bool selfScale = false)
        {
            child.SetParent(parent);
            child.Reset(selfRotation, selfScale);
        }
    }
}