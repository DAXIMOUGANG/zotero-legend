# Zotero 高亮图例条

读文献时用 Zotero 的 8 色高亮做标注，配色表却总记不住——"红的是局限还是方法？紫的是可复用还是待办？"

这个小工具在屏幕顶部常驻一条**无边框、几乎全透明**的彩色圆点条：平时只有颜色，鼠标悬停显示 4 字短义，点一下弹出完整用法，其余时间完全不挡视线。

![图例条](docs/images/legend-bar.png)

![悬停显示短义](docs/images/legend-hover.png)

## 特性

- **只在 Zotero 前台时出现**，切到浏览器就自动消失，不干扰其他软件。
- **跟着 Zotero 走**：Zotero 在哪块屏，条就在哪块屏的顶部居中；换屏 200 ms 内跟随，每块屏各自记住你拖到的位置。
- **双屏不同缩放比也对**：主屏 200% + 副屏 100%，两边的圆看着一样大、文字不发虚。
- **点击穿透**：圆点、气泡、卡片之外的地方，点击直接落到下面的 Zotero 窗口，不挡菜单栏。
- **8 个圆点**：7 个启用色实心；品红「表述素材」是虚线幽灵圈，提醒你这只色只走下划线通道。
- **登录自启**，托盘图标可随时隐藏/退出。

## 快速开始

1. 把 [`dist/ZoteroLegend.exe`](dist/ZoteroLegend.exe) 和 [`dist/colors.json`](dist/colors.json) 下载到同一个文件夹（少了 `colors.json` 也能跑，会用内置配色）。
2. 双击 `ZoteroLegend.exe`，托盘出现图标即成功。
3. 需要开机自启时，在该目录用 PowerShell 执行：

   ```powershell
   powershell -ExecutionPolicy Bypass -File install.ps1
   ```

   撤销：`install.ps1 -Uninstall`

> 首次运行 Windows SmartScreen 可能提示"未知发布者"——这是程序没有代码签名的正常现象，点「更多信息 → 仍要运行」。

## 用法

| 操作 | 效果 |
| --- | --- |
| 鼠标悬停圆点 | 圆点放大 1.2×，下方显示 4 字短义 |
| 单击圆点 | 弹出卡片：短义 / 判定问句 / 该记到笔记模板的哪一节 |
| 拖动圆点 | 移动整条；松手即记住（按显示器分别记忆） |
| 点条外任意处 | 收起卡片 |
| 托盘右键 | 显示 / 隐藏 / 重新载入配置 / 打开配置文件夹 / 退出 |
| 托盘双击 | 显示与隐藏切换 |

命令行开关：`ZoteroLegend.exe --always` 忽略「仅 Zotero 前台」，让条一直显示（调试或多机协同时用）。

## 8 色对照表

| 圆点 | 色值 | 短义 | 判定问句 | 落卡栏目 |
| --- | --- | --- | --- | --- |
| 🟡 | `#ffd400` | 主张 | 作者的核心结论是什么 | §1 一句话结论、§6 主张列 |
| 🔴 | `#ff6666` | 可疑·局限 | 哪里不成立 / 没证明 / 有前提 / 夸大 | §7 局限、§8 批判 |
| 🟢 | `#5fb236` | 方法 | 怎么做的（设定、步骤、公式、参数） | §4 关键方法 |
| 🔵 | `#2ea8e5` | 背景·定义 | 这是什么、别人做到哪了 | §2 动机、§3 骨架 |
| 🟣 | `#a28ae5` | 可复用 | 我能搬走什么（结构、表述、实验设计） | §9 可复用清单 |
| 🩷 | `#e56eee` | 表述素材（停用） | 不参与高亮，只留给「下划线」通道 | — |
| 🟠 | `#f19837` | 数据 | 测出来 / 算出来多少 | §5 实验、§6 证据列 |
| ⚪ | `#aaaaaa` | 待查·待办 | 读完之后还要做一件事（必须写批注） | 待办、§10 锚点 |

这套配色与「落卡栏目」来自笔者的 Obsidian 笔记体系（`文献阅读/Zotero 文献高亮配色方案.md`），表里的 § 编号是笔记模板的小节号。**你的模板不一样，直接改 `colors.json` 即可。**

## 改配色 / 改文案

编辑 `colors.json`，然后在托盘右键「重新载入配置」，不用重启：

```json
{
  "bar": {
    "offsetTop": 8,          // 距屏幕顶部的距离（逻辑像素）
    "dotDiameter": 18,       // 圆点直径
    "dotGap": 10,            // 圆点间距
    "hoverScale": 1.2,       // 悬停放大倍数
    "positions": {}          // 每块屏的拖动位置，程序自动写
  },
  "behavior": {
    "onlyWhenZoteroForeground": true,  // 只在 Zotero 前台时显示
    "pollIntervalMs": 200,             // 前台检测间隔
    "topmostRefreshMs": 3000           // 置顶续期间隔
  },
  "colors": [
    {
      "order": 1,
      "id": "yellow",
      "hex": "#ffd400",
      "short": "主张",
      "question": "作者的核心结论是什么",
      "cardSection": "§1 一句话结论、§6 主张列"
    }
  ]
}
```

给某个色加 `"disabled": true` 就变成虚线幽灵圈；`hex` 支持 `#RRGGBB`、`#AARRGGBB` 和 `#ARGB`。

## 自己编译

只需要 Windows 10/11 自带的 .NET Framework 4.8，**不用装 Visual Studio、不用装 .NET SDK**：

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

`build.ps1` 会调用系统里的 `csc.exe`（`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`）和 GAC 里的 WPF 程序集，产出 `dist\ZoteroLegend.exe`。源码是单文件 [`src/Program.cs`](src/Program.cs)，C# 5 语法。

## 已知限制

- 全屏独占游戏、UAC 安全桌面上盖不住（Windows 限制）。
- 默认只在 Zotero 前台时显示；想让它一直显示用 `--always`。
- 没有代码签名，SmartScreen 会提示未知发布者。
- 只支持 Windows（用到分层窗口、`SetWindowPos` 等 Win32 API）。

## 项目结构

```
colors.json                    配置源文件（构建时复制到 dist）
src/Program.cs                 全部逻辑：窗口、圆点、气泡、卡片、托盘、多屏/DPI
src/app.manifest               PerMonitorV2 DPI 声明
src/ZoteroLegend.exe.config    WPF 逐显示器 DPI 缩放开关
build.ps1                      编译脚本
install.ps1                    开机自启安装 / 卸载
dist/                          编译产物与使用说明
docs/adr/                      两个设计决策记录（为什么不做插件、为什么用 csc 编 WPF）
docs/research/                 GitHub 同类项目调研
docs/agents/                   本仓库的 agent 协作约定
.scratch/zotero-legend/spec.md 需求与验收清单
```

## License

MIT
