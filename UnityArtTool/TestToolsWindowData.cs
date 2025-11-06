using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtTools
{
    #region 辅助类与枚举

    /// <summary>
    /// 编辑器GUI辅助类，提供一些通用的UI绘制和路径处理方法。
    /// </summary>
    public static class EditorGuiHelper
    {
        /// <summary>
        /// 绘制一个“在资源管理器中打开”的按钮。
        /// </summary>
        /// <param name="path">要打开的文件或文件夹在项目中的相对路径。</param>
        public static void DrawOpenInExplorerButton(string path)
        {
            if (GUILayout.Button("在资源管理器打开", GUILayout.MaxWidth(130)))
            {
                // 获取完整物理路径
                string fullPath = System.IO.Path.GetFullPath(path);
                // 使用 /select, 参数可以在资源管理器中选中该文件
                System.Diagnostics.Process.Start("explorer.exe", "/select," + NormalizePathForExplorer(fullPath));
            }
        }

        /// <summary>
        /// 将Unity风格的路径（/）转换为Windows资源管理器可识别的路径（\）。
        /// </summary>
        public static string NormalizePathForExplorer(string path)
        {
            return path.Replace('/', '\\');
        }
    }

    /// <summary>
    /// 工具项的类型枚举。
    /// </summary>
    public enum ToolType
    {
        [Description("打开工具")] Tool,
        [Description("定位项目资产")] FindObject,
        [Description("定位场景对象")] FindGameObject,
        [Description("文本备注")] Text,
        [Description("打开路径")] OpenPath,
        [Description("打开场景")] Scene,
        [Description("打开网页")] OpenWeb, // 新增：打开网页类型
        [Description("分组/分隔符")] Separator // 分隔符类型
    }

    /// <summary>
    /// 新增：分隔符的显示样式枚举
    /// </summary>
    public enum SeparatorDisplayStyle
    {
        [Description("线条")]
        Line,
        [Description("带标题的框")]
        TitledBox
    }

    #endregion

    #region 核心数据结构 (原代码缺失的定义)

    /// <summary>
    /// 存储编辑器工具的路径和显示名称。
    /// </summary>
    [Serializable]
    public class EditorToolPath
    {
        public string label;
        public string path;
    }

    /// <summary>
    /// 存储项目内Object资产的引用和显示名称。
    /// </summary>
    [Serializable]
    public class ProjectObjectReference
    {
        public string label;
        public Object targetObject;
    }

    #endregion

    /// <summary>
    /// ScriptableObject配置文件，用于存储整个测试工具窗口的数据。
    /// </summary>
    [CreateAssetMenu(fileName = "Test Tools Window Data", menuName = "ArtTools/Test Tool Data")]
    public class TestToolsWindowData : ScriptableObject
    {
        [Tooltip("工具窗口的标签页列表")]
        public List<ToolTab> tabs = new List<ToolTab>();

        [Tooltip("执行截图启动时，记录的日志数量上限")]
        public int logLimit = 30;
        
        [Tooltip("截图保存的目录")]
        public string screenshotDirectory = "Screenshots/Capture/";
        
        [Header("快捷启动设置")]
        [Tooltip("联机启动时，需要打开的场景。如果为空，则无法使用联机启动功能。")]
        public SceneAsset onlineStartScene; // 新增：可配置的联机启动场景
    }

    /// <summary>
    /// 单个标签页的数据模型，包含名称和该页下的所有工具项。
    /// 重构后的版本，拖拽逻辑由外部DragDropManager处理。
    /// </summary>
    [Serializable]
    public class ToolTab
    {
        /// <summary>
        /// 标签页上显示的名称
        /// </summary>
        [Tooltip("标签页上显示的名称")]
        public string name;

        /// <summary>
        /// 此标签页下的所有具体工具项列表
        /// </summary>
        [Tooltip("此标签页下的所有具体工具项")]
        [SerializeReference] // 使用SerializeReference支持多态序列化，非常关键！
        public List<TestToolItem> toolItems = new List<TestToolItem>();

        /// <summary>
        /// 绘制该标签页下的所有工具项UI
        /// </summary>
        /// <param name="isEditMode">窗口当前是否处于编辑模式</param>
        /// <param name="dragDropManager">拖拽管理器实例，用于处理拖拽逻辑</param>
        /// <param name="tabIndex">当前标签页的索引，用于拖拽事件</param>
        /// <returns>如果UI内部发生数据修改（如删除），则返回true</returns>
        public bool DrawMainUI(bool isEditMode, DragDropManager dragDropManager = null, int tabIndex = -1)
        {
            bool hasChanged = false;
            int itemIndexToRemove = -1;

            for (int i = 0; i < toolItems.Count; i++)
            {
                var item = toolItems[i];
                
                // 绘制拖拽指示器（如果正在拖拽）
                if (dragDropManager != null && dragDropManager.IsDraggingItem)
                {
                    dragDropManager.DrawItemDropIndicator(i, new Rect());
                }
                
                // 获取当前工具项的矩形区域，用于拖拽检测
                Rect itemRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // 在编辑模式下，添加一个拖拽手柄
                if (isEditMode)
                {
                    GUIStyle dragHandleStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedWidth = 20
                    };
                    GUILayout.Label("::", dragHandleStyle);
                }

                // 绘制工具项主要内容
                GUILayout.BeginVertical();
                item.DrawMainUI(); // 让每个工具项自己绘制主UI
                GUILayout.EndVertical();

                // 在编辑模式下显示删除按钮
                if (isEditMode)
                {
                    if (GUILayout.Button("-", GUILayout.Width(20))) 
                    { 
                        itemIndexToRemove = i; 
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // 处理拖拽逻辑（如果提供了拖拽管理器）
                if (isEditMode && dragDropManager != null && tabIndex >= 0)
                {
                    dragDropManager.HandleItemDragAndDrop(tabIndex, i, itemRect);
                }
            }

            // 处理拖拽完成事件
            if (dragDropManager != null && tabIndex >= 0)
            {
                dragDropManager.CompleteItemDragAndDrop(toolItems.Count);
            }

            // 处理工具项删除
            if (itemIndexToRemove != -1)
            {
                toolItems.RemoveAt(itemIndexToRemove);
                hasChanged = true;
            }

            return hasChanged;
        }
        
        /// <summary>
        /// 添加新的工具项到当前标签页
        /// </summary>
        /// <param name="toolItem">要添加的工具项</param>
        public void AddToolItem(TestToolItem toolItem)
        {
            if (toolItem != null)
            {
                toolItems.Add(toolItem);
            }
        }
        
        /// <summary>
        /// 移除指定索引的工具项
        /// </summary>
        /// <param name="index">要移除的工具项索引</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveToolItem(int index)
        {
            if (index >= 0 && index < toolItems.Count)
            {
                toolItems.RemoveAt(index);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 重排序工具项
        /// </summary>
        /// <param name="fromIndex">原始索引</param>
        /// <param name="toIndex">目标索引</param>
        /// <returns>是否成功重排序</returns>
        public bool ReorderToolItem(int fromIndex, int toIndex)
        {
            if (fromIndex >= 0 && fromIndex < toolItems.Count && 
                toIndex >= 0 && toIndex <= toolItems.Count && 
                fromIndex != toIndex)
            {
                TestToolItem item = toolItems[fromIndex];
                toolItems.RemoveAt(fromIndex);
                
                // 调整插入位置
                int insertIndex = fromIndex < toIndex ? toIndex - 1 : toIndex;
                insertIndex = UnityEngine.Mathf.Clamp(insertIndex, 0, toolItems.Count);
                
                toolItems.Insert(insertIndex, item);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取工具项的数量
        /// </summary>
        public int ToolItemCount => toolItems?.Count ?? 0;
        
        /// <summary>
        /// 检查标签页是否为空
        /// </summary>
        public bool IsEmpty => ToolItemCount == 0;
        
        /// <summary>
        /// 清空所有工具项
        /// </summary>
        public void ClearAllToolItems()
        {
            toolItems?.Clear();
        }
    }

    // 注意：具体的工具项类已移动到 ToolItems 子目录中的独立文件
    // TestToolItem 基类现在位于 ToolItems/TestToolItem.cs



}

