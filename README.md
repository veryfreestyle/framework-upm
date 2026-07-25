# VeryFS Framework

VeryFS Framework 是 VeryFreeStyle Unity 项目的基础框架包，提供启动流程、资源包加载、FairyGUI UI 管理和常用运行时工具。当前仓库是 `com.veryfreestyle.framework` 的 UPM 分发版本。

## 安装

在 Unity 项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.veryfreestyle.framework": "https://github.com/veryfreestyle/framework-upm.git"
  }
}
```

指定版本：

```json
{
  "dependencies": {
    "com.veryfreestyle.framework": "https://github.com/veryfreestyle/framework-upm.git#v2.0.0"
  }
}
```

## 要求

- Unity 2021.3+
- UniTask
- FairyGUI
- LitJson

## 依赖

本包通过 `package.json` 声明以下 UPM 依赖：

- `com.cysharp.unitask`
- `com.veryfreestyle.unity.fairygui`
- `com.veryfreestyle.unity.litjson`

## 模块

### Runtime

- `LauncherBase`：应用启动基类，处理常驻对象、暂停/恢复、退出清理、目标帧率和 URP 下的 FairyGUI StageCamera 初始化。
- `ResourceModule`：资源包入口，负责加载 `PackageManifest`、注册 `ResourcePackage`、读取 raw resource。
- `UIModule`：FairyGUI 相关封装，包含语言设置、翻译数据加载、字体注册、UI 包预加载和卸载。
- `VeryFS.Framework.Runtime.Resource`：资源路径、资源包、AssetBundle 加载器、资源 provider、资源句柄和调试辅助。
- `VeryFS.Framework.Runtime.UI`：`ViewRouter`、`ViewHolder`、`GPopupWindow`、`GMsgBoxWindow` 等 UI 流程封装。
- `VeryFS.Framework.Runtime.Utilities`：二进制读写、对象池、单例、扩展方法、颜色、时间源、XML 和字符串工具。

### Editor

- `Framework/资源打包`：打开资源打包窗口。
- `Framework/Utility/Clear PlayerPrefs`：清理 `PlayerPrefs`。
- `Framework/Utility/打开PersistentData目录`：打开 `Application.persistentDataPath`。
- `Framework/Utility/Clear PersistentData`：清理 `Application.persistentDataPath` 下的文件和目录。

## Assembly Definition

- `VeryFS.Framework.Runtime`
- `VeryFS.Framework.Editor`
- `VeryFS.Framework.Editor.Tests`

## 版本

当前包版本：`2.0.0`
