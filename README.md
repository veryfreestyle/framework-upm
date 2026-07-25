# VeryFS Framework

VeryFS Framework v2.0.0，打包为 UPM 格式。

## 安装

在 Unity 项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.veryfreestyle.framework": "https://github.com/veryfreestyle/framework-upm.git"
  }
}
```

指定 commit 或 tag：

```json
"com.veryfreestyle.framework": "https://github.com/veryfreestyle/framework-upm.git#v2.0.0"
```

## 要求

- Unity 2021.3+
- UniTask
- FairyGUI
- LitJson

## 依赖

本包会通过 `package.json` 声明以下 UPM 依赖：

- `com.cysharp.unitask`
- `com.veryfreestyle.unity.fairygui`
- `com.veryfreestyle.unity.litjson`
