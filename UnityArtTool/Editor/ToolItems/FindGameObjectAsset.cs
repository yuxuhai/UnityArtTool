/**
 * 文件名: FindGameObjectAsset.cs
 * 作用: 定义在场景中查找并选中GameObject的工具项
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
    /// 工具项：在场景中查找并选中一个GameObject。
    /// 支持在指定场景中查找对象，如果场景未打开会自动切换。
    /// </summary>
    [Serializable]
    public class FindGameObjectAsset : TestToolItem, IReferenceValidator
    {
        /// <summary>
        /// 在工具项上方显示的标签文本，可为空
        /// </summary>
        [Tooltip("在工具项上方显示的标签，可为空")]
        public string labelName;
        
        /// <summary>
        /// 按钮上显示的名称，如果为空则使用物件名称
        /// </summary>
        [Tooltip("按钮上显示的名称，如果为空则使用物件名称")]
        public string buttonLabel;
        
        /// <summary>
        /// 要查找的GameObject的名称
        /// </summary>
        [Tooltip("要查找的GameObject的名称")]
        public string targetObjectName;

        [Tooltip("目标对象所在的场景")]
        public SceneAsset targetScene;

        /// <summary>
        /// 验证此对象持有的所有关键资产引用是否仍然有效。
        /// </summary>
        /// <returns>如果所有引用都有效，则返回true；否则返回false。</returns>
        public bool ValidateReferences()
        {
            return targetScene != null && !string.IsNullOrEmpty(targetObjectName);
        }

        /// <summary>
        /// 当检测到无效引用时，获取一条描述性的错误或警告消息。
        /// </summary>
        /// <returns>描述引用问题的消息字符串。</returns>
        public string GetValidationMessage()
        {
            if (targetScene == null)
            {
                return "场景引用丢失或未分配。";
            }
            if (string.IsNullOrEmpty(targetObjectName))
            {
                return "目标对象的名称不能为空。";
            }
            return "引用有效。"; // 理论上不应到达这里
        }

        /// <summary>
        /// 创建在场景中查找游戏对象工具项的深拷贝
        /// </summary>
        /// <returns>新的 FindGameObjectAsset 实例</returns>
        public override TestToolItem Clone()
        {
            return new FindGameObjectAsset
            {
                labelName = this.labelName,
                buttonLabel = this.buttonLabel,
                targetObjectName = this.targetObjectName,
                targetScene = this.targetScene
            };
        }

        /// <summary>
        /// 绘制场景对象查找工具项的UI界面
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

            // 显示查找按钮
            if (!string.IsNullOrEmpty(targetObjectName))
            {
                string buttonText = string.IsNullOrEmpty(buttonLabel) ? targetObjectName : buttonLabel;

                // 创建一个包含图标和文本的GUIContent
                var content = EditorGUIUtility.IconContent("d_GameObject Icon");
                content.text = " " + buttonText;
                content.tooltip = $"定位场景物件: {targetObjectName}";

                if (GUILayout.Button(content, GUILayout.Height(22)))
                {
                    // 如果指定了场景，并且当前场景不是目标场景，则询问用户是否要切换场景
                    if (targetScene != null && EditorSceneManager.GetActiveScene().name != targetScene.name)
                    {
                        string currentSceneName = EditorSceneManager.GetActiveScene().name;
                        string targetSceneName = targetScene.name;
                        
                        // 显示确认对话框
                        bool shouldSwitchScene = EditorUtility.DisplayDialog(
                            "切换场景确认",
                            $"要定位的物件 '{targetObjectName}' 位于场景 '{targetSceneName}' 中。\n\n" +
                            $"当前场景: {currentSceneName}\n" +
                            $"目标场景: {targetSceneName}\n\n" +
                            "是否要切换到目标场景？",
                            "切换场景",
                            "取消"
                        );
                        
                        if (shouldSwitchScene)
                        {
                            string scenePath = AssetDatabase.GetAssetPath(targetScene);
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                try
                                {
                                    EditorSceneManager.OpenScene(scenePath);
                                    Debug.Log($"[测试工具] 已切换到场景: {targetSceneName}");
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError($"[测试工具] 切换场景失败: {ex.Message}");
                                    return;
                                }
                            }
                            else
                            {
                                // 用户取消了保存当前场景的修改，不执行切换
                                return;
                            }
                        }
                        else
                        {
                            // 用户选择不切换场景，直接返回
                            return;
                        }
                    }

                    // 查找GameObject
                    GameObject foundObject = GameObject.Find(targetObjectName);
                    if (foundObject != null)
                    {
                        Selection.activeGameObject = foundObject;
                        EditorGUIUtility.PingObject(foundObject);
                        Debug.Log($"[测试工具] 已定位到物件: {targetObjectName}");
                    }
                    else
                    {
                        // 如果常规Find找不到（可能是非激活状态），尝试遍历查找
                        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
                        {
                            if (go.name == targetObjectName)
                            {
                                Selection.activeGameObject = go;
                                EditorGUIUtility.PingObject(go);
                                return;
                            }
                        }
                        Debug.LogError($"[测试工具] 在场景中未找到名为 '{targetObjectName}' 的对象。");
                    }
                }
            }
        }
    }
}