// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace VeryFS.Framework.Runtime.UI
{
    public class ViewHolder : IDisposable
    {


        // public static T Create<T>(string viewName, bool fullscreen) where T : ViewHolder, new()
        // {
        //     var view = new T();
        //     if (!view.Initialize())
        //     {
        //         Debug.LogError($"{view.Name}: Initialize failed");
        //         return null;
        //     }
        //     if (fullscreen)
        //         view.FullScreen();
        //     return view;
        // }

        public GComponent component;

        public bool IsShowing { get; private set; }
        public bool IsInitialized => component != null;

        public readonly string pkgName;
        public readonly string resName;
        public string Name { get; private set; }

        public object view;

        // 生命周期回调
        public Action onInit;
        public Action onDestroy;
        public Action onShown;
        public Action onHidden;
        public Action onUpdate;

        public ViewHolder(string viewName)
        {
            (this.resName, this.pkgName) = ParseViewName(viewName);
            Name = resName + "@" + pkgName;
        }

        public bool Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"View '{Name}' is already initialized.");
                return false;
            }

            var package = UIPackage.GetByName(pkgName);

            if (package == null)
            {
                Debug.LogError($"UIPackage '{pkgName}' not found for view '{Name}'");
                return false;
            }

            this.component = CreateGComponent(package, resName);
            if (component == null)
            {
                Debug.LogError($"Failed to create GComponent for view '{Name}'");
                return false;
            }

            this.component.onAddedToStage.Add(OnAddedToStage);
            this.component.onRemovedFromStage.Add(OnRemovedFromStage);

            try
            {
                onInit?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"View '{Name}' onInit failed: {e}");

                // 清理已分配资源
                component.onAddedToStage.Clear();
                component.onRemovedFromStage.Clear();
                component.Dispose();
                component = null;
                return false;
            }
        }

        private void OnAddedToStage()
        {
            IsShowing = true;
            onShown?.Invoke();
        }

        private void OnRemovedFromStage()
        {
            IsShowing = false;
            onHidden?.Invoke();
        }

        public void Dispose()
        {
            if (!IsInitialized)
                return;

            try
            {
                onDestroy?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"View '{Name}' onDestroy error: {e}");
            }

            GRoot.inst.RemoveChild(component);
            component.onAddedToStage.Clear();
            component.onRemovedFromStage.Clear();
            component.Dispose();
            component = null;
            view = null;
            onDestroy = null;
            onInit = null;
            onShown = null;
            onHidden = null;
            onUpdate = null;
        }

        public void FullScreen()
        {
            if (!IsInitialized) return;

            component.SetSize(GRoot.inst.width, GRoot.inst.height);
            component.AddRelation(GRoot.inst, RelationType.Size);
        }

        protected virtual GComponent CreateGComponent(UIPackage package, string resName)
        {
            return package.CreateObject(resName).asCom;
        }

        /// 解析视图名称，支持两种格式：
        /// 1. "ViewName@PackageName"：指定包名和视图名称
        /// 2. "ViewName"：默认包名为 "Main"
        private static (string, string) ParseViewName(string viewName)
        {
            var parts = viewName.Split('@', '.');
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
            else
            {
                return ("Main", viewName);
            }
        }
    }

    /// <summary>
    /// 弹窗宿主，管理 GPopupWindow 生命周期。
    /// 与 ViewHost 的差异：
    /// 1. 创建 GPopupWindow 而非普通 GComponent
    /// 2. 提供 ShowModal() 弹窗显示方法
    /// </summary>
    public class PopupHolder : ViewHolder
    {

        public PopupHolder(string name) : base(name)
        {
        }

        private GPopupWindow window;

        public void ShowModal(bool popupMode = false)
        {
            window.ShowModal(popupMode);
        }

        public void Close()
        {
            window?.Hide();
        }

        public PopupResult result => window.result;

        protected override GComponent CreateGComponent(UIPackage package, string resName)
        {
            window = new GPopupWindow(package, resName);
            return window;
        }

    }


    public class MsgBoxHolder : PopupHolder
    {

        public string title = "TITLE";
        public string prompt = "PROMPT";
        public MsgBoxStyle style = MsgBoxStyle.OkOnly;

        private GMsgBoxWindow msgBoxWindow;

        public MsgBoxHolder(string name) : base(name)
        {
        }

        protected override GComponent CreateGComponent(UIPackage package, string resName)
        {
            msgBoxWindow = new GMsgBoxWindow(package, resName);

            // 追加 onShown 回调来初始化消息框
            onShown += () =>
            {
                // Debug.Log("OnShown " + style);
                msgBoxWindow.result = PopupResult.Cancel;
                msgBoxWindow.prompt = prompt;
                msgBoxWindow.title = title;
                msgBoxWindow.style = style;
            };

            return msgBoxWindow;
        }
    }
}