/**
 * 文件名: SeparatorAsset.cs
 * 作用: 定义分隔符工具项，用于UI组织和分组
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：一个用于UI组织的分隔符。
    /// 可以显示为简单的线条或带标题的框，用于在工具列表中进行视觉分组。
    /// </summary>
    [Serializable]
    public class SeparatorAsset : TestToolItem
    {
        /// <summary>
        /// 分隔符上显示的标题文本
        /// </summary>
        [Tooltip("分隔符上显示的标题")]
        public string title = "分组标题";
        
        /// <summary>
        /// 分隔符的显示样式（线条或带标题的框）
        /// </summary>
        [Tooltip("显示为一条线还是一个带标题的框")]
        public SeparatorDisplayStyle displayStyle = SeparatorDisplayStyle.TitledBox;

        /// <summary>
        /// 创建分隔符工具项的深拷贝
        /// </summary>
        /// <returns>新的 SeparatorAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new SeparatorAsset
            {
                title = this.title,
                displayStyle = this.displayStyle
            };
        }

        /// <summary>
        /// 绘制分隔符的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            switch (displayStyle)
            {
                case SeparatorDisplayStyle.Line:
                    // 绘制一条水平线
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    break;
                case SeparatorDisplayStyle.TitledBox:
                    // 绘制一个加粗的标题
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    break;
            }
        }
    }
}