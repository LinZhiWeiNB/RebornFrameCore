# RebornCore

Reborn 框架核心包（`com.reborn.reborncore`），面向 Unity 的游戏启动流程、AssetBundle 热更新与确定性定点数学库。

- 应用启动与初始化流程编排（`AppInstance` / `IDataInit` / `IBeforeInit`）
- 可回滚的逻辑帧驱动（`RebornCore`），用于帧同步类玩法
- AssetBundle 分包加载、依赖解析与热更新版本管理
- 定点数学库（`RebornFloat` / `RebornV3` / `RebornQuaternion` / `Matric3x3`），跨平台结果一致
- 编辑器工具：AB 打包导出、配置类代码生成、框架目录初始化、宏开关面板

## 环境要求

- Unity 2021.3 或更高版本
- 依赖包 `com.reborn.utils` 1.0.0（提供 `Singleton`、`ReflectionUtils` 等基础设施）

部分功能还需要项目自行引入第三方库：热更新示例模板用到 HybridCLR，UI 绑定示例用到 FairyGUI。这两者不是包的强制依赖，未使用相关功能时无需安装。

## 安装

在 Unity 的 `Packages/manifest.json` 中通过 Git URL 引入：

```json
{
  "dependencies": {
    "com.reborn.reborncore": "https://github.com/LinZhiWeiNB/RebornFrameCore.git"
  }
}
```

也可以在 Package Manager 中选择 `Add package from git URL`，填入：

```
https://github.com/LinZhiWeiNB/RebornFrameCore.git
```

注意依赖的 `com.reborn.utils` 需要单独引入，Package Manager 不会自动拉取非注册表来源的包。

## 快速开始

1. 菜单栏执行 `RebornFrame/初始化框架`，生成框架约定的目录结构，并从 `Editor/Examples` 释放 `AppInit`、`AppUpdate` 模板。
2. 在启动场景中创建一个 GameObject，挂上 `AppInstance` 组件。
3. 实现 `IDataInit` 的子类（模板已生成 `AppInit`），框架会通过反射自动找到它：

```csharp
public class AppInit : IDataInit
{
    public override void Awake()
    {
        // 配置与绑定已完成，这里做业务初始化
    }

    public override void LoadLaunchUI(string sceneName)
    {
        // 打开首个界面
    }
}
```

`AppInstance` 在 `Awake` 时反射收集所有 `IBeforeInit` 实现，按 `Priority` 排序后分两批执行：`BeforeAwake` 为 true 的在热更新与配置加载之前跑，为 false 的在之后跑。全部完成后调用 `IDataInit.InitConfigs()`、`BindAll()`、`Awake()`、`LoadLaunchUI()`。启动期间框架会临时创建一台黑屏 Loading 相机，初始化结束后自动销毁。

想在启动流程中插入自己的步骤（比如 SDK 初始化、公告拉取），实现 `IBeforeInit` 即可，无需注册：

```csharp
public class SdkInit : IBeforeInit
{
    public bool BeforeAwake => true;
    public int Priority => 100;
    public bool IsFinished { get; private set; }

    public void Start() { /* 发起初始化 */ }
    public void Update() { /* 轮询，完成后置 IsFinished = true */ }
}
```

## 模块概览

### Runtime/Core

`AppInstance` 是整个框架的入口，常驻 `DontDestroyOnLoad`，对外暴露 `UpdateActions`、`LateUpdateActions`、`FixedUpdateActions`、`AppQuitActions`、`FocusActions` 等生命周期事件，以及 `DelayCall` 和 `LoadSceneInABPackage`（从 AB 包加载场景，支持 `LoadSceneMode` 与覆盖重载）。

`RebornCore` 是独立线程驱动的逻辑帧循环，默认 45 帧。`StartLogicFrame` / `PauseLogicFrame` / `ResumeLogicFrame` / `StopLogicFrame` 控制生命周期，`OnFixedUpdate` 按逻辑帧回调，`RollBack(frame)` 回滚到指定帧并快速重算到当前帧，`QuickRunToFrame` 用于追帧。`TargetDeltaTime`、`FramesTransToSeconds`、`SecondsTransToFrames` 都是定点数，保证多端一致。

`UpdateLauncher` 是热更新启动器基类，子类重写 `GetABRootPath` 决定 AB 包的读取位置，`PullUpdate` 实现下载流程。`FileMD5` 描述一次热更的版本号、Manifest 包名与文件 MD5 清单。`MD5Helper` 提供字符串与文件的 MD5 计算。

### Runtime/Manager

`ABPackageManager` 读取 `packagedata.json` 中的分包信息，按 PackageID 或包名解析依赖链并加载 AssetBundle，支持 `ABPath://` 形式的资源路径、同步与异步加载、单包与全量卸载。

`DataConfigManager` 统一资源读取入口，根据 `USE_AB`、`SIMULATE_PHONE` 宏在 Resources、AssetDatabase 和 AssetBundle 三种来源间切换，调用方代码不用改。`DataLoadRequest<T>` 封装异步加载的进度与完成回调。

### Runtime/Math

定点数学库，用整数模拟小数以消除浮点误差，适合帧同步与回放。精度由 `RebornMath.PRECISION` 控制（默认 1000，精度越高可用数值范围越小）。`RebornFloat` 支持常规运算符与 `Float` 转换，`int`/`float` 可通过 `.RebornFloat()` 扩展方法转入。`RebornMath` 提供 `Sin`、`Cos`、`Sqrt`、`Clamp`、`Dot`、`Cross`、`Distance`、`NormLize`、`MoveToWards` 等，`NextRandom` 配合 `RandomSeed` 做可复现的随机。

## 编辑器工具

所有工具都收在 `RebornFrame` 顶级菜单下。

| 菜单 | 说明 |
| --- | --- |
| `RebornFrame/初始化框架` | 创建框架目录结构，释放启动模板 |
| `RebornFrame/ABToolEditor` | AB 打包配置与导出、版本对比、热更文件生成 |
| `RebornFrame/配置/生成配置文件` | 扫描配置类，自动生成对应的 Manager 代码 |
| `RebornFrame/框架宏/*` | 一键开关框架宏，菜单项前缀标注当前状态 |
| `RebornFrame/环境宏/*` | 切换 `DEVELOPMENT`、`RELEASE_TEST`、`RELEASE`，三者互斥 |

ABToolEditor 的配置保存在 `Assets/Editor/ABExportConfig.asset`，包含导出路径、对比路径、各环境的版本与 MD5 服务器地址、HotUpdate DLL 路径、AOT DLL 列表，以及按目录和正则划分的分包规则。

## 宏开关

| 宏 | 作用 |
| --- | --- |
| `USE_AB` | 启用 AssetBundle 资源加载，关闭时走 Resources |
| `SIMULATE_PHONE` | 在编辑器内模拟真机热更流程，启用热更启动器 |
| `LOGIC_LOG` | 输出逻辑帧生命周期日志 |
| `REBORN_FS` | 框架保留开关 |
| `DEVELOPMENT` / `RELEASE_TEST` / `RELEASE` | 环境区分，决定热更服务器地址 |
| `USE_DLL` | 以 DLL 形式集成时屏蔽编辑器调试代码 |

## 许可

MIT，详见 [LICENSE.md](LICENSE.md)。
