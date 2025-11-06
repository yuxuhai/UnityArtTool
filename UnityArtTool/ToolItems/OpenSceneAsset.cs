/**
 * 文件名: OpenSceneAsset.cs
 * 作用: 定义在Unity编辑器中打开场景文件的工具项
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 工具项：在Unity编辑器中打开一个场景文件。
    /// 提供快速切换场景的功能，支持保存当前场景的修改。
    /// </summary>
    [Serializable]
    public class OpenSceneAsset : TestToolItem, IReferenceValidator
    {
        /// <summary>
        /// 在工具项上方显示的标签文本，可为空
        /// </summary>
        [Tooltip("在工具项上方显示的标签，可为空")]
        public string labelName;
        
        /// <summary>
        /// 按钮上显示的名称，如果为空则使用场景名称
        /// </summary>
        [Tooltip("按钮上显示的名称，如果为空则使用场景名称")]
        public string buttonLabel;
        
        /// <summary>
        /// 要打开的场景资产引用
        /// </summary>
        [Tooltip("要打开的场景资产")]
        public SceneAsset scene;

        /// <summary>
        /// 验证此对象持有的所有关键资产引用是否仍然有效。
        /// </summary>
        /// <returns>如果所有引用都有效，则返回true；否则返回false。</returns>
        public bool ValidateReferences()
        {
            // 检查场景引用是否为空
            return scene != null;
        }

        /// <summary>
        /// 当检测到无效引用时，获取一条描述性的错误或警告消息。
        /// </summary>
        /// <returns>描述引用问题的消息字符串。</returns>
        public string GetValidationMessage()
        {
            if (scene == null)
            {
                return "场景引用丢失: 请重新指定场景文件";
            }
            return string.Empty;
        }

        /// <summary>
        /// 创建场景打开工具项的深拷贝
        /// </summary>
        /// <returns>新的 OpenSceneAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new OpenSceneAsset
            {
                labelName = this.labelName,
                buttonLabel = this.buttonLabel,
                scene = this.scene
            };
        }

        /// <summary>
        /// 绘制场景打开工具项的UI界面
        /// </summary>
        public override void DrawMainUI()
        {
            // 验证引用
            if (!ValidateReferences())
            {
                EditorGUILayout.HelpBox(GetValidationMessage(), MessageType.Warning);
                return;
            }

            // 显示标签（如果有）
            if (!string.IsNullOrEmpty(labelName))
                GUILayout.Label(labelName);

            // 显示打开场景按钮和资源管理器打开按钮
            if (scene != null)
            {
                EditorGUILayout.BeginHorizontal();
                string buttonText = string.IsNullOrEmpty(buttonLabel) ? scene.name : buttonLabel;

                // 创建一个包含图标和文本的GUIContent
                var content = EditorGUIUtility.IconContent("SceneAsset Icon");
                content.text = " " + buttonText;
                content.tooltip = $"打开场景: {scene.name}";

                if (GUILayout.Button(content, GUILayout.Height(22)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene));
                    }
                }
                
                // 添加在资源管理器中打开的按钮
                EditorGuiHelper.DrawOpenInExplorerButton(AssetDatabase.GetAssetPath(scene));

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}