# Unity 美术通用工具合集

Unity 美术通用工具合集是一个用于提升美术、关卡、运营等非程序同学工作效率的 **Unity 编辑器工具面板**。  
你可以通过配置标签页和多种工具项，将常用操作集中到一个窗口里，一键执行。

## 主要特性

- **可配置工具面板**：支持多个标签页，每个标签页下可以配置任意数量的工具项。
- **丰富的工具类型**：
  - `ToolAsset`：执行 Unity 菜单命令（如 Lightmap 烘焙、打包命令等）。
  - `FindObjectAsset`：在 Project 窗口中定位指定资源。
  - `FindGameObjectAsset`：在场景中查找指定名称的 GameObject。
  - `TextAsset`：显示备注/说明文本。
  - `OpenPathAsset`：一键打开系统文件夹。
  - `OpenSceneAsset`：一键切换场景，支持保存当前修改。
  - `OpenWebAsset`：打开常用网页/后台地址。
  - `SeparatorAsset`：分隔/分组显示工具项。
- **编辑模式 + 拖拽排序**：标签页与工具项均支持拖拽排序，方便整理布局。
- **快速搜索区域**：集成 Google / 百度 / Bing / GitHub / StackOverflow / Unity 文档 / 知乎 的快速搜索入口。
- **联机启动支持**：在底部配置联机启动场景，一键进入指定场景并自动 Play。
- **安全性与健壮性**：支持配置验证、引用校验、异常捕获，尽量避免因坏引用导致窗口不可用。

## 安装与打开

1. 将本工具所在的 `UnityArtTool` 目录放入项目的 `Assets` 下的任意 Editor 目录中（确保脚本位于 Editor 代码环境中编译）。
2. 打开 Unity 后，等待脚本编译完成。
3. 在菜单栏中通过 `工具/美术工具合集` 打开主工具窗口。

> 菜单路径来源于 `TestToolsEditorWindow` 中的  
> `[MenuItem("工具/美术工具合集", false, 2)]`

## 快速上手

1. **创建配置文件**
   - 在 Project 窗口中右键空白区域：`Create -> ArtTools -> Test Tool Data`。
   - 建议将配置资产放在团队约定的目录，例如 `Assets/ArtTools/Configs`。
2. **选择配置**
   - 打开 `工具/美术工具合集` 窗口。
   - 在顶部的“配置文件”下拉框中选择刚刚创建的配置资产。
3. **开启编辑模式**
   - 窗口底部点击“编辑模式”按钮，进入可编辑状态。
   - 左侧可以新增/重命名/删除标签页，并通过拖拽调整顺序。
4. **添加工具项**
   - 在右侧“添加新工具项”区域选择工具类型（如 Tool、FindObject、Scene、OpenWeb 等）。
   - 填写对应参数（如菜单路径、场景引用、URL、文件夹路径等），点击 `+` 添加到当前标签页。
5. **保存与使用**
   - 配置变更后底部“保存配置”按钮会带 `*` 高亮提示。
   - 点击“保存配置”写回到 `TestToolsWindowData` 资产中。
   - 关闭编辑模式后，列表即为最终使用形态，供日常一键操作。

## 配置与数据存储

- **配置资产类型**：`TestToolsWindowData`（ScriptableObject）。
- **多态存储**：内部使用 `[SerializeReference]` 存储 `TestToolItem` 派生类，以支持多种工具类型共存。
- **自动记住最后一次使用的配置**：
  - 通过 `EditorPrefs` 键 `TestToolsWindowDataName` 记录上一次加载的配置路径。
  - 下次打开窗口会自动尝试加载该配置（如果仍然存在）。
- **创建新配置时的保存路径（重要）**：
  - `ConfigurationManager.CreateNewConfiguration` 不再使用硬编码路径。
  - 若未显式指定保存目录时，会按如下优先级自动选择：
    1. 当前活动配置所在目录；
    2. 项目中任意一个已有配置文件所在目录；
    3. 回退到 `Assets` 根目录。

## 文件结构（核心脚本）

- `TestToolsEditorWindow.cs`  
  主编辑器窗口：负责窗口生命周期、管理器初始化与事件转发。

- `Managers/ConfigurationManager.cs`  
  - 发现、加载、保存 `TestToolsWindowData` 配置资产。  
  - 负责“最后一次配置”的记忆与自动加载。  
  - 提供配置验证与工具项引用校验。

- `Managers/UIManager.cs`  
  - 负责窗口全部 UI 绘制：配置选择、快速搜索区、标签页列表、右侧工具面板、底部操作栏。  
  - 通过事件向外通知“配置切换、标签页增删、工具项新增、保存请求、联机启动请求”等行为。

- `Managers/DragDropManager.cs`  
  - 统一管理标签页与工具项的拖拽行为与排序回写。

- `TestToolsWindowData.cs`  
  - 核心数据模型：包含标签页列表、全局设置（联机启动场景、日志数量限制、截图路径等）。

- `ToolItems/` 目录（所有工具项类型）  
  - `TestToolItem.cs`：抽象基类，约束所有工具项实现 `DrawMainUI()` 和 `Clone()`。  
  - `ToolAsset.cs`：执行 Unity 菜单命令。  
  - `FindObjectAsset.cs`：在 Project 中定位资源。  
  - `FindGameObjectAsset.cs`：在场景中查找目标 GameObject，并可限制所属场景。  
  - `TextAsset.cs`：显示自由文本。  
  - `OpenPathAsset.cs`：打开操作系统文件夹。  
  - `OpenSceneAsset.cs`：切换场景（实现 `IReferenceValidator`，支持引用校验）。  
  - `OpenWebAsset.cs`：打开网页链接，支持使用内置浏览器/系统浏览器。  
  - `SeparatorAsset.cs`：分隔符，用于视觉分组。

## 扩展新的工具项类型

1. 在 `ToolItems` 目录中创建新的工具类文件。
2. 让新类继承 `TestToolItem` 抽象基类。
3. 实现：
   - `public override void DrawMainUI()`：定义在面板中绘制的实际 UI 与行为。
   - `public override TestToolItem Clone()`：实现深拷贝逻辑，用于“添加新工具项”时复制模板。
4. 在 `TestToolsWindowData.cs` 中的 `ToolType` 枚举添加对应枚举值。
5. 在 `UIManager` 的“添加新工具项面板”中：
   - 增加新类型在 `switch(_toolTypeToAdd)` 中的 UI 绘制逻辑。
   - 在 `GetClonedNewItem()` 中返回对应类型的克隆实例。
6. 如工具项持有重要资源引用，推荐实现 `IReferenceValidator`：
   - `bool ValidateReferences()`：检查引用是否仍然有效。
   - `string GetValidationMessage()`：返回友好的错误/警告信息。

## 注意事项

- 所有工具项类应标记为 `[Serializable]`，以确保在 `TestToolsWindowData` 中正确序列化。
- 编辑器窗口的 UI 绘制已在内部做异常捕获，如遇界面不刷新的问题可先看 Console 是否有报错。
- 若新增大量自定义菜单项，第一次打开窗口时扫描菜单可能稍有延迟，可通过 UI 中的“刷新菜单项缓存”按钮手动更新。  
- 建议团队约定统一的配置资产存放目录，避免多项目/多分支时路径混乱。  
