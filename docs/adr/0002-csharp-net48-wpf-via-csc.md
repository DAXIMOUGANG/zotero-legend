# 用本机 csc 编译 .NET Framework 4.8 WPF，不装 SDK、不用 PowerShell

本机没有 `dotnet` 命令行，但有 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`，可把 C# 直接编译成单个 `.exe`。选 WPF 分层窗口（`AllowsTransparency=True`）是因为它天然逐像素 alpha：不透明处收得到鼠标（悬停出短义、点击出卡片），全透明处点击自动穿透到 Zotero——正好同时满足「只留圆圈」「圆圈可交互」「其余区域点得穿」三条互相拉扯的要求，不需要 `SetWindowRgn` 裁区域。

**考虑过的替代**：PowerShell 5.1（与 `11_WindowsProject` 一致、可读可改，但透明只能色键抠图，圆边锯齿且与悬停命中互相干扰，托盘/前台判定还要额外写 P/Invoke）；Python + tkinter（本机无 pystray、托盘要联网装包，tkinter 无原生 tooltip）；Electron（体积 100MB+，不划算）。

**代价**：只能用 C# 5 语法（csc 4.0 的限制），改运行逻辑要重新编译（交付里带 `build.ps1`）；但改颜色、短义、位置全在 `colors.json`，用户不必碰代码。
