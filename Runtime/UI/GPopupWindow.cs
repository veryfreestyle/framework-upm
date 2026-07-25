
using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace VeryFS.Framework.Runtime.UI
{
    public enum PopupResult
    {
        Cancel = 0,
        Ok = 1,
        Yes = 2,    // ✅ 改为 2
        No = 3,
        Retry = 4,
    }

    /// <summary>
    /// GPopupWindow 是一个继承自 FairyGUI.Window 的弹窗窗口类，提供了显示和隐藏动画、模态显示、按钮结果处理等功能。
    /// 该类可以用于创建具有自定义动画和按钮行为的弹窗窗口。
    /// </summary>
    internal class GPopupWindow : Window
    {
        public Action<GPopupWindow> onShowAnimation;
        public Action<GPopupWindow> onHideAnimation;
        // private static List<GPopupWindow> _popupStack = new List<GPopupWindow>();

        public PopupResult result;

        // public static void Initialize()
        // {
        //     _popupStack = new List<GPopupWindow>();
        //     // Stage.inst.onClick.AddCapture( _onStageClick); ????
        // }

        // private static void _onStageClick(EventContext context)
        // {
        //     if (_popupStack.Count <= 0)
        //         return;
        //     var top = _popupStack[_popupStack.Count - 1] as GPopupWindow;
        //     var displayObject = Stage.inst.touchTarget;
        //     for (;
        //         displayObject != Stage.inst && displayObject != null;
        //         displayObject = (DisplayObject)displayObject.parent)
        //     {
        //         var popup = displayObject.gOwner != null ? displayObject.gOwner as GPopupWindow : null;
        //         if (popup == top)
        //         {
        //             return;
        //         }
        //     }

        //     top.Hide();
        // }

        public GPopupWindow(UIPackage package, string resName)
        {
            this.contentPane = package.CreateObject(resName).asCom;
            this.onHideAnimation = null;
            this.onShowAnimation = DefaultDoShowAnimation;
        }

        protected override void OnInit()
        {
            var btn = this.contentPane.GetChild("closeButton");
            if (btn != null)
            {
                closeButton = btn;
            }
        }

        public void LinkButtonResult(string buttonName, PopupResult result)
        {
            var com = this.contentPane.GetChild(buttonName);
            var btn = com?.asButton;
            btn?.onClick.Add(() =>
            {
                this.result = result;
                this.Hide();
            });
        }

        public void CloseWindow(PopupResult result = PopupResult.Cancel)
        {
            this.result = result;
            this.Hide();
        }

        /// popup的特点是点击popup对象外的区域，popup对象将自动消失。
        // public static bool PopupMode = false;



        /// popupMode=true, 点击popup对象外的区域，popup对象将自动消失。
        public virtual void ShowModal(bool popupMode = false)
        {
            // if (_popupStack.Count == 0 || _popupStack.IndexOf(this) < 0)
            // {
            //     _popupStack.Add(this);
            // }

            this.modal = true;
            this.result = PopupResult.Cancel;
            this.Center();

            if (popupMode)
            {
                GRoot.inst.ShowPopup(this);
            }
            else
            {
                this.Show();
            }
        }

        protected override void OnShown()
        {
            this.result = PopupResult.Cancel;
        }

        protected override void DoShowAnimation()
        {
            onShowAnimation?.Invoke(this);
        }

        protected override void OnHide()
        {
            // _popupStack.Remove(this);
            //Log.Error("remove");
        }

        protected override void DoHideAnimation()
        {
            if (onHideAnimation != null)
            {
                onHideAnimation(this);
            }
            else
            {
                this.HideImmediately();
            }
            //this.HideImmediately();
            //doHideAnimationAction?.Invoke(this);
            //this.SetPivot(0.5f, 0.5f);
            //float x = this.x;
            //this.x = -this.width;
            //this.TweenMoveX(-this.width, 0.2f).OnComplete(this.HideImmediately);

            /*this.scale =Vector2.one;
            this.TweenScale(Vector2.one*1.05f, 0.1f).
                OnComplete(() => 
                    this.TweenScale(Vector2.one,0.1f
                    ).OnComplete(this.HideImmediately)
                );*/
            // this.TweenFade(0, 0.2f).OnComplete(this.HideImmediately);

            //this.scale = new Vector2(0.5f, 0.5f);
            //this.TweenScale(Vector2.one*0.1f, 0.2f).OnComplete(this.HideImmediately);
        }

        public static void DefaultDoShowAnimation(GPopupWindow win)
        {
            //doShowAnimationAction?.Invoke(this);
            win.SetPivot(0.5f, 0.5f);
            //this.doShowAnimationAction?.Invoke(this);
            //            float x = this.x;
            //            this.x = -this.width;
            //            this.TweenMoveX(x, 0.2f).OnComplete(this.OnShown);
            /*this.scale =Vector2.one;
            this.TweenScale(Vector2.one*1.05f, 0.1f).
                OnComplete(() => 
                    this.TweenScale(Vector2.one,0.1f
                    ).OnComplete(this.OnShown)
                );*/

            // this.alpha = 0f;
            // this.TweenFade(1, 0.2f).OnComplete(this.OnShown);

            win.scale = Vector2.one * 0.7f;

            win.TweenScale(Vector2.one, 0.2f).OnComplete(win.OnShown);
        }

        //        private static void popAnimation(PopupWindow win)
        //        {
        //            win.scale = Vector2.one;
        //            win.SetPivot(0.5f, 0.5f);
        //            win.TweenScale(new Vector2(1.05f, 1.05f), 0.12f).
        //                OnComplete(() => 
        //                    win.TweenScale(Vector2.one,0.1f
        //                    ).OnComplete(win.HideImmediately)
        //                );
        //        }


    }
}