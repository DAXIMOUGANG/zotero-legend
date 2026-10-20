# 桌面外挂一条图例条，而不是做 Zotero 插件

Zotero 原生不支持给高亮色命名；现有插件（zotero-style、zotero-highlight-descriptions）只能把颜色名显示在 **Zotero 窗口内部**，一旦离开 Zotero（知网、网页、Word 里看带色标记的笔记）就帮不上忙。而本需求的形态本身就是「桌面顶部一条常驻、悬停看短义的圈圈条」，它必须活在 Zotero 之外，所以做成一个独立的置顶小程序，靠「前台窗口是否为 zotero.exe」决定显隐，不碰 Zotero 内部、不依赖其版本升级。

**考虑过的替代**：装 zotero-style 或 zotero-highlight-descriptions 插件（只在 Zotero 内生效，不满足形态）；fork TraceLayer（Electron 描图纸，业务层要全重写，100MB+）；Rainmeter 皮肤（写皮肤比自研更绕）。调研结论：全网没有现成的「Zotero 颜色桌面常驻图例」，见 [调研报告](../research/2026-10-19-zotero-annotation-legend-research.md)。
