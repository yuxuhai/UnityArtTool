/**
 * 文件名: FindObjectAsset.cs
 * 作用: 定义在Project窗口中定位资产的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtTools
{
    /// <summary>
    /// 工具项：在Project窗口中高亮选中一个资产。
    /// 用于快速定位和选择项目中的特定资源文件。
    /// </summary>
    [Serializable]
    public class FindObjectAsset : TestToolItem, IReferenceValidator
    {
        /// <summary>
        /// 在工具项上方显示的标签文本，可为空
        /// </summary>
        [Tooltip("在工具项上方显示的标签，可为空")]
        public string labelName;
        
        /// <summary>
        /// 要定位的资产及其按钮名称配置
        /// </summary>
        [Tooltip("要定位的资产及其按钮名称")]
        public ProjectObjectReference objectRef = new ProjectObjectReference();

        public bool ValidateReferences()
        {
            return objectRef?.targetObject != null;
        }

        public string GetValidationMessage()
        {
            string assetName = (objectRef != null && !string.IsNullOrEmpty(objectRef.label)) ? objectRef.label : "未命名";
            return $"资产 '{assetName}' 引用丢失或未分配。";
        }

        /// <summary>
        /// 创建资产定位工具项的深拷贝
        /// </summary>
        /// <returns>新的 FindObjectAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new FindObjectAsset
            {
                labelName = this.labelName,
                objectRef = new ProjectObjectReference { label = this.objectRef.label, targetObject = this.objectRef.targetObject }
            };
        }

        /// <summary>
        /// 绘制资产定位工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            // 显示标签（如果有）
            if (!string.IsNullOrEmpty(labelName))
                GUILayout.Label(labelName);

            if (ValidateReferences())
            {
                EditorGUILayout.BeginHorizontal();
                string buttonText = string.IsNullOrEmpty(objectRef.label) ? objectRef.targetObject.name : objectRef.label;
                
                // 创建一个包含图标和文本的GUIContent用于按钮
                var content = EditorGUIUtility.IconContent("d_ViewToolZoom");
                content.text = " " + buttonText;
                content.tooltip = $"定位资产: {objectRef.targetObject.name}";
                
                if (GUILayout.Button(content, GUILayout.Height(22)))
                {
                    Selection.activeObject = objectRef.targetObject;
                    EditorGUIUtility.PingObject(objectRef.targetObject); 
                }
                
                // 添加在资源管理器中打开的按钮
                EditorGuiHelper.DrawOpenInExplorerButton(AssetDatabase.GetAssetPath(objectRef.targetObject));

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox(GetValidationMessage(), MessageType.Warning);
            }
        }
    }
}