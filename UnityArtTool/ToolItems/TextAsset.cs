/**
 * 文件名: TextAsset.cs
 * 作用: 定义显示多行文本备注的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：显示一段多行文本，用于备注或说明。
    /// 支持锁定模式以防止误编辑，适合在工具界面中添加说明文档。
    /// </summary>
    [Serializable]
    public class TextAsset : TestToolItem
    {
        /// <summary>
        /// 要显示的文本内容
        /// </summary>
        [Tooltip("要显示的文本内容")]
        [TextArea(3, 10)]
        public string textContent = "";
        
        /// <summary>
        /// 是否锁定文本框，防止误编辑
        /// </summary>
        [Tooltip("是否锁定文本框，防止误编辑")]
        public bool isLocked = true;

        /// <summary>
        /// 创建文本工具项的深拷贝
        /// </summary>
        /// <returns>新的 TextAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new TextAsset
            {
                textContent = this.textContent,
                isLocked = this.isLocked
            };
        }

        /// <summary>
        /// 绘制文本工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            // 为文本备注添加图标
            var icon = EditorGUIUtility.IconContent("d_TextAsset Icon", "文本备注");
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            
            // 根据锁定状态决定文本框是否可编辑
            EditorGUI.BeginDisabledGroup(isLocked);
            textContent = EditorGUILayout.TextArea(textContent);
            EditorGUI.EndDisabledGroup();

            // 提供一个锁定/解锁的Toggle
            isLocked = EditorGUILayout.Toggle(isLocked, "IN LockButton", GUILayout.MaxWidth(15));
            
            EditorGUILayout.EndHorizontal();
        }
    }
}