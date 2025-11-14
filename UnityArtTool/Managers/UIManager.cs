/**
 * 文件名: UIManager.cs
 * 作用: 负责测试工具窗口的UI绘制和布局管理
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtTools
{
    /// <summary>
    /// UI管理器类，负责处理测试工具窗口的所有UI绘制和布局操作。
    /// 包括配置选择器、侧边栏、主面板、底部栏等UI组件的绘制。
    /// </summary>
    public class UIManager
    {
        #region 事件定义
        
        /// <summary>
        /// 配置文件选择改变时触发的事件
        /// </summary>
        public event Action<int> OnConfigurationSelectionChanged;
        
        /// <summary>
        /// 标签页选择改变时触发的事件
        /// </summary>
        public event Action<int> OnTabSelectionChanged;
        
        /// <summary>
        /// 编辑模式切换时触发的事件
        /// </summary>
        public event Action<bool> OnEditModeToggled;
        
        /// <summary>
        /// 请求保存配置时触发的事件
        /// </summary>
        public event Action OnSaveRequested;
        
        /// <summary>
        /// 请求定位配置文件时触发的事件
        /// </summary>
        public event Action OnLocateConfigRequested;
        
        /// <summary>
        /// 请求联机启动时触发的事件
        /// </summary>
        public event Action OnOnlineStartRequested;
        
        /// <summary>
        /// 标签页添加时触发的事件
        /// </summary>
        public event Action<string> OnTabAdded;
        
        /// <summary>
        /// 标签页删除时触发的事件
        /// </summary>
        public event Action<int> OnTabRemoved;
        
        /// <summary>
        /// 工具项添加时触发的事件
        /// </summary>
        public event Action<int, TestToolItem> OnToolItemAdded;
        
        #endregion
        
        #region 私有字段
        
        /// <summary>
        /// 当前选中的标签页ID
        /// </summary>
        private int _selectedTabID = 0;
        
        /// <summary>
        /// 是否处于编辑模式
        /// </summary>
        private bool _isEditMode = false;
        
        /// <summary>
        /// 是否处于重命名模式
        /// </summary>
        private bool _isRenameMode = false;
        
        /// <summary>
        /// 新标签页的名称
        /// </summary>
        private string _newTabName = "新标签页";
        
        /// <summary>
        /// 待添加的工具类型
        /// </summary>
        private ToolType _toolTypeToAdd;
        
        /// <summary>
        /// 左侧滚动位置
        /// </summary>
        private Vector2 _scrollPositionLeft;
        
        /// <summary>
        /// 右侧滚动位置
        /// </summary>
        private Vector2 _scrollPositionRight;
        
        /// <summary>
        /// 新工具项的临时实例
        /// </summary>
        private ToolAsset _newToolAsset = new ToolAsset();
        private FindObjectAsset _newFindObjectAsset = new FindObjectAsset();
        private FindGameObjectAsset _newGameObjectAsset = new FindGameObjectAsset();
        private TextAsset _newTextAsset = new TextAsset();
        private OpenPathAsset _newOpenPathAsset = new OpenPathAsset();
        private OpenSceneAsset _newSceneAsset = new OpenSceneAsset();
        private OpenWebAsset _newOpenWebAsset = new OpenWebAsset();
        private SeparatorAsset _newSeparatorAsset = new SeparatorAsset();
        
        /// <summary>
        /// 菜单项缓存
        /// </summary>
        private string[] _menuItems;
        private int _menuItemsIndex = 0;
        
        /// <summary>
        /// 快速搜索相关字段
        /// </summary>
        private string _searchKeyword = "";
        private int _selectedSearchEngine = 0;
        private bool _isSearchFoldoutOpen = false; // 快速搜索折叠状态，默认折叠
        private readonly string[] _searchEngines = new string[]
        {
            "Google",
            "百度",
            "必应 (Bing)",
            "GitHub",
            "Stack Overflow",
            "Unity Documentation",
            "知乎"
        };
        private readonly string[] _searchUrls = new string[]
        {
            "https://www.google.com/search?q={0}",
            "https://www.baidu.com/s?wd={0}",
            "https://www.bing.com/search?q={0}",
            "https://github.com/search?q={0}",
            "https://stackoverflow.com/search?q={0}",
            "https://docs.unity3d.com/2022.3/Documentation/Manual/30_search.html?q={0}",
            "https://www.zhihu.com/search?type=content&q={0}"
        };
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 获取或设置当前选中的标签页ID
        /// </summary>
        public int SelectedTabID 
        { 
            get => _selectedTabID;
            set
            {
                if (_selectedTabID != value)
                {
                    _selectedTabID = value;
                    OnTabSelectionChanged?.Invoke(value);
                }
            }
        }
        
        /// <summary>
        /// 获取或设置编辑模式状态
        /// </summary>
        public bool IsEditMode 
        { 
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnEditModeToggled?.Invoke(value);
                }
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        public void Initialize()
        {
            CacheMenuItems();
        }
        
        /// <summary>
        /// 绘制配置文件选择器
        /// </summary>
        /// <param name="selectedIndex">当前选中的配置文件索引</param>
        /// <param name="configNames">配置文件名称数组</param>
        /// <returns>新选中的配置文件索引</returns>
        public int DrawConfigSelector(int selectedIndex, string[] configNames)
        {
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(new GUIContent("配置文件"), selectedIndex, configNames);
            if (EditorGUI.EndChangeCheck())
            {
                OnConfigurationSelectionChanged?.Invoke(newIndex);
                return newIndex;
            }
            return selectedIndex;
        }
        
        /// <summary>
        /// 绘制快速搜索功能区域
        /// </summary>
        public void DrawQuickSearch()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 可折叠的标题
            _isSearchFoldoutOpen = EditorGUILayout.Foldout(_isSearchFoldoutOpen, "快速搜索", true, EditorStyles.foldoutHeader);
            
            // 只有在展开状态下才显示搜索内容
            if (_isSearchFoldoutOpen)
            {
                EditorGUILayout.Space(5);
                
                // 搜索关键词输入框
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("关键词:", GUILayout.Width(50));
                _searchKeyword = EditorGUILayout.TextField(_searchKeyword);
                EditorGUILayout.EndHorizontal();
                
                // 搜索引擎选择
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("搜索引擎:", GUILayout.Width(70));
                _selectedSearchEngine = EditorGUILayout.Popup(_selectedSearchEngine, _searchEngines);
                EditorGUILayout.EndHorizontal();
                
                // 搜索按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                // 禁用搜索按钮如果关键词为空
                EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_searchKeyword));
                
                // 创建搜索按钮的图标和文本
                var searchContent = EditorGUIUtility.IconContent("d_ViewToolZoom");
                searchContent.text = " 搜索";
                searchContent.tooltip = string.IsNullOrWhiteSpace(_searchKeyword) ? 
                    "请输入搜索关键词" : 
                    $"在{_searchEngines[_selectedSearchEngine]}中搜索: {_searchKeyword}";
                
                if (GUILayout.Button(searchContent, GUILayout.Height(25), GUILayout.MinWidth(80)))
                {
                    PerformSearch();
                }
                
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                // 显示提示信息
                if (string.IsNullOrWhiteSpace(_searchKeyword))
                {
                    EditorGUILayout.HelpBox("输入关键词后点击搜索按钮", MessageType.Info);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制左侧标签页边栏
        /// </summary>
        /// <param name="activeData">当前活动的配置数据</param>
        /// <param name="dragDropManager">拖拽管理器</param>
        /// <returns>是否有数据变化</returns>
        public bool DrawLeftSidebar(TestToolsWindowData activeData, DragDropManager dragDropManager)
        {
            if (activeData == null) return false;
            
            bool hasChanged = false;
            float width = _isEditMode ? 250f : 150f;
            _scrollPositionLeft = EditorGUILayout.BeginScrollView(_scrollPositionLeft, EditorStyles.helpBox, GUILayout.Width(width));

            // 编辑模式下的添加标签页UI
            if (_isEditMode)
            {
                EditorGUILayout.BeginHorizontal();
                _newTabName = EditorGUILayout.TextField(_newTabName);
                if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
                {
                    OnTabAdded?.Invoke(_newTabName);
                    hasChanged = true;
                }
                EditorGUILayout.EndHorizontal();
                _isRenameMode = EditorGUILayout.Toggle("重命名模式", _isRenameMode);
            }
            
            int tabIndexToRemove = -1;
            
            // 绘制所有标签页
            for (int i = 0; i < activeData.tabs.Count; i++)
            {
                Rect tabRect = EditorGUILayout.BeginHorizontal();
                
                // 编辑模式下的拖拽手柄
                if (_isEditMode)
                {
                    GUIStyle dragHandleStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedWidth = 15
                    };
                    GUILayout.Label("::", dragHandleStyle);
                }

                // 标签页名称显示或编辑
                if (_isEditMode && _isRenameMode)
                {
                    string newName = EditorGUILayout.TextField(activeData.tabs[i].name);
                    if (newName != activeData.tabs[i].name)
                    {
                        activeData.tabs[i].name = newName;
                        hasChanged = true;
                    }
                }
                else
                {
                    Color oldColor = GUI.backgroundColor;
                    if (_selectedTabID == i) GUI.backgroundColor = Color.cyan;
                    
                    if (GUILayout.Button(activeData.tabs[i].name, GUILayout.MinHeight(30)))
                    {
                        SelectedTabID = i;
                    }
                    GUI.backgroundColor = oldColor;
                }
                
                // 编辑模式下的删除按钮
                if (_isEditMode)
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        tabIndexToRemove = i;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // 处理拖拽事件
                if (_isEditMode && dragDropManager != null)
                {
                    dragDropManager.HandleTabDragAndDrop(i, tabRect);
                }
            }
            
            // 处理标签页删除
            if (tabIndexToRemove != -1)
            {
                OnTabRemoved?.Invoke(tabIndexToRemove);
                if (_selectedTabID >= activeData.tabs.Count - 1) 
                    _selectedTabID = Math.Max(0, activeData.tabs.Count - 2);
                hasChanged = true;
            }

            EditorGUILayout.EndScrollView();
            
            return hasChanged;
        }
        
        /// <summary>
        /// 绘制右侧主面板
        /// </summary>
        /// <param name="activeData">当前活动的配置数据</param>
        /// <param name="dragDropManager">拖拽管理器</param>
        /// <returns>是否有数据变化</returns>
        public bool DrawRightPanel(TestToolsWindowData activeData, DragDropManager dragDropManager)
        {
            if (activeData == null) return false;
            
            bool hasChanged = false;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 编辑模式下的添加新工具项面板
            if (_isEditMode)
            {
                hasChanged |= DrawAddNewItemPanel(activeData);
            }

            // 主内容滚动区域
            _scrollPositionRight = EditorGUILayout.BeginScrollView(_scrollPositionRight);
            if (activeData.tabs.Count > 0 && _selectedTabID >= 0 && _selectedTabID < activeData.tabs.Count)
            {
                // 传递拖拽管理器和标签页索引给ToolTab的DrawMainUI方法
                if (activeData.tabs[_selectedTabID].DrawMainUI(_isEditMode, dragDropManager, _selectedTabID))
                {
                    hasChanged = true;
                }
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            return hasChanged;
        }
        
        /// <summary>
        /// 绘制底部操作栏
        /// </summary>
        /// <param name="activeData">当前活动的配置数据</param>
        /// <param name="isDirty">配置是否有未保存的修改</param>
        /// <returns>是否有数据变化</returns>
        public bool DrawBottomBar(TestToolsWindowData activeData, bool isDirty)
        {
            if (activeData == null) return false;
            
            bool hasChanged = false;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 快捷启动区域
            EditorGUILayout.LabelField("快捷启动", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(activeData.onlineStartScene == null);
            if (GUILayout.Button(new GUIContent("联机启动", activeData.onlineStartScene == null ? "请先在下方设置联机启动场景" : "")))
            {
                OnOnlineStartRequested?.Invoke();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // 常规设置区域 - 仅在编辑模式下显示
            if (_isEditMode)
            {
                EditorGUILayout.LabelField("常规设置", EditorStyles.boldLabel);
                
                // 联机启动场景配置
                var newOnlineScene = (SceneAsset)EditorGUILayout.ObjectField("联机启动场景", activeData.onlineStartScene, typeof(SceneAsset), false);
                if (newOnlineScene != activeData.onlineStartScene)
                {
                    activeData.onlineStartScene = newOnlineScene;
                    hasChanged = true;
                }

                // 日志数量限制
                int newLogLimit = EditorGUILayout.IntField("日志保存数量", activeData.logLimit);
                if (newLogLimit != activeData.logLimit)
                {
                    activeData.logLimit = newLogLimit;
                    hasChanged = true;
                }
                
                // 截图保存路径
                EditorGUILayout.BeginHorizontal();
                string newScreenshotDir = EditorGUILayout.TextField("截图保存路径", activeData.screenshotDirectory);
                if (newScreenshotDir != activeData.screenshotDirectory)
                {
                    activeData.screenshotDirectory = newScreenshotDir;
                    hasChanged = true;
                }
                
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string path = EditorUtility.OpenFolderPanel("选择路径", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        activeData.screenshotDirectory = path + "/";
                        hasChanged = true;
                    }
                }
                if (GUILayout.Button("打开路径", GUILayout.MaxWidth(80)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", activeData.screenshotDirectory.Replace("/", @"\"));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
            }

            // 窗口核心操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("定位配置文件", GUILayout.MaxWidth(100)))
            {
                OnLocateConfigRequested?.Invoke();
            }
            
            bool newEditMode = GUILayout.Toggle(_isEditMode, "编辑模式", "Button", GUILayout.MaxWidth(80));
            if (newEditMode != _isEditMode)
            {
                IsEditMode = newEditMode;
            }

            string saveButtonText = isDirty ? "保存配置 *" : "保存配置";
            GUI.color = isDirty ? Color.yellow : Color.white;
            if (GUILayout.Button(saveButtonText, GUILayout.MaxWidth(90)))
            {
                OnSaveRequested?.Invoke();
            }
            GUI.color = Color.white;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            return hasChanged;
        }
        
        /// <summary>
        /// 显示通知消息
        /// </summary>
        /// <param name="message">通知内容</param>
        /// <param name="window">目标窗口</param>
        public void ShowNotification(string message, EditorWindow window)
        {
            if (window != null)
            {
                window.ShowNotification(new GUIContent(message));
            }
        }
        
        /// <summary>
        /// 刷新菜单项缓存
        /// </summary>
        public void RefreshMenuItems()
        {
            CacheMenuItems();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 绘制添加新工具项的面板
        /// </summary>
        /// <param name="activeData">当前活动的配置数据</param>
        /// <returns>是否有数据变化</returns>
        private bool DrawAddNewItemPanel(TestToolsWindowData activeData)
        {
            bool hasChanged = false;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("添加新工具项", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _toolTypeToAdd = (ToolType)EditorGUILayout.EnumPopup("工具类型", _toolTypeToAdd);
            if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            {
                if (activeData.tabs.Count > _selectedTabID)
                {
                    TestToolItem newItem = GetClonedNewItem();
                    OnToolItemAdded?.Invoke(_selectedTabID, newItem);
                    hasChanged = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            // 根据工具类型绘制对应的配置UI
            switch (_toolTypeToAdd)
            {
                case ToolType.Tool: DrawNewToolAssetUI(); break;
                case ToolType.FindObject: DrawNewFindObjectAssetUI(); break;
                case ToolType.FindGameObject: DrawNewGameObjectAssetUI(); break;
                case ToolType.Text: DrawNewTextAssetUI(); break;
                case ToolType.OpenPath: DrawNewOpenPathAssetUI(); break;
                case ToolType.Scene: DrawNewSceneAssetUI(); break;
                case ToolType.OpenWeb: DrawNewOpenWebAssetUI(); break;
                case ToolType.Separator: DrawNewSeparatorAssetUI(); break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            
            return hasChanged;
        }
        
        /// <summary>
        /// 绘制新工具资产的配置UI
        /// </summary>
        private void DrawNewToolAssetUI()
        {
            _newToolAsset.labelName = EditorGUILayout.TextField("标签", _newToolAsset.labelName);
            _newToolAsset.toolPath.label = EditorGUILayout.TextField("按钮名称", _newToolAsset.toolPath.label);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            _menuItemsIndex = EditorGUILayout.Popup("工具路径", _menuItemsIndex, _menuItems);
            if (EditorGUI.EndChangeCheck())
            {
                string newPath = _menuItems[_menuItemsIndex];
                _newToolAsset.toolPath.path = newPath;
                var pathParts = newPath.Split('/');
                _newToolAsset.toolPath.label = pathParts.Length > 0 ? pathParts[pathParts.Length - 1].Trim() : "";
            }
            
            if (GUILayout.Button("刷新", GUILayout.MaxWidth(40))) CacheMenuItems();
            EditorGUILayout.EndHorizontal();

            _newToolAsset.toolPath.path = _menuItems[_menuItemsIndex];
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(" ", _newToolAsset.toolPath.path);
            EditorGUI.EndDisabledGroup();
        }
        
        /// <summary>
        /// 绘制新查找对象资产的配置UI
        /// </summary>
        private void DrawNewFindObjectAssetUI()
        {
            _newFindObjectAsset.labelName = EditorGUILayout.TextField("标签", _newFindObjectAsset.labelName);
            _newFindObjectAsset.objectRef.label = EditorGUILayout.TextField("按钮名称", _newFindObjectAsset.objectRef.label);
            
            EditorGUI.BeginChangeCheck();
            _newFindObjectAsset.objectRef.targetObject = EditorGUILayout.ObjectField("资产", _newFindObjectAsset.objectRef.targetObject, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (_newFindObjectAsset.objectRef.targetObject != null)
                {
                    _newFindObjectAsset.objectRef.label = _newFindObjectAsset.objectRef.targetObject.name;
                }
            }
        }
        
        /// <summary>
        /// 绘制新游戏对象资产的配置UI
        /// </summary>
        private void DrawNewGameObjectAssetUI()
        {
            _newGameObjectAsset.labelName = EditorGUILayout.TextField("标签", _newGameObjectAsset.labelName);
            _newGameObjectAsset.buttonLabel = EditorGUILayout.TextField("按钮名称", _newGameObjectAsset.buttonLabel);

            EditorGUI.BeginChangeCheck();
            _newGameObjectAsset.targetObjectName = EditorGUILayout.TextField("物件名称", _newGameObjectAsset.targetObjectName);
            if(EditorGUI.EndChangeCheck())
            {
                _newGameObjectAsset.buttonLabel = _newGameObjectAsset.targetObjectName;
            }

            _newGameObjectAsset.targetScene = (SceneAsset)EditorGUILayout.ObjectField("所属场景 (可选)", _newGameObjectAsset.targetScene, typeof(SceneAsset), false);
        }
        
        /// <summary>
        /// 绘制新文本资产的配置UI
        /// </summary>
        private void DrawNewTextAssetUI()
        {
            _newTextAsset.textContent = EditorGUILayout.TextArea(_newTextAsset.textContent, GUILayout.MinHeight(40));
        }

        /// <summary>
        /// 绘制新打开路径资产的配置UI
        /// </summary>
        private void DrawNewOpenPathAssetUI()
        {
            EditorGUILayout.BeginHorizontal();
            _newOpenPathAsset.fullPath = EditorGUILayout.TextField("文件夹路径", _newOpenPathAsset.fullPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("选择路径", "", "");
                if(!string.IsNullOrEmpty(path)) _newOpenPathAsset.fullPath = path;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制新场景资产的配置UI
        /// </summary>
        private void DrawNewSceneAssetUI()
        {
            _newSceneAsset.labelName = EditorGUILayout.TextField("标签", _newSceneAsset.labelName);
            _newSceneAsset.buttonLabel = EditorGUILayout.TextField("按钮名称", _newSceneAsset.buttonLabel);

            EditorGUI.BeginChangeCheck();
            _newSceneAsset.scene = (SceneAsset)EditorGUILayout.ObjectField("场景资产", _newSceneAsset.scene, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (_newSceneAsset.scene != null)
                {
                    _newSceneAsset.buttonLabel = _newSceneAsset.scene.name;
                }
            }
        }
        
        /// <summary>
        /// 绘制新打开网页资产的配置UI
        /// </summary>
        private void DrawNewOpenWebAssetUI()
        {
            _newOpenWebAsset.labelName = EditorGUILayout.TextField("标签", _newOpenWebAsset.labelName);
            _newOpenWebAsset.buttonLabel = EditorGUILayout.TextField("按钮名称", _newOpenWebAsset.buttonLabel);
            _newOpenWebAsset.webUrl = EditorGUILayout.TextField("网页URL", _newOpenWebAsset.webUrl);
            _newOpenWebAsset.useInternalBrowser = EditorGUILayout.Toggle("使用内置浏览器", _newOpenWebAsset.useInternalBrowser);
            
            // 显示URL格式提示
            if (string.IsNullOrEmpty(_newOpenWebAsset.webUrl) || _newOpenWebAsset.webUrl == "https://")
            {
                EditorGUILayout.HelpBox("请输入完整的网页URL，例如：https://www.example.com", MessageType.Info);
            }
        }
        
        /// <summary>
        /// 绘制新分隔符资产的配置UI
        /// </summary>
        private void DrawNewSeparatorAssetUI()
        {
            _newSeparatorAsset.title = EditorGUILayout.TextField("标题", _newSeparatorAsset.title);
            _newSeparatorAsset.displayStyle = (SeparatorDisplayStyle)EditorGUILayout.EnumPopup("显示样式", _newSeparatorAsset.displayStyle);
        }
        
        /// <summary>
        /// 获取当前配置的新工具项的克隆实例
        /// </summary>
        /// <returns>克隆的工具项实例</returns>
        private TestToolItem GetClonedNewItem()
        {
            switch (_toolTypeToAdd)
            {
                case ToolType.Tool:           return _newToolAsset.Clone();
                case ToolType.FindObject:     return _newFindObjectAsset.Clone();
                case ToolType.FindGameObject: return _newGameObjectAsset.Clone();
                case ToolType.Text:           return _newTextAsset.Clone();
                case ToolType.OpenPath:       return _newOpenPathAsset.Clone();
                case ToolType.Scene:          return _newSceneAsset.Clone();
                case ToolType.OpenWeb:        return _newOpenWebAsset.Clone();
                case ToolType.Separator:      return _newSeparatorAsset.Clone();
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 执行搜索操作
        /// </summary>
        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(_searchKeyword))
            {
                Debug.LogWarning("[测试工具] 搜索关键词为空");
                return;
            }
            
            if (_selectedSearchEngine < 0 || _selectedSearchEngine >= _searchUrls.Length)
            {
                Debug.LogError("[测试工具] 无效的搜索引擎索引");
                return;
            }
            
            try
            {
                // URL编码搜索关键词
                string encodedKeyword = System.Uri.EscapeDataString(_searchKeyword.Trim());
                
                // 构建搜索URL
                string searchUrl = string.Format(_searchUrls[_selectedSearchEngine], encodedKeyword);
                
                // 打开搜索页面
                Application.OpenURL(searchUrl);
                
                Debug.Log($"[测试工具] 已在{_searchEngines[_selectedSearchEngine]}中搜索: {_searchKeyword}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[测试工具] 执行搜索时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 缓存所有可用的菜单项
        /// </summary>
        private void CacheMenuItems()
        {
            List<string> menuItemsList = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        foreach (MenuItem menuItem in method.GetCustomAttributes(typeof(MenuItem), false))
                        {
                            menuItemsList.Add(menuItem.menuItem);
                        }
                    }
                }
            }
            menuItemsList.Sort();
            _menuItems = menuItemsList.ToArray();
        }
        
        #endregion
    }
}