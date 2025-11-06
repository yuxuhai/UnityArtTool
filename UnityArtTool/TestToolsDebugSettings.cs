/**
 * 文件名: TestToolsDebugSettings.cs
 * 作者: yuxuhai
 * 日期: 2024
 * 描述: 测试工具调试设置提供者，集成到Unity的设置系统中，允许用户在偏好设置中配置调试选项
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtTools
{
    /// <summary>
    /// 测试工具调试设置提供者
    /// 集成到Unity的设置系统中，允许用户在偏好设置中配置调试选项
    /// </summary>
    public class TestToolsDebugSettings : SettingsProvider
    {
        #region 设置键常量

        /// <summary>调试功能启用状态的设置键</summary>
        private const string DEBUG_ENABLED_KEY = "TestTools.Debug.Enabled";
        
        /// <summary>调试级别的设置键</summary>
        private const string DEBUG_LEVEL_KEY = "TestTools.Debug.Level";
        
        /// <summary>自动清理日志的设置键</summary>
        private const string AUTO_CLEANUP_LOGS_KEY = "TestTools.Debug.AutoCleanupLogs";
        
        /// <summary>最大日志条目数的设置键</summary>
        private const string MAX_LOG_ENTRIES_KEY = "TestTools.Debug.MaxLogEntries";
        
        /// <summary>最大性能数据条目数的设置键</summary>
        private const string MAX_PERFORMANCE_ENTRIES_KEY = "TestTools.Debug.MaxPerformanceEntries";
        
        /// <summary>性能监控启用状态的设置键</summary>
        private const string PERFORMANCE_MONITORING_ENABLED_KEY = "TestTools.Debug.PerformanceMonitoringEnabled";

        #endregion

        #region 私有字段

        /// <summary>调试管理器实例</summary>
        private DebugManager _debugManager;
        
        /// <summary>设置是否已初始化</summary>
        private bool _isInitialized = false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">设置路径</param>
        /// <param name="scopes">设置作用域</param>
        public TestToolsDebugSettings(string path, SettingsScope scopes = SettingsScope.User)
            : base(path, scopes)
        {
        }

        #endregion

        #region 设置提供者注册

        /// <summary>
        /// 创建设置提供者
        /// </summary>
        /// <returns>设置提供者实例</returns>
        [SettingsProvider]
        public static SettingsProvider CreateTestToolsDebugSettingsProvider()
        {
            var provider = new TestToolsDebugSettings("Preferences/测试工具/调试设置", SettingsScope.User);
            
            // 设置关键词用于搜索
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            
            return provider;
        }

        #endregion

        #region 设置界面绘制

        /// <summary>
        /// 绘制设置界面
        /// </summary>
        /// <param name="searchContext">搜索上下文</param>
        public override void OnGUI(string searchContext)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            DrawDebugSettings();
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        private void Initialize()
        {
            _debugManager = DebugManager.Instance;
            _isInitialized = true;
        }

        /// <summary>
        /// 绘制调试设置界面
        /// </summary>
        private void DrawDebugSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("测试工具调试设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 基本调试设置
            DrawBasicDebugSettings();
            
            EditorGUILayout.Space();
            
            // 日志设置
            DrawLogSettings();
            
            EditorGUILayout.Space();
            
            // 性能监控设置
            DrawPerformanceSettings();
            
            EditorGUILayout.Space();
            
            // 数据管理
            DrawDataManagement();
            
            EditorGUILayout.Space();
            
            // 操作按钮
            DrawActionButtons();
        }

        /// <summary>
        /// 绘制基本调试设置
        /// </summary>
        private void DrawBasicDebugSettings()
        {
            EditorGUILayout.LabelField("基本设置", EditorStyles.boldLabel);
            
            // 调试功能开关
            bool debugEnabled = EditorPrefs.GetBool(DEBUG_ENABLED_KEY, true);
            bool newDebugEnabled = EditorGUILayout.Toggle(
                new GUIContent("启用调试功能", "开启或关闭测试工具的调试功能"), 
                debugEnabled
            );
            if (newDebugEnabled != debugEnabled)
            {
                EditorPrefs.SetBool(DEBUG_ENABLED_KEY, newDebugEnabled);
                if (_debugManager != null)
                {
                    _debugManager.IsDebugEnabled = newDebugEnabled;
                }
            }

            // 调试级别
            DebugLevel debugLevel = (DebugLevel)EditorPrefs.GetInt(DEBUG_LEVEL_KEY, (int)DebugLevel.Info);
            DebugLevel newDebugLevel = (DebugLevel)EditorGUILayout.EnumPopup(
                new GUIContent("调试级别", "设置调试信息的详细程度"), 
                debugLevel
            );
            if (newDebugLevel != debugLevel)
            {
                EditorPrefs.SetInt(DEBUG_LEVEL_KEY, (int)newDebugLevel);
                if (_debugManager != null)
                {
                    _debugManager.CurrentDebugLevel = newDebugLevel;
                }
            }

            // 性能监控开关
            bool performanceEnabled = EditorPrefs.GetBool(PERFORMANCE_MONITORING_ENABLED_KEY, true);
            bool newPerformanceEnabled = EditorGUILayout.Toggle(
                new GUIContent("启用性能监控", "开启或关闭UI性能监控功能"), 
                performanceEnabled
            );
            if (newPerformanceEnabled != performanceEnabled)
            {
                EditorPrefs.SetBool(PERFORMANCE_MONITORING_ENABLED_KEY, newPerformanceEnabled);
            }
        }

        /// <summary>
        /// 绘制日志设置
        /// </summary>
        private void DrawLogSettings()
        {
            EditorGUILayout.LabelField("日志设置", EditorStyles.boldLabel);
            
            // 自动清理日志
            bool autoCleanup = EditorPrefs.GetBool(AUTO_CLEANUP_LOGS_KEY, true);
            bool newAutoCleanup = EditorGUILayout.Toggle(
                new GUIContent("自动清理日志", "当日志条目超过最大数量时自动清理旧日志"), 
                autoCleanup
            );
            if (newAutoCleanup != autoCleanup)
            {
                EditorPrefs.SetBool(AUTO_CLEANUP_LOGS_KEY, newAutoCleanup);
            }

            // 最大日志条目数
            int maxLogEntries = EditorPrefs.GetInt(MAX_LOG_ENTRIES_KEY, 1000);
            int newMaxLogEntries = EditorGUILayout.IntSlider(
                new GUIContent("最大日志条目数", "保留的最大日志条目数量"), 
                maxLogEntries, 100, 5000
            );
            if (newMaxLogEntries != maxLogEntries)
            {
                EditorPrefs.SetInt(MAX_LOG_ENTRIES_KEY, newMaxLogEntries);
            }

            // 显示当前日志统计
            if (_debugManager != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"当前日志条目: {_debugManager.DebugLogs.Count}", EditorStyles.helpBox);
            }
        }

        /// <summary>
        /// 绘制性能监控设置
        /// </summary>
        private void DrawPerformanceSettings()
        {
            EditorGUILayout.LabelField("性能监控设置", EditorStyles.boldLabel);
            
            // 最大性能数据条目数
            int maxPerfEntries = EditorPrefs.GetInt(MAX_PERFORMANCE_ENTRIES_KEY, 500);
            int newMaxPerfEntries = EditorGUILayout.IntSlider(
                new GUIContent("最大性能数据条目数", "保留的最大性能监控数据条目数量"), 
                maxPerfEntries, 50, 2000
            );
            if (newMaxPerfEntries != maxPerfEntries)
            {
                EditorPrefs.SetInt(MAX_PERFORMANCE_ENTRIES_KEY, newMaxPerfEntries);
            }

            // 显示当前性能数据统计
            if (_debugManager != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"当前性能数据条目: {_debugManager.PerformanceData.Count}", EditorStyles.helpBox);
                
                // 提示用户在调试面板查看详细统计
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("详细的平均执行时间统计和性能分析请在调试面板的<性能>标签页中查看", MessageType.Info);
                
                if (GUILayout.Button("打开调试面板查看详细统计", GUILayout.Height(25)))
                {
                    DebugPanelWindow.ShowWindow();
                }
            }
        }

        /// <summary>
        /// 绘制数据管理区域
        /// </summary>
        private void DrawDataManagement()
        {
            EditorGUILayout.LabelField("数据管理", EditorStyles.boldLabel);
            
            if (_debugManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("清除日志"))
                {
                    if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有调试日志吗？", "确定", "取消"))
                    {
                        _debugManager.ClearDebugLogs();
                    }
                }
                
                if (GUILayout.Button("清除性能数据"))
                {
                    if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有性能数据吗？", "确定", "取消"))
                    {
                        _debugManager.ClearPerformanceData();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("清除所有调试数据"))
                {
                    if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有调试数据吗？", "确定", "取消"))
                    {
                        _debugManager.ClearAllDebugData();
                    }
                }
            }
        }

        /// <summary>
        /// 绘制操作按钮区域
        /// </summary>
        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("打开调试面板"))
            {
                DebugPanelWindow.ShowWindow();
            }
            
            if (GUILayout.Button("导出调试数据"))
            {
                ExportDebugData();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("重置所有设置"))
            {
                if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有调试设置到默认值吗？", "确定", "取消"))
                {
                    ResetAllSettings();
                }
            }
        }

        /// <summary>
        /// 导出调试数据
        /// </summary>
        private void ExportDebugData()
        {
            if (_debugManager != null)
            {
                string debugData = _debugManager.ExportDebugDataAsText();
                string filePath = EditorUtility.SaveFilePanel(
                    "导出调试数据", 
                    Application.dataPath, 
                    $"TestTools_Debug_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt", 
                    "txt"
                );
                
                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        System.IO.File.WriteAllText(filePath, debugData);
                        EditorUtility.DisplayDialog("导出成功", $"调试数据已导出到:\n{filePath}", "确定");
                    }
                    catch (System.Exception ex)
                    {
                        EditorUtility.DisplayDialog("导出失败", $"导出调试数据失败:\n{ex.Message}", "确定");
                    }
                }
            }
        }

        /// <summary>
        /// 重置所有设置到默认值
        /// </summary>
        private void ResetAllSettings()
        {
            EditorPrefs.DeleteKey(DEBUG_ENABLED_KEY);
            EditorPrefs.DeleteKey(DEBUG_LEVEL_KEY);
            EditorPrefs.DeleteKey(AUTO_CLEANUP_LOGS_KEY);
            EditorPrefs.DeleteKey(MAX_LOG_ENTRIES_KEY);
            EditorPrefs.DeleteKey(MAX_PERFORMANCE_ENTRIES_KEY);
            EditorPrefs.DeleteKey(PERFORMANCE_MONITORING_ENABLED_KEY);
            
            // 重新初始化调试管理器设置
            if (_debugManager != null)
            {
                _debugManager.IsDebugEnabled = true;
                _debugManager.CurrentDebugLevel = DebugLevel.Info;
                _debugManager.SaveDebugSettings();
            }
            
            EditorUtility.DisplayDialog("重置完成", "所有调试设置已重置到默认值", "确定");
        }

        #endregion

        #region 样式类

        /// <summary>
        /// GUI样式和内容定义
        /// </summary>
        private class Styles
        {
            public static readonly GUIContent debugEnabled = new GUIContent("启用调试功能");
            public static readonly GUIContent debugLevel = new GUIContent("调试级别");
            public static readonly GUIContent performanceMonitoring = new GUIContent("性能监控");
            public static readonly GUIContent autoCleanup = new GUIContent("自动清理");
            public static readonly GUIContent maxEntries = new GUIContent("最大条目数");
            public static readonly GUIContent dataManagement = new GUIContent("数据管理");
            public static readonly GUIContent export = new GUIContent("导出");
            public static readonly GUIContent reset = new GUIContent("重置");
        }

        #endregion
    }
}