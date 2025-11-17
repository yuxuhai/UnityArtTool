using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// ArtTools 编辑器窗口主类，负责窗口生命周期管理和各管理器的协调。
    /// 采用事件驱动架构，通过各个专门的管理器类来处理具体功能。
    /// </summary>
    public class ArtToolsEditorWindow : EditorWindow, IHasCustomMenu
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
        
        #endregion

        #region 窗口生命周期

        /// <summary>
        /// 打开 ArtTools 工具窗口的菜单项
        /// </summary>
        [MenuItem("工具/美术工具合集", false, 2)]
        public static void Open()
        {
            ArtToolsEditorWindow window = GetWindow<ArtToolsEditorWindow>();
            window.titleContent = new GUIContent("美术工具合集");
            window.Show();
        }

        /// <summary>
        /// 窗口被激活或首次打开时调用，初始化所有管理器
        /// </summary>
        private void OnEnable()
        {
            InitializeManagers();
        }

        /// <summary>
        /// 窗口关闭时调用，清理资源和事件订阅
        /// </summary>
        private void OnDisable()
        {
            // 解除配置管理器事件订阅
            if (_configManager != null)
            {
                _configManager.OnConfigurationLoaded -= OnConfigurationLoaded;
                _configManager.OnConfigurationSaved -= OnConfigurationSaved;
                _configManager.OnConfigurationDirty -= OnConfigurationDirty;
                // OnAvailableConfigsUpdated 使用的是匿名委托，这里无法显式解除，但其生命周期随窗口结束
            }

            // 解除 UI 管理器事件订阅
            if (_uiManager != null)
            {
                _uiManager.OnConfigurationSelectionChanged -= OnConfigurationSelectionChanged;
                _uiManager.OnTabAdded -= OnTabAdded;
                _uiManager.OnTabRemoved -= OnTabRemoved;
                _uiManager.OnToolItemAdded -= OnToolItemAdded;
                _uiManager.OnSaveRequested -= OnSaveRequested;
                _uiManager.OnLocateConfigRequested -= OnLocateConfigRequested;
                _uiManager.OnOnlineStartRequested -= OnOnlineStartRequested;
                _uiManager.OnRefreshConfigRequested -= OnRefreshConfigRequested;
            }

            // 解除拖拽管理器事件订阅并重置状态
            if (_dragDropManager != null)
            {
                _dragDropManager.OnTabReordered -= OnTabReordered;
                _dragDropManager.OnToolItemReordered -= OnToolItemReordered;
                _dragDropManager.ResetDragState();
            }
        }

        /// <summary>
        /// 窗口获得或失去焦点时调用
        /// </summary>
        /// <param name="hasFocus">是否有焦点</param>
        private void OnFocus()
        {
        }

        /// <summary>
        /// 窗口失去焦点时调用
        /// </summary>
        private void OnLostFocus()
        {
        }

        /// <summary>
        /// Unity每帧调用此方法来绘制窗口UI
        /// </summary>
        void OnGUI()
        {
            try
            {
                DrawMainUI();
            }
            // 只拦截“真正的”异常，像 ExitGUIException 这样的控制流异常直接让 Unity 自己处理，
            // 否则会在使用 ObjectField / ObjectPicker 等控件时频繁刷 ExitGUIException 日志。
            catch (Exception ex) when (!(ex is ExitGUIException))
            {
                Debug.LogError($"[ArtTools] 绘制 UI 时发生异常: {ex.Message}");
                Debug.LogException(ex);
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
                // 初始化各个管理器
                _configManager = new ConfigurationManager();
                _uiManager = new UIManager();
                _dragDropManager = new DragDropManager();
                
                // 初始化管理器
                _configManager.Initialize();
                _uiManager.Initialize();

                // 订阅配置管理事件
                _configManager.OnConfigurationLoaded += OnConfigurationLoaded;
                _configManager.OnConfigurationSaved += OnConfigurationSaved;
                _configManager.OnConfigurationDirty += OnConfigurationDirty;
                // 配置列表更新目前只影响下拉内容，简单刷新窗口即可
                _configManager.OnAvailableConfigsUpdated += (paths, names) => Repaint();

                // 订阅 UI 管理事件
                _uiManager.OnConfigurationSelectionChanged += OnConfigurationSelectionChanged;
                _uiManager.OnTabAdded += OnTabAdded;
                _uiManager.OnTabRemoved += OnTabRemoved;
                _uiManager.OnToolItemAdded += OnToolItemAdded;
                _uiManager.OnSaveRequested += OnSaveRequested;
                _uiManager.OnLocateConfigRequested += OnLocateConfigRequested;
                _uiManager.OnOnlineStartRequested += OnOnlineStartRequested;
                _uiManager.OnRefreshConfigRequested += OnRefreshConfigRequested;

                // 订阅拖拽事件
                _dragDropManager.OnTabReordered += OnTabReordered;
                _dragDropManager.OnToolItemReordered += OnToolItemReordered;
                
                Debug.Log("[ArtTools] 所有管理器初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ArtTools] 管理器初始化失败: {ex.Message}");
                throw;
            }
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
                EditorGUILayout.HelpBox("请在上方选择一个 ArtTools 配置文件，或者在项目中创建一个。\n(右键 -> Create -> ArtTools -> Art Tool Data)", MessageType.Info);
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
        private void OnConfigurationLoaded(ArtToolsWindowData data)
        {
            Repaint(); // 刷新UI显示
        }
        
        /// <summary>
        /// 处理配置文件保存完成事件
        /// </summary>
        /// <param name="data">保存的配置数据</param>
        private void OnConfigurationSaved(ArtToolsWindowData data)
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
                    Debug.LogError("[ArtTools] 联机启动失败: 场景引用丢失，请重新设置联机启动场景");
                    ShowNotification(new GUIContent("联机启动失败: 场景引用丢失"));
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
                        Debug.LogError($"[ArtTools] 联机启动失败, 无法打开场景 '{scenePath}'");
                        Debug.LogException(e);
                        ShowNotification(new GUIContent("联机启动失败, 请查看控制台"));
                    }
                }
            }
            else
            {
                Debug.LogError("[ArtTools] 联机启动失败: 未设置联机启动场景");
                ShowNotification(new GUIContent("联机启动失败: 未设置联机启动场景"));
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
                activeData.tabs.Add(new ArtToolsTab { name = tabName });
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
                ArtToolsTab draggedTab = activeData.tabs[fromIndex];
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
        private void OnToolItemAdded(int tabIndex, ArtToolItem toolItem)
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
                    ArtToolItem draggedItem = toolItems[fromIndex];
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
        /// 处理刷新配置列表请求事件
        /// </summary>
        private void OnRefreshConfigRequested()
        {
            _configManager?.RefreshAvailableConfigurations();
            Debug.Log("[ArtTools] 已刷新配置文件列表");
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
          
             menu.AddItem(new GUIContent("重置拖拽状态"), false, () => _dragDropManager?.ResetDragState());
         }
        
         
         #endregion
     }
 }


