# 问题跟踪器：本地 Markdown

本仓库的问题（issue）与 spec 都以 Markdown 文件存放在 `.scratch/`。

## 约定

- 一个 feature 一个目录：`.scratch/<feature-slug>/`
- spec 文件：`.scratch/<feature-slug>/spec.md`
- 实现票（ticket）一票一文件：`.scratch/<feature-slug>/issues/<NN>-<slug>.md`，从 `01` 起编号；**不要**把所有票合并进一个文件
- 每个票文件顶部用 `Status:` 行记录 triage 状态（角色字符串见 `triage-labels.md`）
- 评论与对话历史追加到文件末尾的 `## Comments` 标题下

## 技能说「发布到 issue tracker」时

在 `.scratch/<feature-slug>/` 下新建文件（目录不存在就创建）。

## 技能说「取出相关 ticket」时

直接读引用路径。用户通常会给出路径或票号。

## Wayfinding 操作（供 `/wayfinder` 使用）

- **地图**：`.scratch/<effort>/map.md`，正文写 Notes / 已定的决策 / 迷雾
- **子票**：`.scratch/<effort>/issues/NN-<slug>.md`，从 `01` 起编号，正文写问题；`Type:` 行记录票型（`research` / `prototype` / `grilling` / `task`），`Status:` 行记录 `claimed` / `resolved`
- **阻塞**：顶部 `Blocked by: NN, NN` 行；列出的文件全部 `resolved` 后才解锁
- **前沿**：扫 `.scratch/<effort>/issues/`，取「未关闭 + 未阻塞 + 未被认领」的票，编号最小者优先
- **认领**：动工前把 `Status:` 改成 `claimed` 并保存
- **解决**：在 `## Answer` 标题下写答案，`Status:` 改为 `resolved`，再把上下文指针（要点 + 链接）追加到 `map.md` 的「已定的决策」
