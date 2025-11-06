/**
 * 文件名: EventSystem.cs
 * 作用: 实现测试工具窗口的事件驱动架构
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 事件系统类，提供统一的事件管理和分发机制。
    /// 用于解耦各个管理器之间的直接依赖，实现松耦合的架构设计。
    /// </summary>
    public class EventSystem
    {
        #region 单例模式
        
        /// <summary>
        /// 事件系统的单例实例
        /// </summary>
        private static EventSystem _instance;
        
        /// <summary>
        /// 获取事件系统的单例实例
        /// </summary>
        public static EventSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventSystem();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private EventSystem() { }
        
        #endregion
        
        #region 事件定义
        
        // 配置管理相关事件
        /// <summary>
        /// 配置文件加载完成事件
        /// </summary>
        public event Action<TestToolsWindowData> ConfigurationLoaded;
        
        /// <summary>
        /// 配置文件保存完成事件
        /// </summary>
        public event Action<TestToolsWindowData> ConfigurationSaved;
        
        /// <summary>
        /// 配置文件列表更新事件
        /// </summary>
        public event Action<List<string>, string[]> AvailableConfigsUpdated;
        
        /// <summary>
        /// 配置数据变脏事件
        /// </summary>
        public event Action ConfigurationDirty;
        
        /// <summary>
        /// 配置选择改变事件
        /// </summary>
        public event Action<int> ConfigurationSelectionChanged;
        
        // UI管理相关事件
        /// <summary>
        /// 标签页选择改变事件
        /// </summary>
        public event Action<int> TabSelectionChanged;
        
        /// <summary>
        /// 编辑模式切换事件
        /// </summary>
        public event Action<bool> EditModeToggled;
        
        /// <summary>
        /// 保存请求事件
        /// </summary>
        public event Action SaveRequested;
        
        /// <summary>
        /// 定位配置文件请求事件
        /// </summary>
        public event Action LocateConfigRequested;
        
        /// <summary>
        /// 联机启动请求事件
        /// </summary>
        public event Action OnlineStartRequested;
        
        /// <summary>
        /// 通知消息显示事件
        /// </summary>
        public event Action<string> NotificationRequested;
        
        // 标签页管理相关事件
        /// <summary>
        /// 标签页添加事件
        /// </summary>
        public event Action<string> TabAdded;
        
        /// <summary>
        /// 标签页删除事件
        /// </summary>
        public event Action<int> TabRemoved;
        
        /// <summary>
        /// 标签页重命名事件
        /// </summary>
        public event Action<int, string> TabRenamed;
        
        // 工具项管理相关事件
        /// <summary>
        /// 工具项添加事件
        /// </summary>
        public event Action<int, TestToolItem> ToolItemAdded;
        
        /// <summary>
        /// 工具项删除事件
        /// </summary>
        public event Action<int, int> ToolItemRemoved; // tabIndex, itemIndex
        
        /// <summary>
        /// 工具项修改事件
        /// </summary>
        public event Action<int, int, TestToolItem> ToolItemModified; // tabIndex, itemIndex, newItem
        
        // 拖拽相关事件
        /// <summary>
        /// 标签页重排序事件
        /// </summary>
        public event Action<int, int> TabReordered; // fromIndex, toIndex
        
        /// <summary>
        /// 工具项重排序事件
        /// </summary>
        public event Action<int, int, int> ToolItemReordered; // tabIndex, fromIndex, toIndex
        
        /// <summary>
        /// 拖拽状态改变事件
        /// </summary>
        public event Action<bool> DragStateChanged;
        
        // 应用程序相关事件
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        public event Action WindowClosing;
        
        /// <summary>
        /// 窗口焦点改变事件
        /// </summary>
        public event Action<bool> WindowFocusChanged;
        
        /// <summary>
        /// 错误发生事件
        /// </summary>
        public event Action<string, Exception> ErrorOccurred;
        
        #endregion
        
        #region 事件触发方法
        
        // 配置管理事件触发
        /// <summary>
        /// 触发配置文件加载完成事件
        /// </summary>
        /// <param name="data">加载的配置数据</param>
        public void TriggerConfigurationLoaded(TestToolsWindowData data)
        {
            try
            {
                ConfigurationLoaded?.Invoke(data);
                Debug.Log($"[事件系统] 配置文件加载完成: {data?.name}");
            }
            catch (Exception ex)
            {
                HandleEventError("ConfigurationLoaded", ex);
            }
        }
        
        /// <summary>
        /// 触发配置文件保存完成事件
        /// </summary>
        /// <param name="data">保存的配置数据</param>
        public void TriggerConfigurationSaved(TestToolsWindowData data)
        {
            try
            {
                ConfigurationSaved?.Invoke(data);
                Debug.Log($"[事件系统] 配置文件保存完成: {data?.name}");
            }
            catch (Exception ex)
            {
                HandleEventError("ConfigurationSaved", ex);
            }
        }
        
        /// <summary>
        /// 触发配置文件列表更新事件
        /// </summary>
        /// <param name="paths">配置文件路径列表</param>
        /// <param name="names">配置文件名称数组</param>
        public void TriggerAvailableConfigsUpdated(List<string> paths, string[] names)
        {
            try
            {
                AvailableConfigsUpdated?.Invoke(paths, names);
            }
            catch (Exception ex)
            {
                HandleEventError("AvailableConfigsUpdated", ex);
            }
        }
        
        /// <summary>
        /// 触发配置数据变脏事件
        /// </summary>
        public void TriggerConfigurationDirty()
        {
            try
            {
                ConfigurationDirty?.Invoke();
            }
            catch (Exception ex)
            {
                HandleEventError("ConfigurationDirty", ex);
            }
        }
        
        /// <summary>
        /// 触发配置选择改变事件
        /// </summary>
        /// <param name="index">新选中的配置索引</param>
        public void TriggerConfigurationSelectionChanged(int index)
        {
            try
            {
                ConfigurationSelectionChanged?.Invoke(index);
            }
            catch (Exception ex)
            {
                HandleEventError("ConfigurationSelectionChanged", ex);
            }
        }
        
        // UI管理事件触发
        /// <summary>
        /// 触发标签页选择改变事件
        /// </summary>
        /// <param name="tabIndex">新选中的标签页索引</param>
        public void TriggerTabSelectionChanged(int tabIndex)
        {
            try
            {
                TabSelectionChanged?.Invoke(tabIndex);
            }
            catch (Exception ex)
            {
                HandleEventError("TabSelectionChanged", ex);
            }
        }
        
        /// <summary>
        /// 触发编辑模式切换事件
        /// </summary>
        /// <param name="isEditMode">是否为编辑模式</param>
        public void TriggerEditModeToggled(bool isEditMode)
        {
            try
            {
                EditModeToggled?.Invoke(isEditMode);
            }
            catch (Exception ex)
            {
                HandleEventError("EditModeToggled", ex);
            }
        }
        
        /// <summary>
        /// 触发保存请求事件
        /// </summary>
        public void TriggerSaveRequested()
        {
            try
            {
                SaveRequested?.Invoke();
            }
            catch (Exception ex)
            {
                HandleEventError("SaveRequested", ex);
            }
        }
        
        /// <summary>
        /// 触发定位配置文件请求事件
        /// </summary>
        public void TriggerLocateConfigRequested()
        {
            try
            {
                LocateConfigRequested?.Invoke();
            }
            catch (Exception ex)
            {
                HandleEventError("LocateConfigRequested", ex);
            }
        }
        
        /// <summary>
        /// 触发联机启动请求事件
        /// </summary>
        public void TriggerOnlineStartRequested()
        {
            try
            {
                OnlineStartRequested?.Invoke();
            }
            catch (Exception ex)
            {
                HandleEventError("OnlineStartRequested", ex);
            }
        }
        
        /// <summary>
        /// 触发通知消息显示事件
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public void TriggerNotificationRequested(string message)
        {
            try
            {
                NotificationRequested?.Invoke(message);
            }
            catch (Exception ex)
            {
                HandleEventError("NotificationRequested", ex);
            }
        }
        
        // 标签页管理事件触发
        /// <summary>
        /// 触发标签页添加事件
        /// </summary>
        /// <param name="tabName">新标签页名称</param>
        public void TriggerTabAdded(string tabName)
        {
            try
            {
                TabAdded?.Invoke(tabName);
            }
            catch (Exception ex)
            {
                HandleEventError("TabAdded", ex);
            }
        }
        
        /// <summary>
        /// 触发标签页删除事件
        /// </summary>
        /// <param name="tabIndex">要删除的标签页索引</param>
        public void TriggerTabRemoved(int tabIndex)
        {
            try
            {
                TabRemoved?.Invoke(tabIndex);
            }
            catch (Exception ex)
            {
                HandleEventError("TabRemoved", ex);
            }
        }
        
        /// <summary>
        /// 触发标签页重命名事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="newName">新名称</param>
        public void TriggerTabRenamed(int tabIndex, string newName)
        {
            try
            {
                TabRenamed?.Invoke(tabIndex, newName);
            }
            catch (Exception ex)
            {
                HandleEventError("TabRenamed", ex);
            }
        }
        
        // 工具项管理事件触发
        /// <summary>
        /// 触发工具项添加事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="toolItem">新工具项</param>
        public void TriggerToolItemAdded(int tabIndex, TestToolItem toolItem)
        {
            try
            {
                ToolItemAdded?.Invoke(tabIndex, toolItem);
            }
            catch (Exception ex)
            {
                HandleEventError("ToolItemAdded", ex);
            }
        }
        
        /// <summary>
        /// 触发工具项删除事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="itemIndex">工具项索引</param>
        public void TriggerToolItemRemoved(int tabIndex, int itemIndex)
        {
            try
            {
                ToolItemRemoved?.Invoke(tabIndex, itemIndex);
            }
            catch (Exception ex)
            {
                HandleEventError("ToolItemRemoved", ex);
            }
        }
        
        /// <summary>
        /// 触发工具项修改事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="itemIndex">工具项索引</param>
        /// <param name="newItem">修改后的工具项</param>
        public void TriggerToolItemModified(int tabIndex, int itemIndex, TestToolItem newItem)
        {
            try
            {
                ToolItemModified?.Invoke(tabIndex, itemIndex, newItem);
            }
            catch (Exception ex)
            {
                HandleEventError("ToolItemModified", ex);
            }
        }
        
        // 拖拽事件触发
        /// <summary>
        /// 触发标签页重排序事件
        /// </summary>
        /// <param name="fromIndex">原始索引</param>
        /// <param name="toIndex">目标索引</param>
        public void TriggerTabReordered(int fromIndex, int toIndex)
        {
            try
            {
                TabReordered?.Invoke(fromIndex, toIndex);
            }
            catch (Exception ex)
            {
                HandleEventError("TabReordered", ex);
            }
        }
        
        /// <summary>
        /// 触发工具项重排序事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="fromIndex">原始索引</param>
        /// <param name="toIndex">目标索引</param>
        public void TriggerToolItemReordered(int tabIndex, int fromIndex, int toIndex)
        {
            try
            {
                ToolItemReordered?.Invoke(tabIndex, fromIndex, toIndex);
            }
            catch (Exception ex)
            {
                HandleEventError("ToolItemReordered", ex);
            }
        }
        
        /// <summary>
        /// 触发拖拽状态改变事件
        /// </summary>
        /// <param name="isDragging">是否正在拖拽</param>
        public void TriggerDragStateChanged(bool isDragging)
        {
            try
            {
                DragStateChanged?.Invoke(isDragging);
            }
            catch (Exception ex)
            {
                HandleEventError("DragStateChanged", ex);
            }
        }
        
        // 应用程序事件触发
        /// <summary>
        /// 触发窗口关闭事件
        /// </summary>
        public void TriggerWindowClosing()
        {
            try
            {
                WindowClosing?.Invoke();
            }
            catch (Exception ex)
            {
                HandleEventError("WindowClosing", ex);
            }
        }
        
        /// <summary>
        /// 触发窗口焦点改变事件
        /// </summary>
        /// <param name="hasFocus">是否有焦点</param>
        public void TriggerWindowFocusChanged(bool hasFocus)
        {
            try
            {
                WindowFocusChanged?.Invoke(hasFocus);
            }
            catch (Exception ex)
            {
                HandleEventError("WindowFocusChanged", ex);
            }
        }
        
        /// <summary>
        /// 触发错误发生事件
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="exception">异常对象</param>
        public void TriggerErrorOccurred(string message, Exception exception)
        {
            try
            {
                ErrorOccurred?.Invoke(message, exception);
                Debug.LogError($"[事件系统] 错误发生: {message}");
                if (exception != null)
                {
                    Debug.LogException(exception);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[事件系统] 处理错误事件时发生异常: {ex.Message}");
            }
        }
        
        #endregion
        
        #region 事件管理方法
        
        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public void ClearAllEvents()
        {
            // 配置管理事件
            ConfigurationLoaded = null;
            ConfigurationSaved = null;
            AvailableConfigsUpdated = null;
            ConfigurationDirty = null;
            ConfigurationSelectionChanged = null;
            
            // UI管理事件
            TabSelectionChanged = null;
            EditModeToggled = null;
            SaveRequested = null;
            LocateConfigRequested = null;
            OnlineStartRequested = null;
            NotificationRequested = null;
            
            // 标签页管理事件
            TabAdded = null;
            TabRemoved = null;
            TabRenamed = null;
            
            // 工具项管理事件
            ToolItemAdded = null;
            ToolItemRemoved = null;
            ToolItemModified = null;
            
            // 拖拽事件
            TabReordered = null;
            ToolItemReordered = null;
            DragStateChanged = null;
            
            // 应用程序事件
            WindowClosing = null;
            WindowFocusChanged = null;
            ErrorOccurred = null;
            
            Debug.Log("[事件系统] 所有事件订阅已清除");
        }
        
        /// <summary>
        /// 获取事件订阅统计信息
        /// </summary>
        /// <returns>事件订阅统计字典</returns>
        public Dictionary<string, int> GetEventSubscriptionStats()
        {
            var stats = new Dictionary<string, int>();
            
            // 统计各个事件的订阅数量
            stats["ConfigurationLoaded"] = ConfigurationLoaded?.GetInvocationList()?.Length ?? 0;
            stats["ConfigurationSaved"] = ConfigurationSaved?.GetInvocationList()?.Length ?? 0;
            stats["AvailableConfigsUpdated"] = AvailableConfigsUpdated?.GetInvocationList()?.Length ?? 0;
            stats["ConfigurationDirty"] = ConfigurationDirty?.GetInvocationList()?.Length ?? 0;
            stats["ConfigurationSelectionChanged"] = ConfigurationSelectionChanged?.GetInvocationList()?.Length ?? 0;
            
            stats["TabSelectionChanged"] = TabSelectionChanged?.GetInvocationList()?.Length ?? 0;
            stats["EditModeToggled"] = EditModeToggled?.GetInvocationList()?.Length ?? 0;
            stats["SaveRequested"] = SaveRequested?.GetInvocationList()?.Length ?? 0;
            stats["LocateConfigRequested"] = LocateConfigRequested?.GetInvocationList()?.Length ?? 0;
            stats["OnlineStartRequested"] = OnlineStartRequested?.GetInvocationList()?.Length ?? 0;
            stats["NotificationRequested"] = NotificationRequested?.GetInvocationList()?.Length ?? 0;
            
            stats["TabAdded"] = TabAdded?.GetInvocationList()?.Length ?? 0;
            stats["TabRemoved"] = TabRemoved?.GetInvocationList()?.Length ?? 0;
            stats["TabRenamed"] = TabRenamed?.GetInvocationList()?.Length ?? 0;
            
            stats["ToolItemAdded"] = ToolItemAdded?.GetInvocationList()?.Length ?? 0;
            stats["ToolItemRemoved"] = ToolItemRemoved?.GetInvocationList()?.Length ?? 0;
            stats["ToolItemModified"] = ToolItemModified?.GetInvocationList()?.Length ?? 0;
            
            stats["TabReordered"] = TabReordered?.GetInvocationList()?.Length ?? 0;
            stats["ToolItemReordered"] = ToolItemReordered?.GetInvocationList()?.Length ?? 0;
            stats["DragStateChanged"] = DragStateChanged?.GetInvocationList()?.Length ?? 0;
            
            stats["WindowClosing"] = WindowClosing?.GetInvocationList()?.Length ?? 0;
            stats["WindowFocusChanged"] = WindowFocusChanged?.GetInvocationList()?.Length ?? 0;
            stats["ErrorOccurred"] = ErrorOccurred?.GetInvocationList()?.Length ?? 0;
            
            return stats;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 处理事件执行过程中的错误
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="exception">异常对象</param>
        private void HandleEventError(string eventName, Exception exception)
        {
            string errorMessage = $"[事件系统] 执行事件 '{eventName}' 时发生异常: {exception.Message}";
            Debug.LogError(errorMessage);
            Debug.LogException(exception);
            
            // 避免在错误处理中再次触发错误事件，造成无限循环
            if (eventName != "ErrorOccurred")
            {
                try
                {
                    ErrorOccurred?.Invoke(errorMessage, exception);
                }
                catch
                {
                    // 忽略错误事件处理中的异常，避免无限循环
                }
            }
        }
        
        #endregion
    }
}