# 领域文档（Domain Docs）

工程技能在探索本仓库前，按下面的规则消费领域文档。

## 探索前先读

- 根目录的 **`GLOSSARY.md`**；或
- 若存在根目录的 **`GLOSSARY-MAP.md`**（多上下文），按主题读取它指向的各个 `GLOSSARY.md`
- **`docs/adr/`**：读与本次改动区域相关的 ADR；多上下文仓库还要看 `src/<context>/docs/adr/`

以上文件不存在就**静默跳过**：不要提示缺失，也不要一上来就建议创建。`/domain-modeling` 技能（经 `/grill-with-docs`、`/improve-codebase-architecture` 触达）会在术语或决策真正定下来时按需创建。

## 文件结构

单上下文仓库（绝大多数情况）：

```
/
├── GLOSSARY.md
├── docs/adr/
│   ├── 0001-event-sourced-orders.md
│   └── 0002-postgres-for-write-model.md
└── src/
```

多上下文仓库（根目录存在 `GLOSSARY-MAP.md`）：

```
/
├── GLOSSARY-MAP.md
├── docs/adr/                          ← 系统级决策
└── src/
    ├── ordering/
    │   ├── GLOSSARY.md
    │   └── docs/adr/                  ← 上下文级决策
    └── billing/
        ├── GLOSSARY.md
        └── docs/adr/
```

## 使用词汇表中的术语

输出里出现领域概念时（issue 标题、重构提案、假设、测试名），用 `GLOSSARY.md` 定义的说法，不要漂移到它明确避开的同义词。

如果需要的概念还没进词汇表，这是个信号：要么你在发明项目不用的语言（该重新考虑），要么是真的缺一块（记下来交给 `/domain-modeling`）。

## 标出 ADR 冲突

输出与既有 ADR 矛盾时，显式提出来，不要悄悄覆盖：

> _与 ADR-0007（event-sourced orders）冲突，但值得重开，因为……_
