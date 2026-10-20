# Changelog

本文件记录 RebornCore 的版本变更，格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [1.0.0] - 2026-10-20

首个公开发布版本。此前的内部迭代历史未保留。

### 变更

- 编辑器工具菜单统一整合到 `RebornFrame` 顶级菜单下，原先分散的三个入口不再使用

### 新增

- 应用启动流程：`AppInstance` 入口组件，`IDataInit` 业务初始化抽象，`IBeforeInit` 可插拔的前置步骤（按优先级分 Awake 前后两批执行）
- 逻辑帧驱动 `RebornCore`：独立线程、默认 45 帧、支持暂停恢复、回滚重算与追帧
- AssetBundle 管理 `ABPackageManager`：分包依赖解析、`ABPath://` 路径、同步与异步加载、单包与全量卸载
- 资源读取统一入口 `DataConfigManager`：按宏在 Resources、AssetDatabase 与 AssetBundle 之间切换
- 热更新基础设施：`UpdateLauncher` 启动器基类、`FileMD5` 版本清单、`MD5Helper`
- 定点数学库：`RebornFloat`、`RebornV3`、`RebornQuaternion`、`Matric3x3`、`RebornMath`、可复现随机 `RandomUtils`
- 编辑器工具：`ABToolEditor` 打包导出、配置类代码生成、框架目录初始化、框架宏与环境宏开关面板

[1.0.0]: https://github.com/LinZhiWeiNB/RebornFrameCore/releases/tag/v1.0.0
