// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Utilities
{
    public  static class TransformExtensions
    {
        public static void Reset(this Transform transform, bool selfRotation = false,
            bool selfScale = false)
        {
            transform.localPosition = Vector3.zero;
            if (!selfRotation)
                transform.localEulerAngles = Vector3.zero;

            if (!selfScale)
                transform.localScale = Vector3.one;
        }
        
        
        public static Transform FindRecursive(this Transform trans, string name)
        {
            if (trans.name == name)
            {
                return trans;
            }

            for (int i = 0; i < trans.childCount; i++)
            {
                Transform transform = trans.GetChild(i).FindRecursive(name);
                if (transform != null)
                {
                    return transform;
                }
            }

            return null;
        }

        public static Transform[] GetChildrenRecursive(this Transform trans,string startWith=null)
        {
            List<Transform> list = new List<Transform>();
            _GetChildrenRecursive(list,trans,false ,startWith);
            list.Sort((a, b) =>
            {
                return String.Compare(a.name, b.name, StringComparison.Ordinal);
            });
            return list.ToArray();
        }
        
        private static void _GetChildrenRecursive(
            List<Transform> list,Transform parent,bool includeParent, string startWith)
        {
            if (includeParent) list.Add(parent);
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (string.IsNullOrEmpty(startWith) || child.name.StartsWith(startWith))
                {
                    _GetChildrenRecursive(list, child, true, startWith);
                }
            }
        }

        public static void SetLayer(this Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
            {
                SetLayer(child, layer);
            }
        }
    }
    
    
    
}