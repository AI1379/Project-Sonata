# 剧情设计文档（Narrative Design）

| 字段 | 值 |
|---|---|
| 项目 | Project Sonata |
| 版本 | v0.1（草稿） |
| 最后更新 | 2026-07-19 |
| 状态 | 大纲整理完成；具体台本待写 |

## 文档目的

存放 Project Sonata 的**剧情设定、章节大纲、台本、暗线、美术方向**。

战斗机制见 [BattleDesign.md](../BattleDesign.md)；角色定位见 [CharacterDesign.md](../CharacterDesign.md)；本文档聚焦于"**故事如何被讲述**"——世界、人物、情节、情感、视听。

## 与原始笔记的关系

原始设计笔记 `Project-Sonata/拉赫之梦 - Project Sonata.md` 仍是项目的**设计源头**（保持 untracked，不在 git 管理），是创意的沙盒。本文档是该笔记的**结构化、版本化整理**，作为团队协作和实施依据。

笔记中的灵光一现、未定型想法继续保留在原笔记中；本目录的文档则只收录**已决定写入正式设定**的内容。两者冲突时以本目录为准。

## 阅读顺序

### 第一遍（快速理解整体）

1. [WorldSetting.md](./WorldSetting.md) — 世界观与基本设定
2. [NarrativeStructure.md](./NarrativeStructure.md) — 三幕式叙事结构
3. [Prologue.md](./Prologue.md) — 序幕设计（情感契约）

### 第二遍（按章节深入）

4. [Chapter01_SnowTrain.md](./Chapter01_SnowTrain.md) — 雪国列车 · 「无信号」
5. [Chapter02_FoggyGarden.md](./Chapter02_FoggyGarden.md) — 雾中花园 · 「调谐」
6. [Chapter03_DarkForest.md](./Chapter03_DarkForest.md) — 黑森林 · 「低频」
7. [Chapter04_LakeSpire.md](./Chapter04_LakeSpire.md) — 湖中尖塔 · 「月光主题与变奏」
8. [Chapter05_SpireTop.md](./Chapter05_SpireTop.md) — 塔尖 · 「钟声」
9. [Chapter06_InvertedTower.md](./Chapter06_InvertedTower.md) — 倒置之塔 · 「镜像下行」
10. [Chapter07_Replay.md](./Chapter07_Replay.md) — 来时路的重放 · 「变奏与展开」
11. [Chapter08_AbandonedStation.md](./Chapter08_AbandonedStation.md) — 废弃车站 · 「终局之战」
12. [Chapter09_DawnTrain.md](./Chapter09_DawnTrain.md) — 黎明列车 · 「尾声」

### 第三遍（深度阅读）

13. [HiddenThreads.md](./HiddenThreads.md) — 贯穿全程的叙事暗线
14. [ArtDirection.md](./ArtDirection.md) — 美术方向与角色视觉概念

## 文件结构

```
Docs/Narrative/
├── README.md                       (本文件)
├── WorldSetting.md                 世界观 / 主角 / 名字
├── NarrativeStructure.md           三幕式结构 / 9 章总览
├── Prologue.md                     序幕（拉二和弦闪回）
├── Chapter01_SnowTrain.md          第①章
├── Chapter02_FoggyGarden.md        第②章
├── Chapter03_DarkForest.md         第③章
├── Chapter04_LakeSpire.md          第④章
├── Chapter05_SpireTop.md           第⑤章
├── Chapter06_InvertedTower.md      第⑥章
├── Chapter07_Replay.md             第⑦章
├── Chapter08_AbandonedStation.md   第⑧章
├── Chapter09_DawnTrain.md          第⑨章
├── HiddenThreads.md                暗线（拉赫投射 / 磁带碎片）
└── ArtDirection.md                 美术方向 / 角色视觉
```

## 文档使用约定

- **已确定的设定**写在正文，给明确细节
- **未确定的部分**用 `> TODO` 引用块标记
- **每个章节文件末尾**有 `## 台本` 章节，目前为空，作为后续填写入口
- **剧情变更**应在 [HiddenThreads.md](./HiddenThreads.md) 末尾的"决策日志"留一条记录

## 相关文档

| 类型 | 路径 |
|---|---|
| 设计源笔记（untracked） | `Project-Sonata/拉赫之梦 - Project Sonata.md` |
| 工程架构 | `Project-Sonata/README.md` |
| 战斗系统设计 | `Project-Sonata/Docs/BattleDesign.md` |
| 角色与乐器设计 | `Project-Sonata/Docs/CharacterDesign.md` |
| 剧情设计（本目录） | `Project-Sonata/Docs/Narrative/` |
