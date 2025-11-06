/**
 * 文件名: OpenWebAsset.cs
 * 作用: 定义打开网页的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：打开指定的网页URL。
    /// 支持在默认浏览器中打开网页链接，可配置显示名称和URL地址。
    /// </summary>
    [Serializable]
    public class OpenWebAsset : TestToolItem
    {
        /// <summary>
        /// 在工具项上方显示的标签文本，可为空
        /// </summary>
        [Tooltip("在工具项上方显示的标签，可为空")]
        public string labelName;
        
        /// <summary>
        /// 按钮上显示的名称
        /// </summary>
        [Tooltip("按钮上显示的名称")]
        public string buttonLabel = "打开网页";
        
        /// <summary>
        /// 要打开的网页URL地址
        /// </summary>
        [Tooltip("要打开的网页URL地址，例如：https://www.example.com")]
        public string webUrl = "https://";
        
        /// <summary>
        /// 是否在Unity编辑器内置浏览器中打开（如果支持）
        /// </summary>
        [Tooltip("是否尝试在Unity编辑器内置浏览器中打开，否则使用系统默认浏览器")]
        public bool useInternalBrowser = false;

        /// <summary>
        /// 创建打开网页工具项的深拷贝
        /// </summary>
        /// <returns>新的 OpenWebAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new OpenWebAsset
            {
                labelName = this.labelName,
                buttonLabel = this.buttonLabel,
                webUrl = this.webUrl,
                useInternalBrowser = this.useInternalBrowser
            };
        }

        /// <summary>
        /// 绘制打开网页工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            // 显示标签（如果有）
            if (!string.IsNullOrEmpty(labelName))
                GUILayout.Label(labelName);

            // 显示打开网页按钮
            if (!string.IsNullOrEmpty(webUrl))
            {
                string buttonText = string.IsNullOrEmpty(buttonLabel) ? "打开网页" : buttonLabel;

                // 创建一个包含图标和文本的GUIContent
                var content = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small");
                content.text = " " + buttonText;
                content.tooltip = $"打开网页: {webUrl}";

                if (GUILayout.Button(content, GUILayout.Height(22)))
                {
                    OpenWebPage();
                }
            }
            else
            {
                // 如果没有设置URL，显示警告
                EditorGUILayout.HelpBox("请设置要打开的网页URL地址", MessageType.Warning);
            }
        }
        
        /// <summary>
        /// 打开网页的核心方法
        /// </summary>
        private void OpenWebPage()
        {
            if (string.IsNullOrEmpty(webUrl))
            {
                Debug.LogWarning("[测试工具] 网页URL为空，无法打开");
                return;
            }
            
            // 验证URL格式
            if (!IsValidUrl(webUrl))
            {
                Debug.LogWarning($"[测试工具] 无效的URL格式: {webUrl}");
                return;
            }
            
            try
            {
                if (useInternalBrowser)
                {
                    // 尝试使用Unity内置浏览器（如果可用）
                    #if UNITY_2020_1_OR_NEWER
                    // Unity 2020.1+ 支持内置浏览器
                    UnityEditor.Help.BrowseURL(webUrl);
                    #else
                    // 旧版本Unity使用系统默认浏览器
                    Application.OpenURL(webUrl);
                    #endif
                }
                else
                {
                    // 使用系统默认浏览器
                    Application.OpenURL(webUrl);
                }
                
                Debug.Log($"[测试工具] 已打开网页: {webUrl}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[测试工具] 打开网页失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 验证URL格式是否有效
        /// </summary>
        /// <param name="url">要验证的URL字符串</param>
        /// <returns>如果URL格式有效返回true，否则返回false</returns>
        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
                
            // 检查是否以http://或https://开头
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            // 尝试创建Uri对象来验证格式
            try
            {
                Uri uri = new Uri(url);
                return uri.IsWellFormedOriginalString();
            }
            catch
            {
                return false;
            }
        }
    }
}