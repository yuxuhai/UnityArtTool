/**
 * 文件名: ConfigurationManager.cs
 * 作用: 负责测试工具配置文件的加载、保存和管理
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 配置管理器类，负责处理测试工具配置文件的所有操作。
    /// 包括配置文件的发现、加载、保存、验证等功能。
    /// </summary>
    public class ConfigurationManager
    {        
        #region 事件定义
        
        /// <summary>
        /// 配置文件加载完成时触发的事件
        /// </summary>
        public event Action<TestToolsWindowData> OnConfigurationLoaded;
        
        /// <summary>
        /// 配置文件保存完成时触发的事件
        /// </summary>
        public event Action<TestToolsWindowData> OnConfigurationSaved;
        
        /// <summary>
        /// 配置文件列表更新时触发的事件
        /// </summary>
        public event Action<List<string>, string[]> OnAvailableConfigsUpdated;
        
        /// <summary>
        /// 配置数据发生变化时触发的事件
        /// </summary>
        public event Action OnConfigurationDirty;
        
        #endregion
        
        #region 私有字段
        
        /// <summary>
        /// 当前正在使用的配置数据
        /// </summary>
        private TestToolsWindowData _activeData;
        
        /// <summary>
        /// 项目中所有可用的配置文件路径列表
        /// </summary>
        private List<string> _availableDataPaths = new List<string>();
        
        /// <summary>
        /// 用于UI显示的配置文件名称数组
        /// </summary>
        private string[] _availableDataNames;
        
        /// <summary>
        /// 当前选中的配置文件索引
        /// </summary>
        private int _selectedDataIndex = -1;
        
        /// <summary>
        /// 标记当前配置是否有未保存的修改
        /// </summary>
        private bool _isDirty = false;
        
        /// <summary>
        /// EditorPrefs中存储上次使用配置的键名
        /// </summary>
        private const string PREFS_KEY_LAST_CONFIG = "TestToolsWindowDataName";
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 获取当前活动的配置数据
        /// </summary>
        public TestToolsWindowData ActiveData => _activeData;
        
        /// <summary>
        /// 获取所有可用配置文件的路径列表
        /// </summary>
        public List<string> AvailableDataPaths => _availableDataPaths;
        
        /// <summary>
        /// 获取用于UI显示的配置文件名称数组
        /// </summary>
        public string[] AvailableDataNames => _availableDataNames;
        
        /// <summary>
        /// 获取或设置当前选中的配置文件索引
        /// </summary>
        public int SelectedDataIndex 
        { 
            get => _selectedDataIndex;
            set
            {
                if (_selectedDataIndex != value)
                {
                    _selectedDataIndex = value;
                    LoadConfigurationByIndex(value);
                }
            }
        }
        
        /// <summary>
        /// 获取当前配置是否有未保存的修改
        /// </summary>
        public bool IsDirty => _isDirty;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        public void Initialize()
        {
            RefreshAvailableConfigurations();
            LoadLastUsedConfiguration();
        }
        
        /// <summary>
        /// 刷新项目中所有可用的配置文件
        /// </summary>
        public void RefreshAvailableConfigurations()
        {
            _availableDataPaths.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(TestToolsWindowData)}");
            
            foreach (string guid in guids)
            {
                _availableDataPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            
            _availableDataNames = _availableDataPaths.Select(Path.GetFileNameWithoutExtension).ToArray();
            
            // 触发配置列表更新事件
            OnAvailableConfigsUpdated?.Invoke(_availableDataPaths, _availableDataNames);
        }
        
        /// <summary>
        /// 根据索引加载指定的配置文件
        /// </summary>
        /// <param name="index">配置文件在列表中的索引</param>
        /// <returns>是否成功加载</returns>
        public bool LoadConfigurationByIndex(int index)
        {
            if (index >= 0 && index < _availableDataPaths.Count)
            {
                string path = _availableDataPaths[index];
                return LoadConfigurationByPath(path);
            }
            else
            {
                ClearActiveConfiguration();
                return false;
            }
        }
        
        /// <summary>
        /// 根据路径加载指定的配置文件
        /// </summary>
        /// <param name="path">配置文件的资源路径</param>
        /// <returns>是否成功加载</returns>
        public bool LoadConfigurationByPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning("[测试工具] 配置文件路径为空，无法加载配置");
                    return false;
                }

                // 刷新资产数据库以确保加载最新的配置文件内容
                AssetDatabase.Refresh();
                
                // 直接通过 AssetDatabase 尝试加载配置资产，避免使用物理路径检查导致误判
                TestToolsWindowData data = AssetDatabase.LoadAssetAtPath<TestToolsWindowData>(path);
                if (data == null)
                {
                    Debug.LogError($"[测试工具] 无法加载配置文件: {path}");
                    return false;
                }
                
                _activeData = data;
                _selectedDataIndex = _availableDataPaths.IndexOf(path);
                _isDirty = false;

                // 验证所有工具项的引用
                ValidateItemReferences(_activeData);

                // 保存到EditorPrefs
                EditorPrefs.SetString(PREFS_KEY_LAST_CONFIG, path);
                
                Debug.Log($"[测试工具] 已加载配置文件: {Path.GetFileName(path)}");
                
                // 触发配置加载事件
                OnConfigurationLoaded?.Invoke(_activeData);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[测试工具] 加载配置文件时发生异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 保存当前的配置数据
        /// </summary>
        /// <returns>是否成功保存</returns>
        public bool SaveConfiguration()
        {
            if (_activeData == null)
            {
                Debug.LogWarning("[测试工具] 没有活动的配置数据可以保存");
                return false;
            }
            
            try
            {
                EditorUtility.SetDirty(_activeData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                _isDirty = false;
                
                Debug.Log($"[测试工具] 配置已保存: {_activeData.name}");
                
                // 触发配置保存事件
                OnConfigurationSaved?.Invoke(_activeData);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[测试工具] 保存配置时发生异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 标记配置数据为已修改状态
        /// </summary>
        public void MarkDirty()
        {
            if (_activeData == null) return;
            
            _isDirty = true;
            EditorUtility.SetDirty(_activeData);
            
            // 触发配置变脏事件
            OnConfigurationDirty?.Invoke();
        }
        
        /// <summary>
        /// 验证配置数据的完整性
        /// </summary>
        /// <param name="data">要验证的配置数据</param>
        /// <returns>验证结果和错误信息</returns>
        public (bool isValid, string errorMessage) ValidateConfiguration(TestToolsWindowData data)
        {
            if (data == null)
                return (false, "配置数据为空");
            
            if (data.tabs == null)
                return (false, "标签页列表为空");
            
            // 检查标签页名称是否重复
            var duplicateNames = data.tabs
                .GroupBy(t => t.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            if (duplicateNames.Any())
                return (false, $"存在重复的标签页名称: {string.Join(", ", duplicateNames)}");
            
            // 检查工具项的完整性
            for (int i = 0; i < data.tabs.Count; i++)
            {
                var tab = data.tabs[i];
                if (string.IsNullOrEmpty(tab.name))
                    return (false, $"第{i + 1}个标签页的名称为空");
                
                if (tab.toolItems == null)
                    return (false, $"标签页'{tab.name}'的工具项列表为空");
            }
            
            return (true, string.Empty);
        }
        
        /// <summary>
        /// 创建新的配置文件
        /// </summary>
        /// <param name="fileName">文件名（不包含扩展名）</param>
        /// <param name="saveFolder">
        ///  保存目录（相对于项目根目录，例如 "Assets/ArtTools"）。
        ///  如果为空，将按以下优先级自动推导：
        ///  1. 当前活动配置所在目录
        ///  2. 已有任意配置文件所在目录
        ///  3. "Assets"
        /// </param>
        /// <returns>创建的配置数据对象</returns>
        public TestToolsWindowData CreateNewConfiguration(string fileName, string saveFolder = null)
        {
            try
            {
                // 1. 推导保存目录（Assets 相对路径）
                string folderPath = saveFolder;

                if (string.IsNullOrEmpty(folderPath))
                {
                    // 优先使用当前活动配置所在目录
                    if (_activeData != null)
                    {
                        string activePath = AssetDatabase.GetAssetPath(_activeData);
                        if (!string.IsNullOrEmpty(activePath))
                        {
                            folderPath = Path.GetDirectoryName(activePath);
                        }
                    }

                    // 其次使用任意已存在配置的目录
                    if (string.IsNullOrEmpty(folderPath) && _availableDataPaths.Count > 0)
                    {
                        folderPath = Path.GetDirectoryName(_availableDataPaths[0]);
                    }

                    // 最后兜底到 Assets 根目录
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        folderPath = "Assets";
                    }
                }

                // 规范化为 Unity 资源路径格式
                folderPath = folderPath.Replace("\\", "/");

                // 2. 确保物理目录存在（需要将 Assets 相对路径转换为磁盘路径）
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string fullFolderPath = Path.Combine(projectRoot, folderPath);
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // 3. 创建新的配置数据
                TestToolsWindowData newData = ScriptableObject.CreateInstance<TestToolsWindowData>();
                newData.tabs = new List<ToolTab>
                {
                    new ToolTab { name = "默认标签页" }
                };

                // 4. 生成资源路径并保存到文件
                string assetPath = Path.Combine(folderPath, fileName + ".asset").Replace("\\", "/");
                AssetDatabase.CreateAsset(newData, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 5. 刷新配置列表并尝试加载新建的配置
                RefreshAvailableConfigurations();
                LoadConfigurationByPath(assetPath);

                Debug.Log($"[测试工具] 已创建新配置文件: {assetPath}");

                return newData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[测试工具] 创建配置文件时发生异常: {ex.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 验证配置文件中所有工具项的引用完整性。
        /// 如果发现无效引用，会在控制台打印警告信息。
        /// </summary>
        /// <param name="data">要验证的配置数据</param>
        private void ValidateItemReferences(TestToolsWindowData data)
        {
            if (data == null || data.tabs == null) return;

            Debug.Log("[测试工具] 开始验证所有工具项的引用...");

            int invalidReferencesCount = 0;
            foreach (var tab in data.tabs)
            {
                if (tab.toolItems == null) continue;

                foreach (var item in tab.toolItems)
                {
                    if (item is IReferenceValidator validator)
                    {
                        if (!validator.ValidateReferences())
                        {
                            invalidReferencesCount++;
                            Debug.LogWarning($"[测试工具] 引用校验失败: {validator.GetValidationMessage()} (标签页: '{tab.name}')");
                        }
                    }
                }
            }

            if (invalidReferencesCount == 0)
            {
                Debug.Log("[测试工具] 所有引用均有效。");
            }
            else
            {
                Debug.LogWarning($"[测试工具] 引用验证完成，共发现 {invalidReferencesCount} 个无效引用。");
            }
        }

        /// <summary>
        /// 加载上次使用的配置文件
        /// </summary>
        private void LoadLastUsedConfiguration()
        {
            string lastConfigPath = EditorPrefs.GetString(PREFS_KEY_LAST_CONFIG, null);
            if (!string.IsNullOrEmpty(lastConfigPath))
            {
                // 统一通过 LoadConfigurationByPath 进行加载和合法性验证
                if (!LoadConfigurationByPath(lastConfigPath))
                {
                    Debug.LogWarning($"[测试工具] 上次使用的配置文件已无法加载: {lastConfigPath}，已清空记录。");
                    EditorPrefs.DeleteKey(PREFS_KEY_LAST_CONFIG);
                }
            }
        }
        
        /// <summary>
        /// 清空当前活动的配置
        /// </summary>
        private void ClearActiveConfiguration()
        {
            _activeData = null;
            _selectedDataIndex = -1;
            _isDirty = false;
        }
        
        #endregion
    }
}