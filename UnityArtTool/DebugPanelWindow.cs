/**
 * 文件名: DebugPanelWindow.cs
 * 作者: yuxuhai
 * 日期: 2024
 * 描述: 测试工具调试面板窗口，提供调试信息的可视化界面，包括日志查看、性能监控、事件统计等功能
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 测试工具调试面板窗口
    /// 提供调试信息的可视化界面，包括日志查看、性能监控、事件统计等功能
    /// </summary>
    public class DebugPanelWindow : EditorWindow
    {
        #region 私有字段

        /// <summary>调试管理器实例</summary>
        private DebugManager _debugManager;
        
        /// <summary>当前选中的标签页索引</summary>
        private int _selectedTabIndex = 0;
        
        /// <summary>标签页名称数组</summary>
        private readonly string[] _tabNames = { "日志", "性能", "事件统计", "设置" };
        
        /// <summary>日志滚动位置</summary>
        private Vector2 _logScrollPosition;
        
        /// <summary>性能数据滚动位置</summary>
        private Vector2 _performanceScrollPosition;
        
        /// <summary>事件统计滚动位置</summary>
        private Vector2 _eventScrollPosition;
        
        /// <summary>日志级别过滤器</summary>
        private DebugLevel _logLevelFilter = DebugLevel.Verbose;
        
        /// <summary>日志分类过滤器</summary>
        private string _logCategoryFilter = "";
        
        /// <summary>是否显示堆栈跟踪</summary>
        private bool _showStackTrace = false;
        
        /// <summary>是否自动滚动到底部</summary>
        private bool _autoScrollToBottom = true;
        
        /// <summary>性能数据排序方式</summary>
        private int _performanceSortMode = 0; // 0: 时间, 1: 操作名称, 2: 执行时间
        
        /// <summary>性能数据排序方向</summary>
        private bool _performanceSortAscending = false;
        
        /// <summary>GUI样式缓存</summary>
        private GUIStyle _logEntryStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _infoStyle;

        #endregion

        #region 窗口管理

        /// <summary>
        /// 显示调试面板窗口
        /// </summary>
        public static void ShowWindow()
        {
            DebugPanelWindow window = GetWindow<DebugPanelWindow>();
            window.titleContent = new GUIContent("测试工具调试面板", "查看测试工具的调试信息、性能数据和事件统计");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        /// <summary>
        /// 窗口启用时调用
        /// </summary>
        private void OnEnable()
        {
            _debugManager = DebugManager.Instance;
            InitializeStyles();
        }

        /// <summary>
        /// 初始化GUI样式
        /// </summary>
        private void InitializeStyles()
        {
            _logEntryStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
                fontSize = 11
            };

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };

            _errorStyle = new GUIStyle(_logEntryStyle)
            {
                normal = { textColor = Color.red }
            };

            _warningStyle = new GUIStyle(_logEntryStyle)
            {
                normal = { textColor = Color.yellow }
            };

            _infoStyle = new GUIStyle(_logEntryStyle)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
        }

        /// <summary>
        /// 绘制窗口GUI
        /// </summary>
        private void OnGUI()
        {
            if (_debugManager == null)
            {
                EditorGUILayout.HelpBox("调试管理器未初始化", MessageType.Error);
                return;
            }

            DrawHeader();
            DrawTabBar();
            DrawTabContent();
        }

        #endregion

        #region UI绘制方法

        /// <summary>
        /// 绘制窗口头部
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("测试工具调试面板", EditorStyles.toolbarButton);
                
                GUILayout.FlexibleSpace();
                
                // 调试开关
                bool debugEnabled = _debugManager.IsDebugEnabled;
                bool newDebugEnabled = GUILayout.Toggle(debugEnabled, "启用调试", EditorStyles.toolbarButton);
                if (newDebugEnabled != debugEnabled)
                {
                    _debugManager.IsDebugEnabled = newDebugEnabled;
                    _debugManager.SaveDebugSettings();
                }
                
                // 刷新按钮
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
                {
                    Repaint();
                }
                
                // 清除数据按钮
                if (GUILayout.Button("清除", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有调试数据吗？", "确定", "取消"))
                    {
                        _debugManager.ClearAllDebugData();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制标签页栏
        /// </summary>
        private void DrawTabBar()
        {
            _selectedTabIndex = GUILayout.Toolbar(_selectedTabIndex, _tabNames);
        }

        /// <summary>
        /// 绘制标签页内容
        /// </summary>
        private void DrawTabContent()
        {
            switch (_selectedTabIndex)
            {
                case 0:
                    DrawLogTab();
                    break;
                case 1:
                    DrawPerformanceTab();
                    break;
                case 2:
                    DrawEventStatisticsTab();
                    break;
                case 3:
                    DrawSettingsTab();
                    break;
            }
        }

        /// <summary>
        /// 绘制日志标签页
        /// </summary>
        private void DrawLogTab()
        {
            EditorGUILayout.BeginVertical();
            {
                // 过滤器控件
                DrawLogFilters();
                
                EditorGUILayout.Space();
                
                // 日志列表
                DrawLogList();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制日志过滤器
        /// </summary>
        private void DrawLogFilters()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("过滤器:", GUILayout.Width(50));
                
                // 级别过滤器
                GUILayout.Label("级别:", GUILayout.Width(35));
                _logLevelFilter = (DebugLevel)EditorGUILayout.EnumPopup(_logLevelFilter, GUILayout.Width(80));
                
                GUILayout.Space(10);
                
                // 分类过滤器
                GUILayout.Label("分类:", GUILayout.Width(35));
                _logCategoryFilter = EditorGUILayout.TextField(_logCategoryFilter, GUILayout.Width(120));
                
                GUILayout.FlexibleSpace();
                
                // 显示选项
                _showStackTrace = GUILayout.Toggle(_showStackTrace, "显示堆栈", GUILayout.Width(80));
                _autoScrollToBottom = GUILayout.Toggle(_autoScrollToBottom, "自动滚动", GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制日志列表
        /// </summary>
        private void DrawLogList()
        {
            var logs = _debugManager.DebugLogs;
            var filteredLogs = FilterLogs(logs);
            
            EditorGUILayout.LabelField($"日志条目: {filteredLogs.Count} / {logs.Count}", _headerStyle);
            
            _logScrollPosition = EditorGUILayout.BeginScrollView(_logScrollPosition);
            {
                foreach (var log in filteredLogs)
                {
                    DrawLogEntry(log);
                }
                
                // 自动滚动到底部
                if (_autoScrollToBottom && Event.current.type == EventType.Repaint)
                {
                    _logScrollPosition.y = float.MaxValue;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 过滤日志条目
        /// </summary>
        /// <param name="logs">原始日志列表</param>
        /// <returns>过滤后的日志列表</returns>
        private List<DebugLogEntry> FilterLogs(IReadOnlyList<DebugLogEntry> logs)
        {
            var filtered = new List<DebugLogEntry>();
            
            foreach (var log in logs)
            {
                // 级别过滤
                if (log.level < _logLevelFilter)
                    continue;
                
                // 分类过滤
                if (!string.IsNullOrEmpty(_logCategoryFilter) && 
                    !log.category.ToLower().Contains(_logCategoryFilter.ToLower()))
                    continue;
                
                filtered.Add(log);
            }
            
            return filtered;
        }

        /// <summary>
        /// 绘制单个日志条目
        /// </summary>
        /// <param name="log">日志条目</param>
        private void DrawLogEntry(DebugLogEntry log)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                // 日志头部信息
                EditorGUILayout.BeginHorizontal();
                {
                    string timeStr = log.timestamp.ToString("HH:mm:ss.fff");
                    string levelStr = log.level.ToString();
                    
                    GUIStyle style = GetLogStyle(log.level);
                    GUILayout.Label($"[{timeStr}] [{levelStr}] [{log.category}]", style, GUILayout.Width(200));
                    
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                
                // 日志消息
                EditorGUILayout.SelectableLabel(log.message, _logEntryStyle, GUILayout.MinHeight(20));
                
                // 堆栈跟踪
                if (_showStackTrace && !string.IsNullOrEmpty(log.stackTrace))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.Label("堆栈跟踪:", EditorStyles.boldLabel);
                        EditorGUILayout.SelectableLabel(log.stackTrace, _logEntryStyle, GUILayout.MinHeight(60));
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 根据日志级别获取对应的GUI样式
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns>GUI样式</returns>
        private GUIStyle GetLogStyle(DebugLevel level)
        {
            switch (level)
            {
                case DebugLevel.Error:
                    return _errorStyle;
                case DebugLevel.Warning:
                    return _warningStyle;
                default:
                    return _infoStyle;
            }
        }

        /// <summary>
        /// 绘制性能标签页
        /// </summary>
        private void DrawPerformanceTab()
        {
            EditorGUILayout.BeginVertical();
            {
                // 性能数据控件
                DrawPerformanceControls();
                
                EditorGUILayout.Space();
                
                // 性能数据列表
                DrawPerformanceList();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制性能数据控件
        /// </summary>
        private void DrawPerformanceControls()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("排序:", GUILayout.Width(35));
                
                string[] sortOptions = { "时间", "操作名称", "执行时间" };
                _performanceSortMode = EditorGUILayout.Popup(_performanceSortMode, sortOptions, GUILayout.Width(100));
                
                _performanceSortAscending = GUILayout.Toggle(_performanceSortAscending, "升序", GUILayout.Width(50));
                
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制性能数据列表
        /// </summary>
        private void DrawPerformanceList()
        {
            var performanceData = _debugManager.PerformanceData;
            var sortedData = SortPerformanceData(performanceData);
            
            EditorGUILayout.LabelField($"性能数据条目: {performanceData.Count}", _headerStyle);
            
            // 显示平均执行时间统计
            DrawAveragePerformanceStats();
            
            EditorGUILayout.Space();
            
            _performanceScrollPosition = EditorGUILayout.BeginScrollView(_performanceScrollPosition);
            {
                // 表头
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("操作名称", EditorStyles.boldLabel, GUILayout.Width(200));
                    GUILayout.Label("执行时间(ms)", EditorStyles.boldLabel, GUILayout.Width(100));
                    GUILayout.Label("开始时间", EditorStyles.boldLabel, GUILayout.Width(120));
                    GUILayout.Label("结束时间", EditorStyles.boldLabel, GUILayout.Width(120));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // 数据行
                foreach (var data in sortedData)
                {
                    DrawPerformanceEntry(data);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制平均执行时间统计
        /// </summary>
        private void DrawAveragePerformanceStats()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("平均执行时间统计", _headerStyle);
                
                // 获取各个操作的平均执行时间
                var avgOnGUI = _debugManager.GetAverageExecutionTime("OnGUI");
                var avgLeftSidebar = _debugManager.GetAverageExecutionTime("DrawLeftSidebar");
                var avgRightPanel = _debugManager.GetAverageExecutionTime("DrawRightPanel");
                var avgBottomBar = _debugManager.GetAverageExecutionTime("DrawBottomBar");
                
                // 获取所有操作的统计信息
                var operationStats = GetOperationStatistics();
                
                if (operationStats.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // 左列：主要UI组件
                        EditorGUILayout.BeginVertical(GUILayout.Width(200));
                        {
                            EditorGUILayout.LabelField("主要UI组件:", EditorStyles.boldLabel);
                            
                            if (avgOnGUI > 0)
                            {
                                Color originalColor = GUI.color;
                                if (avgOnGUI > 50) GUI.color = Color.red;
                                else if (avgOnGUI > 20) GUI.color = Color.yellow;
                                
                                EditorGUILayout.LabelField($"OnGUI: {avgOnGUI:F2}ms");
                                GUI.color = originalColor;
                            }
                            
                            if (avgLeftSidebar > 0)
                            {
                                Color originalColor = GUI.color;
                                if (avgLeftSidebar > 20) GUI.color = Color.red;
                                else if (avgLeftSidebar > 10) GUI.color = Color.yellow;
                                
                                EditorGUILayout.LabelField($"左侧边栏: {avgLeftSidebar:F2}ms");
                                GUI.color = originalColor;
                            }
                            
                            if (avgRightPanel > 0)
                            {
                                Color originalColor = GUI.color;
                                if (avgRightPanel > 20) GUI.color = Color.red;
                                else if (avgRightPanel > 10) GUI.color = Color.yellow;
                                
                                EditorGUILayout.LabelField($"右侧面板: {avgRightPanel:F2}ms");
                                GUI.color = originalColor;
                            }
                            
                            if (avgBottomBar > 0)
                            {
                                Color originalColor = GUI.color;
                                if (avgBottomBar > 10) GUI.color = Color.red;
                                else if (avgBottomBar > 5) GUI.color = Color.yellow;
                                
                                EditorGUILayout.LabelField($"底部栏: {avgBottomBar:F2}ms");
                                GUI.color = originalColor;
                            }
                        }
                        EditorGUILayout.EndVertical();
                        
                        // 右列：其他操作统计
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("操作统计:", EditorStyles.boldLabel);
                            
                            foreach (var stat in operationStats.Take(8)) // 只显示前8个
                            {
                                if (stat.Key != "OnGUI" && stat.Key != "DrawLeftSidebar" && 
                                    stat.Key != "DrawRightPanel" && stat.Key != "DrawBottomBar")
                                {
                                    Color originalColor = GUI.color;
                                    if (stat.Value.averageTime > 10) GUI.color = Color.red;
                                    else if (stat.Value.averageTime > 5) GUI.color = Color.yellow;
                                    
                                    EditorGUILayout.LabelField($"{stat.Key}: {stat.Value.averageTime:F2}ms (x{stat.Value.count})");
                                    GUI.color = originalColor;
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    // 性能提示
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("性能提示:", EditorStyles.boldLabel);
                    
                    var totalAvgTime = avgOnGUI + avgLeftSidebar + avgRightPanel + avgBottomBar;
                    if (totalAvgTime > 100)
                    {
                        EditorGUILayout.HelpBox("UI绘制总时间较长，可能影响编辑器响应速度", MessageType.Warning);
                    }
                    else if (totalAvgTime > 50)
                    {
                        EditorGUILayout.HelpBox("UI绘制时间适中，注意监控性能变化", MessageType.Info);
                    }
                    else if (totalAvgTime > 0)
                    {
                        EditorGUILayout.HelpBox("UI绘制性能良好", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("暂无性能数据", EditorStyles.centeredGreyMiniLabel);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 获取操作统计信息
        /// </summary>
        /// <returns>操作统计字典</returns>
        private Dictionary<string, (double averageTime, int count)> GetOperationStatistics()
        {
            var stats = new Dictionary<string, (double averageTime, int count)>();
            var performanceData = _debugManager.PerformanceData;
            
            // 按操作名称分组统计
            var groupedData = performanceData.GroupBy(p => p.operationName);
            
            foreach (var group in groupedData)
            {
                var avgTime = group.Average(p => p.executionTimeMs);
                var count = group.Count();
                stats[group.Key] = (avgTime, count);
            }
            
            // 按平均时间降序排序
            return stats.OrderByDescending(kvp => kvp.Value.averageTime).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// 排序性能数据
        /// </summary>
        /// <param name="data">原始性能数据</param>
        /// <returns>排序后的性能数据</returns>
        private List<PerformanceData> SortPerformanceData(IReadOnlyList<PerformanceData> data)
        {
            var sorted = data.ToList();
            
            switch (_performanceSortMode)
            {
                case 0: // 时间
                    sorted = _performanceSortAscending 
                        ? sorted.OrderBy(d => d.startTime).ToList()
                        : sorted.OrderByDescending(d => d.startTime).ToList();
                    break;
                case 1: // 操作名称
                    sorted = _performanceSortAscending
                        ? sorted.OrderBy(d => d.operationName).ToList()
                        : sorted.OrderByDescending(d => d.operationName).ToList();
                    break;
                case 2: // 执行时间
                    sorted = _performanceSortAscending
                        ? sorted.OrderBy(d => d.executionTimeMs).ToList()
                        : sorted.OrderByDescending(d => d.executionTimeMs).ToList();
                    break;
            }
            
            return sorted;
        }

        /// <summary>
        /// 绘制单个性能数据条目
        /// </summary>
        /// <param name="data">性能数据</param>
        private void DrawPerformanceEntry(PerformanceData data)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label(data.operationName, GUILayout.Width(200));
                
                // 根据执行时间设置颜色
                Color originalColor = GUI.color;
                if (data.executionTimeMs > 100) // 超过100ms显示红色
                    GUI.color = Color.red;
                else if (data.executionTimeMs > 50) // 超过50ms显示黄色
                    GUI.color = Color.yellow;
                
                GUILayout.Label($"{data.executionTimeMs:F2}", GUILayout.Width(100));
                GUI.color = originalColor;
                
                GUILayout.Label(data.startTime.ToString("HH:mm:ss.fff"), GUILayout.Width(120));
                GUILayout.Label(data.endTime.ToString("HH:mm:ss.fff"), GUILayout.Width(120));
                
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制事件统计标签页
        /// </summary>
        private void DrawEventStatisticsTab()
        {
            EditorGUILayout.BeginVertical();
            {
                var eventStats = _debugManager.EventStatistics;
                
                EditorGUILayout.LabelField($"事件统计条目: {eventStats.Count}", _headerStyle);
                
                _eventScrollPosition = EditorGUILayout.BeginScrollView(_eventScrollPosition);
                {
                    // 表头
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("事件名称", EditorStyles.boldLabel, GUILayout.Width(300));
                        GUILayout.Label("触发次数", EditorStyles.boldLabel, GUILayout.Width(100));
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space();
                    
                    // 按触发次数排序显示
                    var sortedStats = eventStats.OrderByDescending(kvp => kvp.Value);
                    
                    foreach (var kvp in sortedStats)
                    {
                        EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        {
                            GUILayout.Label(kvp.Key, GUILayout.Width(300));
                            GUILayout.Label(kvp.Value.ToString(), GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space();
                
                // 重置按钮
                if (GUILayout.Button("重置事件统计", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有事件统计数据吗？", "确定", "取消"))
                    {
                        _debugManager.ResetEventStatistics();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制设置标签页
        /// </summary>
        private void DrawSettingsTab()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("调试设置", _headerStyle);
                EditorGUILayout.Space();
                
                // 调试开关
                bool debugEnabled = _debugManager.IsDebugEnabled;
                bool newDebugEnabled = EditorGUILayout.Toggle("启用调试功能", debugEnabled);
                if (newDebugEnabled != debugEnabled)
                {
                    _debugManager.IsDebugEnabled = newDebugEnabled;
                }
                
                // 调试级别
                DebugLevel currentLevel = _debugManager.CurrentDebugLevel;
                DebugLevel newLevel = (DebugLevel)EditorGUILayout.EnumPopup("调试级别", currentLevel);
                if (newLevel != currentLevel)
                {
                    _debugManager.CurrentDebugLevel = newLevel;
                }
                
                EditorGUILayout.Space();
                
                // 保存设置按钮
                if (GUILayout.Button("保存设置", GUILayout.Height(25)))
                {
                    _debugManager.SaveDebugSettings();
                    ShowNotification(new GUIContent("设置已保存"));
                }
                
                EditorGUILayout.Space();
                
                GUILayout.Label("数据管理", _headerStyle);
                EditorGUILayout.Space();
                
                // 数据管理按钮
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("清除日志"))
                    {
                        _debugManager.ClearDebugLogs();
                    }
                    
                    if (GUILayout.Button("清除性能数据"))
                    {
                        _debugManager.ClearPerformanceData();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("清除所有数据", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有调试数据吗？", "确定", "取消"))
                    {
                        _debugManager.ClearAllDebugData();
                    }
                }
                
                EditorGUILayout.Space();
                
                // 导出数据按钮
                if (GUILayout.Button("导出调试数据", GUILayout.Height(25)))
                {
                    ExportDebugData();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 导出调试数据
        /// </summary>
        private void ExportDebugData()
        {
            string debugData = _debugManager.ExportDebugDataAsText();
            string filePath = EditorUtility.SaveFilePanel(
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
                    ShowNotification(new GUIContent("调试数据导出成功"));
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog("导出失败", $"导出调试数据失败: {ex.Message}", "确定");
                }
            }
        }

        #endregion

        #region 窗口更新

        /// <summary>
        /// 定期更新窗口内容
        /// </summary>
        private void Update()
        {
            // 每秒刷新一次
            if (Time.realtimeSinceStartup % 1.0f < 0.1f)
            {
                Repaint();
            }
        }

        #endregion
    }
}