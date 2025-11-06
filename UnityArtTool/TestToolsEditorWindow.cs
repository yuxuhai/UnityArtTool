using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 测试工具编辑器窗口主类，负责窗口生命周期管理和各管理器的协调。
    /// 采用事件驱动架构，通过各个专门的管理器类来处理具体功能。
    /// </summary>
    public class TestToolsEditorWindow : EditorWindow, IHasCustomMenu
    {
        #region 管理器实例

        /// <summary>
        /// 配置管理器，负责配置文件的加载、保存和管理
        /// </summary>
        private ConfigurationManager _configManager;
        
        /// <summary>
        /// UI管理器，负责UI绘制和布局管理
        /// </summary>
        private UIManager _uiManager;
        
        /// <summary>
        /// 拖拽管理器，负责拖拽排序功能
        /// </summary>
        private DragDropManager _dragDropManager;
        
        /// <summary>
        /// 事件系统实例
        /// </summary>
        private EventSystem _eventSystem;
        
        /// <summary>
        /// 调试管理器实例
        /// </summary>
        private DebugManager _debugManager;

        #endregion

        #region 窗口生命周期

        /// <summary>
        /// 打开测试工具窗口的菜单项
        /// </summary>
        [MenuItem("工具/美术工具合集", false, 2)]
        public static void Open()
        {
            TestToolsEditorWindow window = GetWindow<TestToolsEditorWindow>();
            window.titleContent = new GUIContent("美术工具合集");
            window.Show();
        }

        /// <summary>
        /// 窗口被激活或首次打开时调用，初始化所有管理器和事件系统
        /// </summary>
        private void OnEnable()
        {
            InitializeManagers();
            SetupEventHandlers();
        }

        /// <summary>
        /// 窗口关闭时调用，清理资源和事件订阅
        /// </summary>
        private void OnDisable()
        {
            CleanupEventHandlers();
            _eventSystem?.TriggerWindowClosing();
        }

        /// <summary>
        /// 窗口获得或失去焦点时调用
        /// </summary>
        /// <param name="hasFocus">是否有焦点</param>
        private void OnFocus()
        {
            _eventSystem?.TriggerWindowFocusChanged(true);
        }

        /// <summary>
        /// 窗口失去焦点时调用
        /// </summary>
        private void OnLostFocus()
        {
            _eventSystem?.TriggerWindowFocusChanged(false);
        }

        /// <summary>
        /// Unity每帧调用此方法来绘制窗口UI
        /// </summary>
        void OnGUI()
        {
            try
            {
                // 开始性能监控
                _debugManager?.StartPerformanceMonitoring("OnGUI");
                
                DrawMainUI();
                
                // 结束性能监控
                _debugManager?.EndPerformanceMonitoring("OnGUI");
            }
            catch (Exception ex)
            {
                _debugManager?.LogError("TestToolsWindow", "绘制UI时发生异常", ex);
                _eventSystem?.TriggerErrorOccurred("绘制UI时发生异常", ex);
                EditorGUILayout.HelpBox($"UI绘制错误: {ex.Message}", MessageType.Error);
            }
        }

        #endregion
        
        #region 管理器初始化和事件处理
        
        /// <summary>
        /// 初始化所有管理器实例
        /// </summary>
        private void InitializeManagers()
        {
            try
            {
                // 获取调试管理器实例
                _debugManager = DebugManager.Instance;
                _debugManager.LogInfo("TestToolsWindow", "开始初始化测试工具窗口");
                
                // 获取事件系统实例
                _eventSystem = EventSystem.Instance;
                
                // 初始化各个管理器
                _configManager = new ConfigurationManager();
                _uiManager = new UIManager();
                _dragDropManager = new DragDropManager();
                
                // 初始化管理器
                _configManager.Initialize();
                _uiManager.Initialize();
                
                _debugManager.LogInfo("TestToolsWindow", "所有管理器初始化完成");
                Debug.Log("[测试工具] 所有管理器初始化完成");
            }
            catch (Exception ex)
            {
                _debugManager?.LogError("TestToolsWindow", "管理器初始化失败", ex);
                Debug.LogError($"[测试工具] 管理器初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 设置事件处理器，连接各个管理器的事件
        /// </summary>
        private void SetupEventHandlers()
        {
            if (_eventSystem == null) return;
            
            // 配置管理器事件
            _eventSystem.ConfigurationLoaded += OnConfigurationLoaded;
            _eventSystem.ConfigurationSaved += OnConfigurationSaved;
            _eventSystem.ConfigurationDirty += OnConfigurationDirty;
            _eventSystem.SaveRequested += OnSaveRequested;
            _eventSystem.LocateConfigRequested += OnLocateConfigRequested;
            _eventSystem.OnlineStartRequested += OnOnlineStartRequested;
            _eventSystem.NotificationRequested += OnNotificationRequested;
            
            // 标签页和工具项管理事件
            _eventSystem.TabAdded += OnTabAdded;
            _eventSystem.TabRemoved += OnTabRemoved;
            _eventSystem.TabReordered += OnTabReordered;
            _eventSystem.ToolItemAdded += OnToolItemAdded;
            _eventSystem.ToolItemReordered += OnToolItemReordered;
            
            // UI管理器事件连接
            if (_uiManager != null)
            {
                _uiManager.OnConfigurationSelectionChanged += (index) => _eventSystem.TriggerConfigurationSelectionChanged(index);
                _uiManager.OnTabSelectionChanged += (tabIndex) => _eventSystem.TriggerTabSelectionChanged(tabIndex);
                _uiManager.OnEditModeToggled += (isEditMode) => _eventSystem.TriggerEditModeToggled(isEditMode);
                _uiManager.OnSaveRequested += () => _eventSystem.TriggerSaveRequested();
                _uiManager.OnLocateConfigRequested += () => _eventSystem.TriggerLocateConfigRequested();
                _uiManager.OnOnlineStartRequested += () => _eventSystem.TriggerOnlineStartRequested();
                _uiManager.OnTabAdded += (tabName) => _eventSystem.TriggerTabAdded(tabName);
                _uiManager.OnTabRemoved += (tabIndex) => _eventSystem.TriggerTabRemoved(tabIndex);
                _uiManager.OnToolItemAdded += (tabIndex, toolItem) => _eventSystem.TriggerToolItemAdded(tabIndex, toolItem);
            }
            
            // 拖拽管理器事件连接
            if (_dragDropManager != null)
            {
                _dragDropManager.OnTabReordered += (fromIndex, toIndex) => _eventSystem.TriggerTabReordered(fromIndex, toIndex);
                _dragDropManager.OnToolItemReordered += (tabIndex, fromIndex, toIndex) => _eventSystem.TriggerToolItemReordered(tabIndex, fromIndex, toIndex);
                _dragDropManager.OnDragStateChanged += (isDragging) => _eventSystem.TriggerDragStateChanged(isDragging);
            }
            
            // 配置管理器事件连接
            if (_configManager != null)
            {
                _configManager.OnConfigurationLoaded += (data) => _eventSystem.TriggerConfigurationLoaded(data);
                _configManager.OnConfigurationSaved += (data) => _eventSystem.TriggerConfigurationSaved(data);
                _configManager.OnConfigurationDirty += () => _eventSystem.TriggerConfigurationDirty();
                _configManager.OnAvailableConfigsUpdated += (paths, names) => _eventSystem.TriggerAvailableConfigsUpdated(paths, names);
            }
            
            // 事件系统内部事件
            _eventSystem.ConfigurationSelectionChanged += OnConfigurationSelectionChanged;
            _eventSystem.ErrorOccurred += OnErrorOccurred;
        }
        
        /// <summary>
        /// 清理事件处理器，防止内存泄漏
        /// </summary>
        private void CleanupEventHandlers()
        {
            if (_eventSystem == null) return;
            
            // 清理所有事件订阅
            _eventSystem.ClearAllEvents();
        }
        
        #endregion
        
        #region UI绘制方法
        
        /// <summary>
        /// 绘制主UI界面
        /// </summary>
        private void DrawMainUI()
        {
            if (_configManager == null || _uiManager == null)
            {
                EditorGUILayout.HelpBox("管理器未初始化，请重新打开窗口。", MessageType.Error);
                return;
            }
            
            // 绘制配置选择器
            int newSelectedIndex = _uiManager.DrawConfigSelector(
                _configManager.SelectedDataIndex, 
                _configManager.AvailableDataNames ?? new string[0]
            );
            
            if (newSelectedIndex != _configManager.SelectedDataIndex)
            {
                _configManager.SelectedDataIndex = newSelectedIndex;
            }
            
            // 绘制快速搜索功能
            _uiManager.DrawQuickSearch();
            EditorGUILayout.Space();

            // 如果没有活动配置，显示提示信息
            if (_configManager.ActiveData == null)
            {
                EditorGUILayout.HelpBox("请在上方选择一个测试工具配置文件，或者在项目中创建一个。\n(右键 -> Create -> ArtTools -> Test Tool Data)", MessageType.Info);
                return;
            }
            
            // 处理拖拽完成事件
            _dragDropManager?.CompleteTabDragAndDrop(_configManager.ActiveData.tabs.Count);
            
            // 绘制主要内容区域
            EditorGUILayout.BeginHorizontal();
            {
                // 绘制左侧边栏
                bool leftSidebarChanged = _uiManager.DrawLeftSidebar(_configManager.ActiveData, _dragDropManager);
                
                // 绘制右侧面板
                bool rightPanelChanged = _uiManager.DrawRightPanel(_configManager.ActiveData, _dragDropManager);
                
                // 如果有数据变化，标记为脏数据
                if (leftSidebarChanged || rightPanelChanged)
                {
                    _configManager.MarkDirty();
                }
            }
            EditorGUILayout.EndHorizontal();

            // 绘制底部操作栏
            bool bottomBarChanged = _uiManager.DrawBottomBar(_configManager.ActiveData, _configManager.IsDirty);
            if (bottomBarChanged)
            {
                _configManager.MarkDirty();
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 处理配置文件加载完成事件
        /// </summary>
        /// <param name="data">加载的配置数据</param>
        private void OnConfigurationLoaded(TestToolsWindowData data)
        {
            Repaint(); // 刷新UI显示
        }
        
        /// <summary>
        /// 处理配置文件保存完成事件
        /// </summary>
        /// <param name="data">保存的配置数据</param>
        private void OnConfigurationSaved(TestToolsWindowData data)
        {
            _uiManager?.ShowNotification("配置已保存!", this);
        }
        
        /// <summary>
        /// 处理配置数据变脏事件
        /// </summary>
        private void OnConfigurationDirty()
        {
            Repaint(); // 刷新UI以显示未保存状态
        }
        
        /// <summary>
        /// 处理保存请求事件
        /// </summary>
        private void OnSaveRequested()
        {
            _configManager?.SaveConfiguration();
        }
        
        /// <summary>
        /// 处理定位配置文件请求事件
        /// </summary>
        private void OnLocateConfigRequested()
        {
            if (_configManager?.ActiveData != null)
            {
                Selection.activeObject = _configManager.ActiveData;
            }
        }
        
        /// <summary>
        /// 处理联机启动请求
        /// </summary>
        private void OnOnlineStartRequested()
        {
            var activeData = _configManager?.ActiveData;
            if (activeData?.onlineStartScene != null)
            {
                string scenePath = AssetDatabase.GetAssetPath(activeData.onlineStartScene);
                
                // 检查场景文件是否存在
                if (string.IsNullOrEmpty(scenePath) || !System.IO.File.Exists(scenePath))
                {
                    _eventSystem?.TriggerErrorOccurred("联机启动失败: 场景引用丢失，请重新设置联机启动场景", null);
                    return;
                }
                
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        EditorApplication.ExecuteMenuItem("Edit/Play");
                    }
                    catch (Exception e)
                    {
                        _eventSystem?.TriggerErrorOccurred($"联机启动失败, 无法打开场景 '{scenePath}'", e);
                    }
                }
            }
            else
            {
                _eventSystem?.TriggerErrorOccurred("联机启动失败: 未设置联机启动场景", null);
            }
        }
        
        /// <summary>
        /// 处理通知消息请求事件
        /// </summary>
        /// <param name="message">通知消息</param>
        private void OnNotificationRequested(string message)
        {
            ShowNotification(new GUIContent(message));
        }
        
        /// <summary>
        /// 处理标签页添加事件
        /// </summary>
        /// <param name="tabName">新标签页名称</param>
        private void OnTabAdded(string tabName)
        {
            var activeData = _configManager?.ActiveData;
            if (activeData != null)
            {
                activeData.tabs.Add(new ToolTab { name = tabName });
                _configManager.MarkDirty();
            }
        }
        
        /// <summary>
        /// 处理标签页删除事件
        /// </summary>
        /// <param name="tabIndex">要删除的标签页索引</param>
        private void OnTabRemoved(int tabIndex)
        {
            var activeData = _configManager?.ActiveData;
            if (activeData != null && tabIndex >= 0 && tabIndex < activeData.tabs.Count)
            {
                activeData.tabs.RemoveAt(tabIndex);
                _configManager.MarkDirty();
            }
        }
        
        /// <summary>
        /// 处理标签页重排序事件
        /// </summary>
        /// <param name="fromIndex">原始索引</param>
        /// <param name="toIndex">目标索引</param>
        private void OnTabReordered(int fromIndex, int toIndex)
        {
            var activeData = _configManager?.ActiveData;
            if (activeData != null && fromIndex >= 0 && fromIndex < activeData.tabs.Count &&
                toIndex >= 0 && toIndex < activeData.tabs.Count)
            {
                ToolTab draggedTab = activeData.tabs[fromIndex];
                activeData.tabs.RemoveAt(fromIndex);
                activeData.tabs.Insert(toIndex, draggedTab);
                
                // 更新UI管理器的选中标签页
                if (_uiManager.SelectedTabID == fromIndex)
                {
                    _uiManager.SelectedTabID = toIndex;
                }
                
                _configManager.MarkDirty();
            }
        }
        
        /// <summary>
        /// 处理工具项添加事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="toolItem">新工具项</param>
        private void OnToolItemAdded(int tabIndex, TestToolItem toolItem)
        {
            var activeData = _configManager?.ActiveData;
            if (activeData != null && tabIndex >= 0 && tabIndex < activeData.tabs.Count)
            {
                activeData.tabs[tabIndex].toolItems.Add(toolItem);
                _configManager.MarkDirty();
            }
        }
        
        /// <summary>
        /// 处理工具项重排序事件
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="fromIndex">原始索引</param>
        /// <param name="toIndex">目标索引</param>
        private void OnToolItemReordered(int tabIndex, int fromIndex, int toIndex)
        {
            var activeData = _configManager?.ActiveData;
            if (activeData != null && tabIndex >= 0 && tabIndex < activeData.tabs.Count)
            {
                var toolItems = activeData.tabs[tabIndex].toolItems;
                if (fromIndex >= 0 && fromIndex < toolItems.Count && toIndex >= 0 && toIndex <= toolItems.Count)
                {
                    TestToolItem draggedItem = toolItems[fromIndex];
                    toolItems.RemoveAt(fromIndex);
                    int finalDropIndex = (fromIndex < toIndex) ? toIndex - 1 : toIndex;
                    toolItems.Insert(finalDropIndex, draggedItem);
                    _configManager.MarkDirty();
                }
            }
        }
        
        /// <summary>
        /// 处理配置选择改变事件
        /// </summary>
        /// <param name="index">新选中的配置索引</param>
        private void OnConfigurationSelectionChanged(int index)
        {
            // 配置管理器会自动处理加载，这里只需要刷新UI
            Repaint();
        }
        
        /// <summary>
        /// 处理错误发生事件
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="exception">异常对象</param>
        private void OnErrorOccurred(string message, Exception exception)
        {
            // 记录到调试管理器
            _debugManager?.LogError("EventSystem", message, exception);
            
            // 错误已经在事件系统中记录，这里可以添加额外的UI反馈
             ShowNotification(new GUIContent($"错误: {message}"));
         }
         
         #endregion
         
         #region 右键菜单 (IHasCustomMenu)

         /// <summary>
         /// 添加自定义菜单项到窗口右键菜单
         /// </summary>
         /// <param name="menu">菜单对象</param>
         public void AddItemsToMenu(GenericMenu menu)
         {
             menu.AddItem(new GUIContent("刷新配置列表"), false, () => _configManager?.RefreshAvailableConfigurations());
             menu.AddItem(new GUIContent("刷新菜单项缓存"), false, () => _uiManager?.RefreshMenuItems());
             menu.AddSeparator("");
             menu.AddItem(new GUIContent("重置拖拽状态"), false, () => _dragDropManager?.ResetDragState());
             menu.AddSeparator("");
             menu.AddItem(new GUIContent("显示事件统计"), false, ShowEventStatistics);
             menu.AddItem(new GUIContent("显示调试面板"), false, ShowDebugPanel);
             menu.AddItem(new GUIContent("导出调试数据"), false, ExportDebugData);
             menu.AddSeparator("");
             menu.AddItem(new GUIContent("清除调试数据"), false, () => _debugManager?.ClearAllDebugData());
             menu.AddItem(new GUIContent("打开设置"), false, () => SettingsService.OpenUserPreferences("Preferences/测试工具"));
         }
         
         /// <summary>
         /// 显示事件系统统计信息
         /// </summary>
         private void ShowEventStatistics()
         {
             if (_eventSystem != null)
             {
                 var stats = _eventSystem.GetEventSubscriptionStats();
                 var message = "事件订阅统计:\n";
                 foreach (var kvp in stats)
                 {
                     if (kvp.Value > 0)
                     {
                         message += $"{kvp.Key}: {kvp.Value} 个订阅\n";
                     }
                 }
                 _debugManager?.LogInfo("EventStatistics", message);
                 Debug.Log($"[测试工具] {message}");
                 ShowNotification(new GUIContent("事件统计已输出到控制台"));
             }
         }
         
         /// <summary>
         /// 显示调试面板窗口
         /// </summary>
         private void ShowDebugPanel()
         {
             DebugPanelWindow.ShowWindow();
         }
         
         /// <summary>
         /// 导出调试数据到文件
         /// </summary>
         private void ExportDebugData()
         {
             if (_debugManager != null)
             {
                 string debugData = _debugManager.ExportDebugDataAsText();
                 string filePath = UnityEditor.EditorUtility.SaveFilePanel(
                     "导出调试数据", 
                     Application.dataPath, 
                     $"TestTools_Debug_{DateTime.Now:yyyyMMdd_HHmmss}.txt", 
                     "txt"
                 );
                 
                 if (!string.IsNullOrEmpty(filePath))
                 {
                     try
                     {
                         System.IO.File.WriteAllText(filePath, debugData);
                         _debugManager.LogInfo("DebugExport", $"调试数据已导出到: {filePath}");
                         ShowNotification(new GUIContent("调试数据导出成功"));
                     }
                     catch (Exception ex)
                     {
                         _debugManager.LogError("DebugExport", "导出调试数据失败", ex);
                         ShowNotification(new GUIContent("调试数据导出失败"));
                     }
                 }
             }
         }
        
         
         #endregion
    }
}
                 