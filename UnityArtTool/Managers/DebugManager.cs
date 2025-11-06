/**
 * 文件名: DebugManager.cs
 * 作者: yuxuhai
 * 日期: 2024
 * 描述: 测试工具调试管理器，负责调试功能的统一管理、性能监控、日志记录和调试数据收集
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 调试日志级别枚举
    /// </summary>
    public enum DebugLevel
    {
        /// <summary>详细信息</summary>
        Verbose = 0,
        /// <summary>调试信息</summary>
        Debug = 1,
        /// <summary>一般信息</summary>
        Info = 2,
        /// <summary>警告信息</summary>
        Warning = 3,
        /// <summary>错误信息</summary>
        Error = 4
    }

    /// <summary>
    /// 调试日志条目
    /// </summary>
    [Serializable]
    public class DebugLogEntry
    {
        /// <summary>日志时间戳</summary>
        public DateTime timestamp;
        /// <summary>日志级别</summary>
        public DebugLevel level;
        /// <summary>日志分类</summary>
        public string category;
        /// <summary>日志消息</summary>
        public string message;
        /// <summary>调用堆栈信息</summary>
        public string stackTrace;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="stackTrace">调用堆栈</param>
        public DebugLogEntry(DebugLevel level, string category, string message, string stackTrace = null)
        {
            this.timestamp = DateTime.Now;
            this.level = level;
            this.category = category;
            this.message = message;
            this.stackTrace = stackTrace;
        }
    }

    /// <summary>
    /// 性能监控数据
    /// </summary>
    [Serializable]
    public class PerformanceData
    {
        /// <summary>操作名称</summary>
        public string operationName;
        /// <summary>执行时间（毫秒）</summary>
        public double executionTimeMs;
        /// <summary>开始时间</summary>
        public DateTime startTime;
        /// <summary>结束时间</summary>
        public DateTime endTime;
        /// <summary>额外数据</summary>
        public Dictionary<string, object> additionalData;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="executionTimeMs">执行时间</param>
        public PerformanceData(string operationName, double executionTimeMs)
        {
            this.operationName = operationName;
            this.executionTimeMs = executionTimeMs;
            this.startTime = DateTime.Now.AddMilliseconds(-executionTimeMs);
            this.endTime = DateTime.Now;
            this.additionalData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 测试工具调试管理器
    /// 负责调试功能的统一管理、性能监控、日志记录和调试数据收集
    /// </summary>
    public class DebugManager
    {
        #region 单例模式

        /// <summary>单例实例</summary>
        private static DebugManager _instance;
        
        /// <summary>获取单例实例</summary>
        public static DebugManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DebugManager();
                }
                return _instance;
            }
        }

        #endregion

        #region 私有字段

        /// <summary>是否启用调试功能</summary>
        private bool _isDebugEnabled = true;
        
        /// <summary>当前调试级别</summary>
        private DebugLevel _currentDebugLevel = DebugLevel.Info;
        
        /// <summary>调试日志列表</summary>
        private List<DebugLogEntry> _debugLogs = new List<DebugLogEntry>();
        
        /// <summary>性能监控数据列表</summary>
        private List<PerformanceData> _performanceData = new List<PerformanceData>();
        
        /// <summary>活动的性能监控器字典</summary>
        private Dictionary<string, Stopwatch> _activeStopwatches = new Dictionary<string, Stopwatch>();
        
        /// <summary>事件统计数据</summary>
        private Dictionary<string, int> _eventStatistics = new Dictionary<string, int>();
        
        /// <summary>最大日志条目数</summary>
        private const int MAX_LOG_ENTRIES = 1000;
        
        /// <summary>最大性能数据条目数</summary>
        private const int MAX_PERFORMANCE_ENTRIES = 500;

        #endregion

        #region 公共属性

        /// <summary>是否启用调试功能</summary>
        public bool IsDebugEnabled
        {
            get => _isDebugEnabled;
            set => _isDebugEnabled = value;
        }

        /// <summary>当前调试级别</summary>
        public DebugLevel CurrentDebugLevel
        {
            get => _currentDebugLevel;
            set => _currentDebugLevel = value;
        }

        /// <summary>获取调试日志列表（只读）</summary>
        public IReadOnlyList<DebugLogEntry> DebugLogs => _debugLogs.AsReadOnly();

        /// <summary>获取性能监控数据列表（只读）</summary>
        public IReadOnlyList<PerformanceData> PerformanceData => _performanceData.AsReadOnly();

        /// <summary>获取事件统计数据（只读）</summary>
        public IReadOnlyDictionary<string, int> EventStatistics => _eventStatistics;

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private DebugManager()
        {
            // 从EditorPrefs加载调试设置
            LoadDebugSettings();
            
            // 记录初始化日志
            LogDebug("DebugManager", "调试管理器已初始化");
        }

        #endregion

        #region 日志记录方法

        /// <summary>
        /// 记录详细信息日志
        /// </summary>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="includeStackTrace">是否包含调用堆栈</param>
        public void LogVerbose(string category, string message, bool includeStackTrace = false)
        {
            LogMessage(DebugLevel.Verbose, category, message, includeStackTrace);
        }

        /// <summary>
        /// 记录调试信息日志
        /// </summary>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="includeStackTrace">是否包含调用堆栈</param>
        public void LogDebug(string category, string message, bool includeStackTrace = false)
        {
            LogMessage(DebugLevel.Debug, category, message, includeStackTrace);
        }

        /// <summary>
        /// 记录一般信息日志
        /// </summary>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="includeStackTrace">是否包含调用堆栈</param>
        public void LogInfo(string category, string message, bool includeStackTrace = false)
        {
            LogMessage(DebugLevel.Info, category, message, includeStackTrace);
        }

        /// <summary>
        /// 记录警告信息日志
        /// </summary>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="includeStackTrace">是否包含调用堆栈</param>
        public void LogWarning(string category, string message, bool includeStackTrace = true)
        {
            LogMessage(DebugLevel.Warning, category, message, includeStackTrace);
        }

        /// <summary>
        /// 记录错误信息日志
        /// </summary>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void LogError(string category, string message, Exception exception = null)
        {
            string stackTrace = exception?.StackTrace ?? Environment.StackTrace;
            LogMessage(DebugLevel.Error, category, message, true, stackTrace);
        }

        /// <summary>
        /// 记录日志消息的核心方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="category">日志分类</param>
        /// <param name="message">日志消息</param>
        /// <param name="includeStackTrace">是否包含调用堆栈</param>
        /// <param name="customStackTrace">自定义调用堆栈</param>
        private void LogMessage(DebugLevel level, string category, string message, bool includeStackTrace = false, string customStackTrace = null)
        {
            if (!_isDebugEnabled || level < _currentDebugLevel)
                return;

            string stackTrace = null;
            if (includeStackTrace)
            {
                stackTrace = customStackTrace ?? Environment.StackTrace;
            }

            var logEntry = new DebugLogEntry(level, category, message, stackTrace);
            _debugLogs.Add(logEntry);

            // 限制日志条目数量
            if (_debugLogs.Count > MAX_LOG_ENTRIES)
            {
                _debugLogs.RemoveAt(0);
            }

            // 同时输出到Unity控制台
            OutputToUnityConsole(logEntry);
        }

        /// <summary>
        /// 将日志输出到Unity控制台
        /// </summary>
        /// <param name="logEntry">日志条目</param>
        private void OutputToUnityConsole(DebugLogEntry logEntry)
        {
            string formattedMessage = $"[{logEntry.category}] {logEntry.message}";
            
            switch (logEntry.level)
            {
                case DebugLevel.Verbose:
                case DebugLevel.Debug:
                case DebugLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case DebugLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case DebugLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }

        #endregion

        #region 性能监控方法

        /// <summary>
        /// 开始性能监控
        /// </summary>
        /// <param name="operationName">操作名称</param>
        public void StartPerformanceMonitoring(string operationName)
        {
            if (!_isDebugEnabled)
                return;

            if (_activeStopwatches.ContainsKey(operationName))
            {
                LogWarning("PerformanceMonitor", $"操作 '{operationName}' 的性能监控已经在运行中");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            _activeStopwatches[operationName] = stopwatch;
            
            LogVerbose("PerformanceMonitor", $"开始监控操作: {operationName}");
        }

        /// <summary>
        /// 结束性能监控
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <returns>执行时间（毫秒）</returns>
        public double EndPerformanceMonitoring(string operationName)
        {
            if (!_isDebugEnabled)
                return 0;

            if (!_activeStopwatches.TryGetValue(operationName, out Stopwatch stopwatch))
            {
                LogWarning("PerformanceMonitor", $"未找到操作 '{operationName}' 的性能监控");
                return 0;
            }

            stopwatch.Stop();
            double executionTime = stopwatch.Elapsed.TotalMilliseconds;
            _activeStopwatches.Remove(operationName);

            var performanceData = new PerformanceData(operationName, executionTime);
            _performanceData.Add(performanceData);

            // 限制性能数据条目数量
            if (_performanceData.Count > MAX_PERFORMANCE_ENTRIES)
            {
                _performanceData.RemoveAt(0);
            }

            LogVerbose("PerformanceMonitor", $"操作 '{operationName}' 完成，耗时: {executionTime:F2}ms");
            
            return executionTime;
        }

        /// <summary>
        /// 记录单次性能数据
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="executionTimeMs">执行时间（毫秒）</param>
        /// <param name="additionalData">额外数据</param>
        public void RecordPerformanceData(string operationName, double executionTimeMs, Dictionary<string, object> additionalData = null)
        {
            if (!_isDebugEnabled)
                return;

            var performanceData = new PerformanceData(operationName, executionTimeMs);
            if (additionalData != null)
            {
                performanceData.additionalData = additionalData;
            }

            _performanceData.Add(performanceData);

            // 限制性能数据条目数量
            if (_performanceData.Count > MAX_PERFORMANCE_ENTRIES)
            {
                _performanceData.RemoveAt(0);
            }

            LogVerbose("PerformanceMonitor", $"记录性能数据: {operationName} - {executionTimeMs:F2}ms");
        }

        /// <summary>
        /// 获取操作的平均执行时间
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <returns>平均执行时间（毫秒）</returns>
        public double GetAverageExecutionTime(string operationName)
        {
            var relevantData = _performanceData.FindAll(p => p.operationName == operationName);
            if (relevantData.Count == 0)
                return 0;

            double totalTime = 0;
            foreach (var data in relevantData)
            {
                totalTime += data.executionTimeMs;
            }

            return totalTime / relevantData.Count;
        }

        #endregion

        #region 事件统计方法

        /// <summary>
        /// 记录事件触发
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void RecordEvent(string eventName)
        {
            if (!_isDebugEnabled)
                return;

            if (_eventStatistics.ContainsKey(eventName))
            {
                _eventStatistics[eventName]++;
            }
            else
            {
                _eventStatistics[eventName] = 1;
            }

            LogVerbose("EventStatistics", $"事件触发: {eventName} (总计: {_eventStatistics[eventName]})");
        }

        /// <summary>
        /// 获取事件触发次数
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns>触发次数</returns>
        public int GetEventCount(string eventName)
        {
            return _eventStatistics.TryGetValue(eventName, out int count) ? count : 0;
        }

        /// <summary>
        /// 重置事件统计
        /// </summary>
        public void ResetEventStatistics()
        {
            _eventStatistics.Clear();
            LogInfo("EventStatistics", "事件统计已重置");
        }

        #endregion

        #region 数据管理方法

        /// <summary>
        /// 清除所有调试日志
        /// </summary>
        public void ClearDebugLogs()
        {
            _debugLogs.Clear();
            LogInfo("DebugManager", "调试日志已清除");
        }

        /// <summary>
        /// 清除所有性能数据
        /// </summary>
        public void ClearPerformanceData()
        {
            _performanceData.Clear();
            LogInfo("DebugManager", "性能数据已清除");
        }

        /// <summary>
        /// 清除所有调试数据
        /// </summary>
        public void ClearAllDebugData()
        {
            ClearDebugLogs();
            ClearPerformanceData();
            ResetEventStatistics();
            LogInfo("DebugManager", "所有调试数据已清除");
        }

        /// <summary>
        /// 导出调试数据为文本格式
        /// </summary>
        /// <returns>调试数据文本</returns>
        public string ExportDebugDataAsText()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=== 测试工具调试数据导出 ===");
            sb.AppendLine($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"调试级别: {_currentDebugLevel}");
            sb.AppendLine();

            // 导出日志数据
            sb.AppendLine("=== 调试日志 ===");
            foreach (var log in _debugLogs)
            {
                sb.AppendLine($"[{log.timestamp:HH:mm:ss.fff}] [{log.level}] [{log.category}] {log.message}");
                if (!string.IsNullOrEmpty(log.stackTrace))
                {
                    sb.AppendLine($"堆栈跟踪: {log.stackTrace}");
                }
                sb.AppendLine();
            }

            // 导出性能数据
            sb.AppendLine("=== 性能数据 ===");
            foreach (var perf in _performanceData)
            {
                sb.AppendLine($"{perf.operationName}: {perf.executionTimeMs:F2}ms ({perf.startTime:HH:mm:ss.fff} - {perf.endTime:HH:mm:ss.fff})");
            }
            sb.AppendLine();

            // 导出事件统计
            sb.AppendLine("=== 事件统计 ===");
            foreach (var kvp in _eventStatistics)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value} 次");
            }

            return sb.ToString();
        }

        #endregion

        #region 设置管理方法

        /// <summary>
        /// 从EditorPrefs加载调试设置
        /// </summary>
        private void LoadDebugSettings()
        {
            _isDebugEnabled = UnityEditor.EditorPrefs.GetBool("TestTools.Debug.Enabled", true);
            _currentDebugLevel = (DebugLevel)UnityEditor.EditorPrefs.GetInt("TestTools.Debug.Level", (int)DebugLevel.Info);
        }

        /// <summary>
        /// 保存调试设置到EditorPrefs
        /// </summary>
        public void SaveDebugSettings()
        {
            UnityEditor.EditorPrefs.SetBool("TestTools.Debug.Enabled", _isDebugEnabled);
            UnityEditor.EditorPrefs.SetInt("TestTools.Debug.Level", (int)_currentDebugLevel);
            
            LogInfo("DebugManager", "调试设置已保存");
        }

        #endregion
    }
}