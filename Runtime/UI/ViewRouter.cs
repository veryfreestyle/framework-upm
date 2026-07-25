// Author: JiangHao <jianghao01@hetao101.com>

using System.Collections.Generic;
using FairyGUI;
using System;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Runtime.UI
{

    public class ViewRouter : Singleton<ViewRouter>
    {
        private ViewHolder _currentView;
        private string _currentRoute;
        private readonly Stack<string> _routeStack = new();
        private readonly Stack<PopupHolder> _popupStack = new();

        // public string CurrentViewName => _currentView != null ? _currentView.Name : string.Empty;
        public bool CanBack => _popupStack.Count > 0 || _routeStack.Count > 0;
        public ViewHolder CurrentView => _currentView;

        /// <summary>
        /// 当前页面路由，未导航时为 null。
        /// </summary>
        public string CurrentRoute => _currentRoute;


        /// <summary>
        /// 更新当前页面的生命周期回调
        /// </summary>
        public void Update()
        {
            if (_currentView != null)
            {
                _currentView.onUpdate?.Invoke();
            }
        }

        /// <summary>
        /// 返回操作：优先关闭弹窗，其次页面回退
        /// </summary>
        public bool Back()
        {
            // 1. 优先关闭弹窗
            if (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Close();
                popup.Dispose();  // 释放资源
                return true;
            }

            // 2. 其次页面回退
            if (_routeStack.Count > 0)
            {
                var route = _routeStack.Pop();
                // 直接重建页面，不操作栈（避免清空剩余历史）
                return NavigateToRoute(route);
            }

            return false;
        }

        /// <summary>
        /// 导航到指定路由（内部方法，只负责页面切换，不操作栈）
        /// </summary>
        private bool NavigateToRoute(string route)
        {
            if (!_factories.TryGetValue(route, out var factory))
            {
                Debug.LogError($"'{route}' not found in factories.");
                return false;
            }
            var (viewName, viewFactory) = factory;

            // 1. 创建并初始化新页面
            var handle = new ViewHolder(viewName);
            if (!handle.Initialize())
            {
                handle.Dispose();
                Debug.LogError($"Failed to initialize view for route '{route}'.");
                return false;
            }

            try
            {
                handle.view = viewFactory(handle.component);
            }
            catch (Exception e)
            {
                Debug.LogError($"Factory failed for route '{route}': {e}");
                handle.Dispose();
                return false;
            }

            handle.FullScreen();

            // 2. 添加新页面到显示树
            GRoot.inst.AddChildAt(handle.component, 0);

            // 3. 处理旧页面
            var prevView = _currentView;
            if (prevView != null)
            {
                prevView.Dispose();  // Dispose 内部会调用 RemoveChild
            }

            // 4. 更新当前页面引用
            _currentView = handle;
            _currentRoute = route;
            return true;
        }

        private Dictionary<string, (string, Func<GComponent, object>)> _factories = new();

        /// <summary>
        /// 注册路由与页面工厂
        /// </summary>
        /// <param name="route"></param>
        /// <param name="viewName"></param>
        /// <param name="factory"></param>
        public void Map(string route, string viewName, Func<GComponent, object> factory)
        {
            if (_factories.ContainsKey(route))
            {
                Debug.LogWarning($"Route '{route}' is already mapped, overwriting.");
            }
            _factories[route] = (viewName, factory);
        }

        /// <summary>
        /// 获取路由对应的页面工厂
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private (string, Func<GComponent, object>) GetFactory(string route)
        {
            if (!_factories.TryGetValue(route, out var factory))
            {
                throw new Exception($"Route '{route}' not found in factories.");
            }
            return factory;
        }

        /// <summary>
        /// 显示通用弹窗
        /// </summary>
        public UniTask<PopupResult> ShowPopupAsync(string route, System.Threading.CancellationToken cancellationToken = default)
        {
            var (viewName, viewFactory) = GetFactory(route);
            var popup = new PopupHolder(viewName);
            // 纯尾部透传：无 await 外的 try/catch/using，直接返回内层 UniTask，省一个状态机
            return ShowPopupHolderAsync(popup, viewFactory, route, cancellationToken);
        }


        public const string MSGBOX_ROUTE = ":msgbox";  //  MsgBox 的路由为 "MsgBox" 

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="prompt">提示文本</param>
        /// <param name="title">标题（默认"提示"）</param>
        /// <param name="style">按钮样式（默认 OkOnly）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public UniTask<PopupResult> ShowMsgBoxAsync(
            string prompt,
            string title = "提示",
            MsgBoxStyle style = MsgBoxStyle.OkOnly,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var (viewName, viewFactory) = GetFactory(MSGBOX_ROUTE);
            var msgBox = new MsgBoxHolder(viewName)
            {
                title = title,
                prompt = prompt,
                style = style
            };
            // 纯尾部透传：无 await 外的 try/catch/using，直接返回内层 UniTask，省一个状态机
            return ShowPopupHolderAsync(msgBox, viewFactory, MSGBOX_ROUTE, cancellationToken);
        }

        /// <summary>
        /// 弹窗公共流程：初始化 → 建 View → 入栈 → 模态显示 → 等待关闭 → 出栈 → 取结果并释放
        /// </summary>
        private async UniTask<PopupResult> ShowPopupHolderAsync(
            PopupHolder popup,
            Func<GComponent, object> viewFactory,
            string route,
            System.Threading.CancellationToken cancellationToken)
        {
            if (!popup.Initialize())
            {
                popup.Dispose();
                throw new Exception($"Failed to initialize popup for route '{route}'.");
            }

            try
            {
                popup.view = viewFactory(popup.component);
            }
            catch
            {
                popup.Dispose();
                throw;
            }

            _popupStack.Push(popup);

            try
            {
                popup.ShowModal();
                await UniTask.WaitUntil(() => popup.IsShowing == false, PlayerLoopTiming.Update, cancellationToken);
            }
            catch
            {
                // 异常时确保出栈并清理
                if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
                    _popupStack.Pop();
                popup.Close();
                popup.Dispose();
                throw;
            }

            // 出栈（防止重复 Pop）
            if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
                _popupStack.Pop();

            var result = popup.result;
            popup.Dispose();
            return result;
        }

        /// <summary>
        /// 导航到指定路由，并可选择是否保留历史
        /// 1. 如果 keepHistory 为 true，则将当前路由入栈，以便后续可以回退。
        /// 2. 如果 keepHistory 为 false，则清空路由栈，当前路由将成为新的起点。
        /// 3. 如果当前没有页面，则直接导航到指定路由。
        /// 4. 如果导航失败（例如路由未注册），则返回 false，当前页面保持不变。
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="keepHistory">是否保留历史（默认 false，清空路由栈）</param>
        public bool Navigate(string route, bool keepHistory = false)
        {
            // 处理栈逻辑
            if (_currentView != null)
            {
                if (keepHistory && !string.IsNullOrEmpty(_currentRoute))
                {
                    _routeStack.Push(_currentRoute);  // 保留历史
                }
                else
                {
                    _routeStack.Clear();  // 清空历史
                }
            }

            // 执行导航
            return NavigateToRoute(route);
        }



        /// <summary>
        /// 释放所有页面和弹窗资源，清理路由栈和弹窗栈。
        /// </summary>
        public void Dispose()
        {
            // 清理弹窗栈
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Close();
                popup.Dispose();  // 释放资源
            }

            // 清理路由栈
            _routeStack.Clear();

            // 清理当前页面
            if (_currentView != null)
            {
                _currentView.Dispose();
                _currentView = null;
            }

            _currentRoute = null;
        }
    }


}