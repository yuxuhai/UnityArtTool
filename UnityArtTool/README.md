# unity美术通用工具合集 - 代码结构说明

## 概述

unity美术通用工具合集是一个功能强大的Unity编辑器工具，允许用户创建可定制的工具面板，集成多种常用的编辑器快捷操作。

## 文件结构

### 核心文件

- **TestToolsEditorWindow.cs** - 主编辑器窗口，负责UI显示和用户交互
- **TestToolsWindowData.cs** - 数据模型和配置管理，包含核心数据结构

### ToolItems 目录

所有具体的工具项类都已拆分到独立文件中，便于维护和扩展：

- **TestToolItem.cs** - 抽象基类，定义所有工具项的基本接口
- **SeparatorAsset.cs** - 分隔符工具项，用于UI分组
- **ToolAsset.cs** - 执行Unity菜单命令的工具项
- **FindObjectAsset.cs** - 在Project窗口中定位资产的工具项
- **FindGameObjectAsset.cs** - 在场景中查找GameObject的工具项
- **TextAsset.cs** - 显示文本备注的工具项
- **OpenPathAsset.cs** - 打开文件路径的工具项
- **OpenSceneAsset.cs** - 打开Unity场景的工具项

## 使用方法

1. 通过菜单 `工具/测试工具合集 (优化版)` 打开工具窗口
2. 在项目中创建配置文件：右键 -> Create -> ArtTools -> Test Tool Data
3. 在工具窗口中选择配置文件
4. 启用编辑模式来添加和配置工具项
5. 使用拖拽功能来重新排序标签页和工具项

## 扩展新工具项

要添加新的工具项类型：

1. 在 `ToolItems` 目录中创建新的类文件
2. 继承 `TestToolItem` 抽象基类
3. 实现 `DrawMainUI()` 和 `Clone()` 方法
4. 在 `TestToolsWindowData.cs` 中的 `ToolType` 枚举添加新类型
5. 在 `TestToolsEditorWindow.cs` 中添加对应的UI绘制和实例化逻辑



## 注意事项

- 所有工具项类都使用 `[Serializable]` 特性
- 配置数据使用 `[SerializeReference]` 支持多态序列化
- 确保新添加的工具项类实现完整的深拷贝逻辑
