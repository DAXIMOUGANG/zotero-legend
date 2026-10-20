# Zotero 七色高亮「桌面常驻图例条」开源调研

- 调研日期：2026-10-19
- 目标：做一个 Windows 桌面小工具——无边框、基本透明、置顶、桌面顶部居中的横向图例条，8 个彩色圆圈（黄/红/绿/蓝/紫/品红/橙/灰），悬停显示 4–6 字短义，Zotero 启动时自动出现，支持开机自启。
- 数据来源：GitHub REST API（经 gh-proxy 代理）、shields.io 徽章端点、Zotero 官方文档与论坛、仓库 README/ARCHITECTURE.md。star 数 shields 返回的是四舍五入值（如 "5.3k"），API 返回的才是精确值（如 5313），下文分别标注。

---

## 一、专门针对「Zotero 高亮配色 / 颜色含义」的项目

结论先说：**在检索到的证据范围内，没有任何项目做过「桌面常驻图例」这件事**。所有相关项目都活在 Zotero 窗口内部（阅读器右键菜单、取色弹窗、标注侧栏）。Zotero 官方本身也一直没有内置颜色标签功能——论坛 feature request「Labelling the highlight text colours」（discussion 103437）从 2024-07 一路 +1 到 2026-05 仍未实现，[官方插件页](https://www.zotero.org/support/plugins) 还写着「我们目前不提供可用插件列表，官方插件目录仍在计划中」，所以安装量没有官方口径可查。

### 1.1 MuiseDestiny/zotero-style（Ethereal Style）

| 项 | 值 |
| --- | --- |
| URL | https://github.com/MuiseDestiny/zotero-style |
| Star | 5,313（GitHub API 实测，2026-10-19）/ 5.3k（shields） |
| 最近更新 | `pushed_at` 2026-06-07；default 分支最近提交 shields 读数 "april"（2026-04） |
| License | AGPL-3.0 |
| 其它 | forks 165，open issues 798，语言 JavaScript；最新 release 下载量 257k |
| 功能一句话 | 「Ethereal Style」：阅读进度条、期刊等级标签、关系图视图，以及阅读器里的**标注颜色（Annotation Colors）**——自定义每个高亮色的名称、色值、顺序，可建多套色板、可导入导出 |
| 与本需求重合度 | **中**（解决了「颜色→语义命名」，配置模型几乎就是你要的 8 色表） |
| 是否解决「桌面常驻图例」 | **否**，只在 Zotero 阅读器的右键菜单/取色处生效 |

证据：论坛用户 2025-02-01 描述「插件设置里有 `Annotation Colors` 复选框，点编辑会弹出窗口，可自定义名称、颜色、排序，还能加比默认更多的颜色或定义多套色板，有导入导出」；作者另有 B 站介绍《Ethereal Style - 标注颜色》（https://www.bilibili.com/opus/992475545244532737）。

### 1.2 paulMrG2/zotero-highlight-descriptions（Zotero Highlight Descriptions）

| 项 | 值 |
| --- | --- |
| URL | https://github.com/paulMrG2/zotero-highlight-descriptions |
| Star | 9（API 实测）/ 9（shields） |
| 最近更新 | `pushed_at` 2026-08-22；创建于 2026-04-04；shields last-commit "august"（2026-08，与 API 一致） |
| License | MIT |
| 其它 | 语言 TypeScript，最新 release 下载 492，要求 **Zotero 8.0+** |
| 功能一句话 | 给每个高亮颜色加可自定义描述文字，显示在**标注右键菜单、选中文字后的取色弹窗、手绘笔记工具栏取色器**三处；可改名、可拖拽排序、卸载时清理偏好 |
| 与本需求重合度 | **高**（正是「取色弹窗里直接显示颜色名称」） |
| 是否解决「桌面常驻图例」 | **否**，只在 Zotero 窗口内 |

证据：Zotero 论坛 2026-04-24 有人推荐、2026-04-27 与 2026-05-24 有人反馈「装了就解决问题、Zotero 9 上可用」；README 明确列出三处显示位置与 Zotero 8+ 要求。

### 1.3 windingwind/zotero-actions-tags

| 项 | 值 |
| --- | --- |
| URL | https://github.com/windingwind/zotero-actions-tags |
| Star | 2.8k（shields） |
| 最近更新 | 未核实（shields 多次超时） |
| License | AGPL-3.0 |
| 功能一句话 | 动作与标签插件；社区用它的 Action Script（Discussion #211）改写阅读器里的颜色标签，达到「颜色带名字」的效果 |
| 与本需求重合度 | **低–中**（要写脚本、不是开箱功能） |
| 是否解决「桌面常驻图例」 | 否 |

### 1.4 zzlb0224/zotero-annotation-manage

| 项 | 值 |
| --- | --- |
| URL | https://github.com/zzlb0224/zotero-annotation-manage |
| Star | 125（shields） |
| 最近更新 / License | 未核实 |
| 功能一句话 | Zotero 标注的批量整理/导出 |
| 与本需求重合度 | **低**（不涉及颜色语义提示） |

### 1.5 检索结论

- 用「Zotero highlight color legend」「Zotero 高亮 颜色 图例 桌面 小工具」「zotero 七色 标注 颜色 含义 插件」「zotero color legend desktop widget」等中英文组合检索，**没有命中任何 GitHub 仓库、油猴脚本、AHK/PowerShell 小工具**在做「桌面常驻颜色图例」。
- 命中的全部是 Zotero 内部插件（上表）。也就是说：需求里「桌面常驻 / 悬停短义 / 开机自启 / Zotero 启动即出现」这四件，目前是空白。

---

## 二、通用「桌面置顶图例 / HUD / 悬浮条」项目

| 项目 | Star | License | 最近更新 | 技术栈 | 一句话 |
| --- | --- | --- | --- | --- | --- |
| [rainmeter/rainmeter](https://github.com/rainmeter/rainmeter) | 6.1k | GPL-2.0 | 活跃（shields last-commit "last saturday"） | C++（Win32） | Windows 桌面皮肤引擎，皮肤可置顶、可调透明、可设「不接收鼠标」 |
| [Code52/carnac](https://github.com/Code52/carnac) | 4.5k | MS-PL | **2020-06 后停更** | C# / WPF（.NET Framework） | 按键可视化浮层，可无边框、可点击穿透 |
| [brookhong/KeyCastOW](https://github.com/brookhong/KeyCastOW) | 1.4k | 未核实 | 未核实 | C++ / Win32 原生 | 体积极小的按键显示条，无边框半透明 |
| [Dogancanka/TraceLayer](https://github.com/Dogancanka/TraceLayer) | 10 | MIT | 2026-07（shields "july"） | Electron + React + TypeScript | 透明置顶描图纸覆盖层，带 Ghost Mode 点击穿透 + 全局快捷键 |
| [microsoft/PowerToys](https://github.com/microsoft/PowerToys)（Always On Top 模块） | 139k（整仓） | MIT | 活跃 | C# / WPF + Win32 | 给任意窗口加置顶边框，不是图例，但置顶实现可参考 |
| [linxuanm/overlay](https://github.com/linxuanm/overlay) | 27 | MIT | 未核实 | Python / tkinter | 用 tkinter 做屏幕覆盖层的轻量库，tk 路线的现成参考 |

### 2.1 各种技术栈的常见做法与坑

**C# / WPF（推荐路线）**
- 做法：`WindowStyle=None` + `AllowsTransparency=True` + `Background=Transparent` + `Topmost=True`，内容用 Canvas 画圆。
- 坑 1：`AllowsTransparency=True` 会关掉硬件加速（软件渲染），文字与圆边容易发虚；条目多时 CPU 占用上升。
- 坑 2：点击穿透必须走 Win32：`WS_EX_LAYERED(0x00080000) | WS_EX_TRANSPARENT(0x00000020)`，且 64 位下必须用 `GetWindowLongPtr/SetWindowLongPtr`（用 `GetWindowLong` 会截断）；在 `SourceInitialized` 之后设置才有效。
- 坑 3：`Topmost` 会被其它 topmost 窗口、UAC 安全桌面、全屏独占程序盖住，需要定时 `SetWindowPos(HWND_TOPMOST)` 续命。
- 坑 4：不进任务栏/Alt+Tab 需要 `ShowInTaskbar=false` + `WS_EX_TOOLWINDOW(0x00000080)`；不抢焦点加 `WS_EX_NOACTIVATE(0x08000000)`。

**Electron**
- 做法（TraceLayer 实测代码结构）：`transparent:true, frame:false, hasShadow:false`；`setAlwaysOnTop(true, 'screen-saver')`（比普通 topmost 更高一层）；点击穿透用 `win.setIgnoreMouseEvents(ghost, { forward: true })`。
- 关键坑：`setIgnoreMouseEvents(true)` 之后**收不到鼠标事件**，于是「悬停显示 tooltip」直接失效。TraceLayer 的解法是 `forward: true` 仍然转发 mousemove，renderer 里用 rAF 节流判断光标是否落在工具栏上，命中时临时 `setIgnoreMouseEvents(false)`——这正是「穿透 + hover」共存的标准套路。它还必须配一个全局快捷键（Ctrl+Alt+G）兜底，否则穿透状态下鼠标点不到任何东西、退不出来。
- 另一个坑：WebView2（Wails 等）里「逐像素透明 + 点击穿透」**不能同时用**，见 [wails #5812](https://github.com/wailsapp/wails/issues/5812) 的 issue 标题；Electron 没有这个限制，但透明窗口在部分显卡驱动上会黑底。
- 体积代价：Electron 打包通常在 100MB 以上，为一个 40px 高的图例条不划算。

**Python / tkinter**
- 做法：`root.overrideredirect(True)` + `root.attributes('-topmost', True)` + `root.attributes('-alpha', 0.85)`；色键透明用 `root.attributes('-transparentcolor', '#010101')`。
- 坑 1：`overrideredirect(True)` 后窗口没有任务栏入口、也进不了 Alt+Tab，一旦被别的窗口盖住很难找回（要留托盘图标或全局热键）。
- 坑 2：`-alpha` 是**整窗统一透明度**，文字会一起变淡；`-transparentcolor` 是色键透明，圆的抗锯齿边缘会出现彩色描边（fringing），且与 `-alpha` 同时用时行为不稳定。
- 坑 3：tk 没有原生 tooltip；hover 又要求窗口能接收鼠标，与「点击穿透」天然冲突（同 Electron 的坑）。
- 坑 4：打包（PyInstaller）后首启动慢、体积 15–30MB，杀软误报概率不低。

**AutoHotkey / PowerShell WinForms**
- AHK 常见写法是 `Gui +E0x20`（WS_EX_TRANSPARENT）、`+AlwaysOnTop`、`WinSetTransparent`，几十行就能出效果；但社区反复出现「只加 E0x20 依然能点到」的问题（AHK 论坛帖标题即《[Cannot Click Through GUI, E0x20 does not work.](https://www.autohotkey.com/boards/viewtopic.php?p=615890&sid=17cbde5131d90eb49c2037fe3dad276d)》），往往要配合 `+E0x80000`（LAYERED）或改用 `WinSetTransColor` 才稳。
- PowerShell WinForms 能做，但 tooltip + 悬停 + 透明组合起来代码比 WPF 更啰嗦，且没有编译产物、分发体验差。
- 这两条路线的定位：**原型验证极快，产品化偏弱**。

### 2.2 各栈共同的坑

1. **「无边框透明」和「点击穿透」和「悬停提示」三者互斥**：穿透的窗口收不到鼠标，就出不来 tooltip。正解是「区域化命中」——窗口整体不穿透，但用 `SetWindowRgn` 把可交互区域裁成「几个圆（+ tooltip 气泡）」，空隙处鼠标直接落到下层窗口；圆上仍能收 `WM_MOUSEMOVE` 出提示。微软文档在 `WS_EX_TRANSPARENT` 词条里也明确建议「To achieve transparency without these restrictions, use the SetWindowRgn function」。
2. **多显示器 + DPI**：必须 Per-Monitor V2 DPI 感知，居中按 `WorkingArea`（而不是 `Bounds`）算，否则 150% 缩放下会偏。
3. **全屏独占游戏 / UAC 安全桌面**：任何 overlay 都盖不上去，只能接受。
4. **开机自启**：写 `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` 即可，不需要管理员；注意命令行长度上限 260 字符，且微软明确说「系统不保证 Run 键里的程序多快启动，可能被延迟」（[MS Learn](https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys)）。要精确控制时机就用任务计划程序「登录时 + 延迟 10s」。

---

## 三、Zotero 生态里「取色弹窗显示颜色名/描述」的现状

| 维度 | Zotero 原生 | Zotero Style | Highlight Descriptions |
| --- | --- | --- | --- |
| 取色弹窗显示名称 | ❌（论坛请求 2024-07 至今未实现） | ✅ 阅读器右键菜单里的颜色带自定义名 | ✅ 选中文字弹窗 + 标注右键菜单 + 手绘取色器 |
| 颜色集合 | 7 个默认色 + 自定义 hex | 可增删颜色、多套色板、导入导出 | 按现有颜色逐个命名，可排序 |
| Star | — | 5,313 | 9 |
| 下载量（最新 release） | — | 257k | 492 |
| License | — | AGPL-3.0 | MIT |
| 维护状态 | 官方持续更新但功能未排期 | 活跃（2026-06 push） | 活跃（2026-08 push），作者在论坛跟进 |
| 能否替代「桌面常驻图例」 | 否 | 否 | 否 |

要点：
- 这两个插件解决的是「**在 Zotero 里选色时**知道这色代表什么」；出了 Zotero 窗口就没有任何提示。你在知网/网页/Word 里看到一段用色标记的笔记时，它们帮不上忙。
- 反过来，桌面图例条解决的是「**不打开 Zotero 也能看到对照表**」，两者不重复，可以并存。
- 安装量口径：Zotero 官方没有插件目录（页面自己写明「官方插件目录仍在计划中」），所以只能用 GitHub release 下载量当代理指标——Style 是 257k 量级，Highlight Descriptions 是 492 量级（很新、很小众，但功能正中靶心）。

---

## 四、结论与建议

### 4.1 选 (c) 自研，但「半复用」

- **没有现成方案可直接装**（(a) 不成立）：桌面常驻图例条 0 命中，最接近的 Highlight Descriptions 只有 9 star / 492 下载、且只在 Zotero 窗口内。
- **没有值得 fork 的项目**（(b) 不成立）：TraceLayer 是「描图纸」不是「图例」，它的窗口/穿透/快捷键机制可以抄，但业务层（8 个圆 + 4–6 字短义 + Zotero 联动）全要重写；Rainmeter 是皮肤引擎，为 8 个圆写一套皮肤比自研更绕。
- **自研成本很低**（(c) 成立）：核心就是「一个 40px 高的置顶条 + 8 个圆 + 自绘 tooltip + 进程轮询 + 注册表自启」，估 300–600 行。
- **建议组合**：
  - Zotero **内**的颜色命名 → 装 `zotero-highlight-descriptions`（MIT、Zotero 8+、功能最贴）或 `zotero-style`（功能全但 AGPL、体积大、AGPL 传染性对自研工具无影响，因为不链接代码）。
  - 桌面**外**的常驻图例 → 自研。两者共用同一份 `colors.json`（颜色 → 4–6 字短义），保证口径一致。

### 4.2 技术选型

| 方案 | 体积/依赖 | 上手速度 | 透明+穿透+hover | 结论 |
| --- | --- | --- | --- | --- |
| **C# WPF (.NET 8)** | self-contained 单文件 ~60MB / 框架依赖 ~200KB | 中 | 最好（逐像素 alpha + SetWindowRgn + 自绘 Popup） | **首选** |
| Python + tkinter | PyInstaller ~15–30MB | 快 | 一般（色键透明有锯齿、无原生 tooltip） | 次选/原型 |
| Electron | 100MB+ | 快 | 好（forward:true） | 不划算 |
| AHK v2 | 单 exe ~1MB | 快 | 差（E0x20 不稳定、tooltip 弱） | 只做原型 |
| PowerShell WinForms | 无编译 | 快 | 差 | 不推荐 |

### 4.3 值得抄的实现要点（按优先级）

1. **窗口只做「一条」**：高 ~40px、宽按内容（8 圆 × 32px + 间距），位置 = `PrimaryScreen.WorkingArea` 顶部居中、距顶 8px；Per-Monitor V2 DPI。
2. **透明**：WPF 用 `AllowsTransparency=True` + `WindowStyle=None` + `Background=Transparent`（逐像素 alpha，圆边干净），别用色键透明。
3. **命中测试用 `SetWindowRgn` 而不是全窗穿透**（这是本需求与普通 overlay 最大的区别）：把窗口区域裁成「8 个圆 + tooltip 气泡」的并集，空隙处鼠标直接穿透到 Zotero；圆上能收 mousemove → 出 tooltip。微软文档原话「To achieve transparency without these restrictions, use the SetWindowRgn function」。若要「点圆也穿透」，再对圆区域加 `WS_EX_TRANSPARENT` 并用 `WH_MOUSE_LL` 低级鼠标钩子自己命中检测（TraceLayer 的 forward:true 是同一思路的 Electron 版）。
4. **不抢焦点、不进 Alt+Tab**：`WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`，`ShowInTaskbar=false`。
5. **置顶续命**：启动时 `Topmost=true`，再起一个 2–5s 的定时器 `SetWindowPos(HWND_TOPMOST, ...)`，防止被别的 topmost 窗口顶下去。
6. **Tooltip 自绘**：不要用系统 tooltip（穿透/无焦点窗口下经常不弹）。在同一个窗口里画一个圆角气泡、或用另一个同风格小窗口；气泡区域也要算进 `SetWindowRgn`。
7. **Zotero 联动**：每 2s `Process.GetProcessesByName("zotero")`，有则显示、无则隐藏。别去读 `zotero.sqlite`（运行中被 Zotero 占用、WAL 状态复杂）；颜色配置放自己的 `colors.json`，可选支持从 Highlight Descriptions 的设置里手工同步一次。
8. **开机自启**：`HKCU\Software\Microsoft\Windows\CurrentVersion\Run` 写一个值即可（≤260 字符、无需管理员）；要等桌面加载完再显示，就任务计划程序「登录时 + 延迟 10s」。
9. **兜底热键**：给一个全局快捷键（如 Ctrl+Alt+L）显隐整条，避免「窗口跑到屏幕外/被盖住」时抓瞎——TraceLayer 的 Ctrl+Alt+G 是同样的设计。

---

## 一句话结论

**没有任何现成项目做过「Zotero 颜色桌面常驻图例」，最接近的 `zotero-highlight-descriptions`（9★/MIT/492 下载）只解决 Zotero 窗口内的颜色命名；建议自研一个约 300–600 行的 C# WPF 置顶条，用 `SetWindowRgn` 做区域命中（而不是全窗穿透）来同时拿到「点击穿透 + 悬停提示」，Zotero 内的命名则直接装现成插件，两端共用同一份 `colors.json`。**

---

## 附：数据核实说明

- Star 数：`shields.io`（四舍五入）与 `api.github.com/repos/...`（精确）两种读数，均于 2026-10-19 抓取；`api.github.com` 在本机 DNS 被解析到非公网地址，实际通过 `gh-proxy.com` 代理访问，其共享出口 IP 随后触发 GitHub API 速率限制（403），因此部分仓库只拿到 shields 读数。
- 「最近更新」：优先用 API 的 `pushed_at`；拿不到时用 shields `last-commit` 徽章，其输出为「月份名」表示当年（2026），「月份 年份」表示更早（例如 carnac 为 "june 2020"）。
- 标注「未核实」的字段（KeyCastOW 的 license 与更新时间、zotero-actions-tags 的更新时间、zotero-annotation-manage 的 license 与更新时间）在抓取时 shields 端点超时/失败，未做二次确认，请勿当作事实引用。
- Zotero 官方无插件目录、无安装量统计，`zotero-style` 的 257k 与 Highlight Descriptions 的 492 均为「最新 release 的 GitHub 下载量」，只是安装量的近似代理。
