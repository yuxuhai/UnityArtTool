/**
 * 文件名: ToolAsset.cs
 * 作用: 定义执行Unity编辑器菜单命令的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：执行一个Unity编辑器菜单命令。
    /// 通过指定菜单路径来调用Unity编辑器中的各种功能。
    /// </summary>
    [Serializable]
    public class ToolAsset : TestToolItem
    {
        /// <summary>
        /// 在工具项上方显示的标签文本，可为空
        /// </summary>
        [Tooltip("在工具项上方显示的标签，可为空")]
        public string labelName;
        
        /// <summary>
        /// 工具的路径和按钮名称配置
        /// </summary>
        [Tooltip("工具的路径和按钮名称")]
        public EditorToolPath toolPath = new EditorToolPath();

        /// <summary>
        /// 创建工具项的深拷贝
        /// </summary>
        /// <returns>新的 ToolAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new ToolAsset
            {
                labelName = this.labelName,
                toolPath = new EditorToolPath { label = this.toolPath.label, path = this.toolPath.path }
            };
        }

        /// <summary>
        /// 绘制工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            // 显示标签（如果有）
            if (!string.IsNullOrEmpty(labelName))
                GUILayout.Label(labelName);

            // 显示执行按钮
            if (!string.IsNullOrEmpty(toolPath.path))
            {
                string buttonText = string.IsNullOrEmpty(toolPath.label) ? toolPath.path : toolPath.label;
                
                // 创建一个包含图标和文本的GUIContent
                var content = EditorGUIUtility.IconContent("d_CustomTool");
                content.text = " " + buttonText;
                content.tooltip = $"执行菜单命令: {toolPath.path}";

                if (GUILayout.Button(content, GUILayout.Height(22)))
                {
                    if (!EditorApplication.ExecuteMenuItem(toolPath.path))
                    {
                        Debug.LogError($"[测试工具] 未找到对应的菜单项，请检查路径是否正确: {toolPath.path}");
                    }
                }
            }
        }
    }
}