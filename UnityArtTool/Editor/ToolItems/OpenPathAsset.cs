/**
 * 文件名: OpenPathAsset.cs
 * 作用: 定义在操作系统文件浏览器中打开指定路径的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：在操作系统的文件浏览器中打开一个指定路径。
    /// 支持打开文件夹或选中特定文件，便于快速访问项目相关的外部资源。
    /// </summary>
    [Serializable]
    public class OpenPathAsset : ArtToolItem
    {
        /// <summary>
        /// 要打开的文件夹或文件的完整路径
        /// </summary>
        [Tooltip("要打开的文件夹完整路径")]
        public string fullPath = "";

        /// <summary>
        /// 创建路径打开工具项的深拷贝
        /// </summary>
        /// <returns>新的 OpenPathAsset 实例</returns>
        public override ArtToolItem Clone()
        {
            return new OpenPathAsset { fullPath = this.fullPath };
        }
        
        /// <summary>
        /// 绘制路径打开工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            string buttonText = string.IsNullOrEmpty(fullPath) ? "选择一个路径..." : fullPath;
            
            // 创建一个包含图标和文本的GUIContent
            var content = EditorGUIUtility.IconContent("d_Folder Icon");
            content.text = " " + buttonText;
            content.tooltip = $"打开路径: {fullPath}";
            
            if (GUILayout.Button(content, GUILayout.Height(22)))
            {
                if (!string.IsNullOrEmpty(fullPath) && (System.IO.Directory.Exists(fullPath) || System.IO.File.Exists(fullPath)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", EditorGuiHelper.NormalizePathForExplorer(fullPath));
                }
                else
                {
                     Debug.LogError($"[ArtTools] 路径无效或不存在: {fullPath}");
                }
            }
        }
    }
}