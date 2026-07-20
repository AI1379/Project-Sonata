# 战斗系统设计文档（Battle Design）

| 字段 | 值 |
|---|---|
| 项目 | Project Sonata |
| 版本 | v0.1（草稿） |
| 最后更新 | 2026-07-19 |
| 状态 | Phase 1 设计中 |
| 优先级 | 高于 README（与 README 冲突时以本文档为准） |

---

## 1. 文档目的

定义 Project Sonata 战斗系统的**完整规则**，作为代码实现、数值调整、UI 反馈、测试覆盖的唯一参考。

README 提供**工程架构**（目录结构、asmdef、MonoBehaviour 分层），本文档提供**游戏设计**（规则、数值、机制）。角色与乐器定位见 [CharacterDesign.md](./CharacterDesign.md)。

### 文档使用约定

- **已确定的规则**写在正文，给明确数值
- **未确定的部分**用 `> TODO` 引用块标记，附上待回答的问题
- **每次重大修改**在 [§14 设计决策日志](#14-设计决策日志) 留一条记录
- 章节号稳定，新增内容优先插入子章节而非重排

---

## 2. 设计哲学

### 2.1 核心循环

```
观察敌人 ToneStack
  → 选择技能挂音
  → 形成共鸣（自动结算）
  → 效果作用于敌人/己方
  → 重复
```

### 2.2 五条核心原则

| 原则 | 含义 | 反例 |
|---|---|---|
| **自动触发** | 共鸣形成即结算，不需要"按按钮触发" | 不要做"Trigger Resonance"独立技能 |
| **路径即策略** | 同一最终共鸣可通过不同挂音顺序达成，路径不同则效果不同 | 不要硬塞路径倍率，差分必须源自机制 |
| **三音为终极目标** | 三和弦是最强共鸣，玩家应围绕"凑三音"规划 | 不要让 2 音共鸣抢夺 3 音和弦的舞台 |
| **过程有代价** | 凑音过程中可能误触发次级共鸣，付出资源/位置代价 | 不要让挂音完全零风险 |
| **规则可演化** | 当前为 Phase 1 简化版，Buff/DoT/Tempo 等留接口 | 不要把临时实现写死到核心数据结构 |

### 2.3 设计参考

- **原神元素反应**：自动触发、底/触发元素、附着量、配队门槛与倍率差异
- **传统 JRPG 行动条**（ATB）：Tempo 系统将基于此
- **古典乐理**：和弦构建、调性色彩、五度圈

---

## 3. 核心概念词汇表

| 术语 | 含义 |
|---|---|
| **Tone（音）** | C / D / E / F / G / A / B 七个自然音之一 |
| **Tonality（调性）** | Major / Minor / Dissonant / Silent，全局音乐氛围 |
| **ToneStack（音堆）** | 一个单位身上挂的音的集合，最多 3 个 |
| **Base Tone** | ToneStack 的不可移除基础音（参考原神元素史莱姆） |
| **Resonance（共鸣）** | ToneStack 上的音形成的特定组合，触发效果 |
| **Trigger Tone（触发音）** | 最后被添加到 stack 的音，决定共鸣的"路径" |
| **Formation Path（形成路径）** | 3 音和弦形成时，根据中间过程判定的子类型 |
| **Harmonic Armor（和声护甲）** | Perfect Fifth 状态给敌人的临时减伤 buff |

---

## 4. 数据模型（关键类）

> 此节定义**规则层（Domain）**的数据结构。表现层（MonoBehaviour）见 README。

### 4.1 已实现

```csharp
BattleUnit
  - DisplayName: string
  - MaxHp / CurrentHp: int
  - Attack / Speed: int
  - IsPlayerUnit: bool
  - Skills: SkillDefinition[]
  - ToneStack: ToneStack
  - IsAlive: bool
  - OnHpChanged: event

ToneStack
  - Tones: IReadOnlyList<Tone>       // 最多 3 个
  - BaseTone: OptionalField<Tone>    // 不可移除基础音
  - OnChange: event
  - AddTone(tone) / ClearTones()

Tone (enum)        // C, D, E, F, G, A, B
Tonality (enum)    // Major, Minor, Dissonant, Silent
ResonanceType (enum)
                   // None, Unison, Dissonance, MajorTriad, MinorTriad, PerfectFifth

SkillDefinition : ScriptableObject
  - DisplayName, Power, AppliesTone, ToneToApply, CanTriggerResonance

SkillResolver
  - Resolve(caster, target, skill) → SkillResult

ResonanceDetector (static)
  - Detect(tones) → ResonanceMatch

ResonanceCatalog (static)
  - Default: Dictionary<ResonanceType, ResonanceConfig>
```

### 4.2 待实现

```csharp
// 路径识别
enum ResonanceFormationPath
{
    Direct,             // 2 音直接形成（如不协和）
    TriadViaThird,      // 先 3 度对，最后补 5 度音（CE→G）
    TriadViaFifth,      // 先 5 度对，最后补 3 度音（CG→E）
}

// Buff 系统（最小集）
interface IBuff
{
    BuffType Type { get; }
    int RemainingCharges { get; }   // 触发次数或回合数
}

class HarmonicArmor : IBuff         // Perfect Fifth 减伤
class DoTStatus : IBuff              // Minor Triad 的持续伤害（TODO）
```

> TODO Buff 系统的具体接口设计（一次性触发 vs 回合衰减）

---

## 5. 共鸣系统总览

### 5.1 共鸣类型一览

| 类型 | 音数 | 触发条件 | 效果方向 | 状态 |
|---|---|---|---|---|
| Unison（齐奏） | 2+ | 相同音 | 强化同音技能 | **未启用**（ToneStack 拒绝重复音） |
| Dissonance（不协和） | 2 | 相邻自然音 | 即时小伤害 + 反噬 | ✅ |
| Perfect Fifth（完全五度） | 2 | 五度音程 | 敌人 buff + 路径加成 | 🚧（设计稳定，实现中） |
| Major Triad（大三和弦） | 3 | 大三和弦组合 | 高伤害（路径决定倍率） | 🚧（基础伤害已实现，路径未实现） |
| Minor Triad（小三和弦） | 3 | 小三和弦组合 | 中伤害 + DoT | 🚧（基础伤害已实现，DoT 待实现） |

### 5.2 Stack 行为

| 情况 | 行为 |
|---|---|
| 添加重复音 | 拒绝（warning log） |
| Stack 已满 3 个 | 挤掉最旧音（若 BaseTone 启用则保留 BaseTone） |
| 触发 Dissonance | 立即结算 + **清空 stack** |
| 触发 Perfect Fifth（2 音阶段） | 附加 Harmonic Armor buff，**不清空 stack** |
| 触发 Perfect Fifth（3 音未成和弦） | 立即结算 buff 效果 + 清空 stack |
| 触发三和弦 | 立即结算 + 清空 stack |

### 5.3 共鸣识别优先级

```
1. 3 音和弦（最高）
   ├─ Major Triad
   └─ Minor Triad
2. 2 音破坏性共鸣（即时清空）
   └─ Dissonance
3. 2 音附着性共鸣（附加 buff，不清空）
   └─ Perfect Fifth
```

**关键约束**：2 音 Perfect Fifth **不立即结算伤害**，只附加状态。否则会抢占 3 音和弦（如 C-G 抢 C-E-G 大三和弦）。

---

## 6. 共鸣详细规则

### 6.1 Dissonance（不协和）

**触发条件**：stack 最后两个音是相邻自然音（C-D / D-E / E-F / F-G / G-A / A-B）

**效果**：

| 对象 | 效果 |
|---|---|
| 目标 | 受 15 真实伤害 |
| 触发者 | 受 5 反噬伤害 |

**Stack 行为**：清空

**设计意图**：快速、可重复、低门槛的小反应。用作"凑不出和弦时的 fallback 输出"，但反噬让玩家不会无脑刷。

> TODO 反噬是否改为别的代价（如降低 caster Tempo / 消耗资源）

### 6.2 Major Triad（大三和弦）

**触发组合**：`C-E-G` / `F-A-C` / `G-B-D`

**基础效果**：目标受 40 伤害

**路径变体**：

| 路径 | 中间过程 | 倍率 |
|---|---|---|
| TriadViaThird（CE→G） | stack 经过 [C,E] 状态，无 buff | 1.0x（基础 40） |
| TriadViaFifth（CG→E） | stack 经过 [C,G] 状态，敌人获得 Harmonic Armor | > TODO 推荐 1.5x - 2.0x |

**Stack 行为**：清空

**触发音识别**：和弦根音 + 2 = 三度音；根音 + 4 = 五度音。最后添加的音决定路径。

### 6.3 Minor Triad（小三和弦）

**触发组合**：`A-C-E` / `D-F-A` / `E-G-B`

**基础效果**：目标受 25 伤害 + 附加 DoT

**路径变体**：

> TODO Minor Triad 是否也做路径差分？目前倾向**不做**，让 Minor Triad 体现"色彩"，Major Triad 体现"路径深度"，避免设计冗余。

**Stack 行为**：清空

> TODO DoT 系统（伤害量、持续回合、是否可叠加）

### 6.4 Perfect Fifth（完全五度）

详见 [§7](#7-完全五度的双路径机制)。

### 6.5 Unison（齐奏）

**当前状态**：未启用。ToneStack 拒绝添加重复音，所以永远无法触发。

**保留原因**：未来引入"强化同音技能"机制时启用。

> TODO 是否移除 enum 项以避免混淆

---

## 7. 完全五度的双路径机制

### 7.1 设计目标

让"凑大三和弦的挂音顺序"成为玩家的策略选择，而非纯粹的执行步骤。机制对应原神的"底/触发元素"与"附着量"。

### 7.2 状态定义

当 ToneStack 含完全五度对（`C-G` / `D-A` / `E-B` / `F-C` / `G-D` / `A-E`）时，敌人附加 **Harmonic Armor** buff：

| Buff | 效果 | 持续 |
|---|---|---|
| Harmonic Armor | 下次受共鸣伤害减免 X% | 直到下次共鸣结算（一次性） |
| Cleanse | 移除自身一个负面状态 | 立即 |

> TODO Harmonic Armor 减免比例（推荐 30%-50%）
> TODO Cleanse 是否保留——若 Phase 1 不做 debuff 系统，可改为别的次要效果（如小伤害反弹）

### 7.3 触发流程

```
玩家挂音到 ToneStack
  │
  ├─ 检测 3 音和弦（最高优先级）
  │    ├─ 是 MajorTriad / MinorTriad？
  │    │    ├─ 是：触发三和弦
  │    │    │    ├─ 检查敌人 Harmonic Armor 状态
  │    │    │    ├─ 激活：路径 = ViaFifth，伤害 = 基础 × 倍率（消耗 buff）
  │    │    │    └─ 未激活：路径 = ViaThird，伤害 = 基础
  │    │    └─ 否：进入 2 音判定
  │    │
  │    └─ 2 音判定
  │         ├─ Dissonance（相邻）→ 立即结算 + 清空
  │         ├─ PerfectFifth（五度）→ 仅附加 Harmonic Armor，不清空
  │         └─ 其他 → 维持 stack
  │
  └─ stack 满 3 音仍未成和弦？
       ├─ 含 PerfectFifth 对？→ 触发 PerfectFifth 结算（buff 已应用）+ 清空
       └─ 否 → 维持 stack（玩家可继续挂音挤掉旧音）
```

### 7.4 路径对比

| 路径 | 挂音顺序 | 中间过程 | 最终伤害 | 适用场景 |
|---|---|---|---|---|
| **TriadViaThird** | `CE→G` | 敌人无 Harmonic Armor | 基础伤害（40） | 灵活、避免 buff 浪费、对配队要求低 |
| **TriadViaFifth** | `CG→E` | 敌人获得 Harmonic Armor | 基础 × 倍率 | 高爆发，需要规划 |

### 7.5 失败情况

| 场景 | 后果 |
|---|---|
| `[C,G]` 后挂非 E 音（如 B、D、F） | 未构成和弦，stack 满 3 音，触发 Perfect Fifth 独立结算，stack 清空。玩家失去继续凑和弦的机会 |
| `[C,G]` 后挂 A → `[C,G,A]` | 未构成和弦但含 CG 五度对，触发 Perfect Fifth 结算，stack 清空 |
| `[C,E]` 后挂非 G 音（如 B、F） | 未构成和弦，stack 维持，玩家可继续挂音挤掉最旧音 |

### 7.6 设计决策依据

1. **机制驱动而非倍率硬塞**：路径差异源自"是否付出 Harmonic Armor 代价"，倍率只是这个代价的补偿
2. **完全五度作为"风险共鸣"**：玩家可故意触发（换取 Cleanse 等防御效果）或避免触发（保留输出节奏）
3. **避免立即触发抢占和弦**：2 音阶段 Perfect Fifth 只附加状态，不立即结算伤害（除非 stack 满 3 音仍无和弦）
4. **失败有明确惩罚**：误触发 Perfect Fifth 独立结算 = 浪费凑和弦的进度

### 7.7 与原神机制的对应

| 原神机制 | Project Sonata 对应 |
|---|---|
| 火元素附着在敌人身上 | Perfect Fifth 状态附着（Harmonic Armor） |
| 火底期间持续触发蒸发（高倍率） | Perfect Fifth 状态下凑成大三和弦（额外倍率） |
| 元素量消耗完毕火底消失 | 补音后五度状态结束、转成大三和弦 |
| 火底白消耗（没接到反应）= 配队失败 | 加错音让五度只剩 buff = 操作失败 |
| 水底低门槛低收益（持续蒸发 1.5x） | CE→G 路径低门槛基础伤害 |

---

## 8. 资源系统

战斗中存在两层资源：

| 资源 | 作用 | 所有制 |
|---|---|---|
| **共鸣槽（Resonance Gauge）** | 大招充能 | **角色独立** |
| **节拍点（Beat Points）** | 释放技能 | **全队共享** |

### 8.1 共鸣槽（Resonance Gauge）

**作用**：角色独立的大招充能条。满槽后可释放该角色大招。

**积累规则**：每次触发共鸣，**触发者**的共鸣槽积累：

| 行为 | 共鸣槽变化 |
|---|---|
| 触发 Major Triad | +20 |
| 触发 Minor Triad | +15 |
| 触发 Perfect Fifth（含双路径大三和弦） | +10 |
| 触发 Dissonance | +5 |
| 大招释放 | 重置为 0 |

**满槽阈值**：100

**角色独立性**：每个角色的共鸣槽独立积累，互不影响。一个角色满槽不会影响其他角色。

**为什么用共鸣槽而非传统能量条**：
- 每次共鸣都是积累，玩家不会陷入"普攻攒大"循环
- 不同体系的节奏自然分化（直伤队共鸣频繁、DoT 队共鸣少但大招强力）
- 与本作"凑和弦"的核心循环深度绑定

> TODO 是否需要"溢出"机制（满槽后继续触发共鸣获得其他收益）

### 8.2 节拍点（Beat Points）

**作用**：全队共享的技能消耗资源，限制"每回合放最强技能"。

**性质**：
- 全队共享一个池（不分敌我方？不，仅我方共享）
- 上限 **5**
- 战斗起始为上限

**回复方式**：

| 行为 | 节拍点变化 |
|---|---|
| 每回合开始 | +1（不超过上限） |
| 使用基础技能（普攻 / 生成型卡） | +1 |
| 鼓组特定技能 | +2 或 +3 |

**消耗方式**：

| 行为 | 节拍点变化 |
|---|---|
| 中等技能 | -1 |
| 强力技能 | -2 |
| 大招 | 不消耗（消耗共鸣槽而非节拍点） |

**节拍点为 0 时**：
> TODO 是否允许使用消耗型技能：禁止 / 允许但受罚（如伤害减半）

**设计意图**：
- 防止"每回合放最强技能"循环
- 让鼓组作为"节拍点生成者"有天然战略价值
- 让玩家在"放强力技能 vs 攒节拍点"之间做选择

### 8.3 资源系统与行动条的关系

> 待 §9 行动条 / Tempo 实现后细化。需要明确：
> - 节拍点按"轮"恢复 vs 按"角色行动"恢复
> - 共鸣槽溢出时的处理
> - 大招占用行动条的方式（独立行动 / 替代普攻 / 不占行动）

### 8.4 数据模型（待实现）

```csharp
class ResourceManager
{
    int ResonanceGauge[BattleUnit]   // 每角色独立的共鸣槽
    int BeatPoints                   // 全队共享的节拍点

    void AddResonance(BattleUnit unit, int amount)
    void SpendBeatPoints(int amount)
    void AddBeatPoints(int amount)
}

// 技能卡需要声明节拍点消耗（详见 CharacterDesign §13）
class SkillCard
{
    int BeatPointCost      // 可为负数（回复）
    bool IsUltimate        // 大招卡：消耗共鸣槽而非节拍点
}
```

### 8.5 设计参考

| 参考 | 借鉴点 |
|---|---|
| **原神 Q 充能** | 元素爆发充能条（角色独立） |
| **星穹铁道战技点** | 全队共享，上限 5，普攻 +1 / 战技 -1 |
| **重返未来：1999** | 全队共享资源机制（心相系统） |

---

## 9. 行动条 / Tempo 系统

> 当前未实现。所有玩家单位同时行动，无敌人 AI。

### 8.1 设计意图

战斗应有"节奏感"——音乐主题需要节奏。第一版使用行动条而非传统回合，让 Speed 数值有意义。

### 8.2 草案

```
每个单位：actionGauge += speed * delta
actionGauge >= 100 → 进入 Ready
执行动作后 actionGauge -= 100
鼓类技能可调整 actionGauge：己方 +10 / 敌方 -10
```

> TODO 离散推进 vs 实时推进
> TODO 暂停 / 等待玩家输入的处理
> TODO 敌人 AI 行为树与 Tempo 的关系

---

## 10. 敌人 AI

> 当前敌人完全静止，仅作为共鸣目标。

### 9.1 设计意图

- 简单的"挂音 → 触发共鸣"反向逻辑：敌人也能给玩家挂音
- 鼓系敌人能延迟玩家行动条
- 三和弦敌人能爆发

### 9.2 行为优先级草案

```
1. 若自身 stack 有潜在和弦 → 优先补音凑成
2. 若玩家单位 stack 接近形成和弦 → 挂相邻音破坏（故意触发 Dissonance 反噬玩家）
3. 否则随机攻击
```

> TODO 行为树 / 状态机选型
> TODO Boss 阶段切换机制

---

## 11. 数值平衡表

> 所有数值均为占位，需迭代调整。修改时在 [§14](#14-设计决策日志) 留记录。

### 11.1 共鸣伤害

| 共鸣类型 | 基础伤害 | 自身反噬 | 路径倍率 |
|---|---|---|---|
| Dissonance | 15 | 5 | - |
| Perfect Fifth | 0 | 0 | - |
| Major Triad | 40 | 0 | Via Third 1.0x / Via Fifth > TODO |
| Minor Triad | 25 | 0 | > TODO |
| Unison | - | - | 未启用 |

### 11.2 Buff / Debuff

| Buff | 来源 | 效果 | 数值 |
|---|---|---|---|
| Harmonic Armor | Perfect Fifth | 下次共鸣伤害减免 | > TODO 30%-50% |
| Minor DoT | Minor Triad | 持续伤害 N 回合 | > TODO 5/回合 × 3 回合 |

### 11.3 单位数值

> 详细角色定位与设计见 [CharacterDesign.md](./CharacterDesign.md)。本表仅给出战斗规则需要的数值占位。

| 类型 | HP | ATK | SPD | 备注 |
|---|---|---|---|---|
| 玩家（小提琴，Strings） | 100 | 10 | 10 | 铺音符、群体标记 |
| 玩家（长笛，Winds） | > TODO | > TODO | > TODO | 精准补音、单体爆发 |
| 玩家（鼓，Drum Kit） | > TODO | > TODO | > TODO | Tempo 操控、推进行动条 |
| 玩家（小号，未分类） | > TODO | > TODO | > TODO | 现有 asset，定位待定（可能并入 Winds 或独立） |
| 普通敌人 | 80 | 8 | 8 | 占位 |
| Boss | > TODO | > TODO | > TODO | 占位 |

### 11.4 调性效果

> TODO 调性如何影响战斗？目前仅作为 metadata。

---

## 12. UI 反馈规范

### 12.1 必要元素

| 元素 | 位置 | 显示 |
|---|---|---|
| 单位 HP 文本 | 单位面板 | `HP {CurrentHp}/{MaxHp}` |
| 单位 ToneStack | 单位面板下方 | 当前挂的音 chip（如 `[C][E]`） |
| Harmonic Armor 状态 | 敌人单位面板 | 护盾图标 / 状态 chip |
| 共鸣触发提示 | 底部 `currentSkill` TMP | `RESONANCE: Major Triad (60 dmg)` |
| 战斗日志 | > TODO | 历史动作（当前仅 Debug.Log） |

### 12.2 共鸣触发反馈

| 共鸣 | 反馈 |
|---|---|
| Dissonance | 红色闪烁 |
| Perfect Fifth | 金色光环 + 护盾图标浮现 |
| Major Triad | 明亮光柱 + 屏幕轻微震动 |
| Minor Triad | 暗紫光柱 + DoT 图标 |

> TODO 反馈系统统一架构（当前 BattleUnitView 直接订阅事件，后续可能需要 BattleEventPlayer）

---

## 13. 测试策略

### 13.1 EditMode 单元测试

| 测试目标 | 覆盖范围 | 状态 |
|---|---|---|
| ResonanceDetector | 所有 5 类共鸣的识别 + 优先级 + 路径 | ✅（基础识别）/ 🚧（路径） |
| ToneStack | 添加、清除、容量限制、BaseTone | > TODO |
| SkillResolver | 完整结算链路（无 UnityEngine） | > TODO |
| PerfectFifth 路径 | Harmonic Armor 应用、路径倍率 | > TODO |
| BattleUnit | HP 变化、OnHpChanged 事件 | > TODO |

### 13.2 PlayMode 集成测试

> TODO 完整战斗循环、UI 刷新正确性、Prefab 实例化

---

## 14. 未实现 / 待定 / TODO 清单

### 14.1 Phase 1 剩余项

- [ ] Buff 系统（Harmonic Armor、DoT）
- [ ] 完全五度的双路径倍率数值定稿
- [ ] Minor Triad 的 DoT 实现
- [ ] ActionTimeline / Tempo 系统
- [ ] 资源系统（共鸣槽 + 节拍点）
- [ ] 技能卡系统（4 槽装备、节拍点消耗）
- [ ] 大招（消耗共鸣槽）
- [ ] 敌人 AI
- [ ] 战斗胜负判定
- [ ] 调性切换及其战斗效果
- [ ] 战斗日志面板 UI
- [ ] 共鸣触发动画 / 反馈

### 14.2 Phase 2 预留

- [ ] 鼓手专属"共鸣强化"机制（待评估是否保留手动触发元素）
- [ ] 多角色协作连携（如小提琴铺音 + 长笛爆发）
- [ ] 钢琴 Boss 的调性切换机制
- [ ] 动态音乐系统（战斗层 / 调性层 / Boss 阶段层）

### 14.3 设计待定问题清单

> TODO PerfectFifth 独立结算时是否清空 stack？（当前草案：清空）
> TODO Harmonic Armor 减免比例
> TODO CG→E 路径大三和弦倍率
> TODO 不协和反噬是否改为别的代价
> TODO Minor Triad 是否也做路径差分
> TODO Unison 是否从 enum 移除
> TODO 添加 Base Tone 的实际机制（哪些单位有、是否可破坏）

---

## 15. 设计决策日志

记录"为什么这么决定"，避免未来反复讨论。最新决策放在最上面。

### 2026-07-19 · 资源系统双层架构（共鸣槽 + 节拍点）

**决定**：战斗资源分两层——共鸣槽（角色独立、大招充能）+ 节拍点（全队共享、技能消耗）。

**理由**：
- 共鸣槽与"凑和弦"核心循环深度绑定，每次共鸣都是积累，避免传统能量条的"普攻攒大"循环
- 节拍点限制"每回合放最强技能"，给鼓组"节拍点生成者"天然战略价值
- 双层资源让不同体系的节奏自然分化（直伤队共鸣频繁、DoT 队共鸣少但大招强力）
- 节拍点参考星铁战技点（已验证的设计），共鸣槽为本作原创

### 2026-07-19 · PerfectFifth 双路径机制

**决定**：完全五度触发时附加敌人 buff（Harmonic Armor），凑成大三和弦时根据是否触发过 PerfectFifth 决定路径倍率。

**理由**：
- 模仿原神"底元素消耗 → 后续反应加成"逻辑
- 让"挂音顺序"成为真正的策略选择
- 比硬塞路径倍率更有机制支撑
- 失败情况（误触发五度独立结算）作为天然风险

### 2026-07-19 · 2 音 PerfectFifth 不立即结算

**决定**：2 音阶段 PerfectFifth 只附加 buff，不清空 stack。

**理由**：立即清空会让玩家永远凑不到大三和弦（CG→E 路径不成立）。仅在 stack 满 3 音且未构成和弦时才独立结算 PerfectFifth。

### 2026-07-19 · 共鸣自动触发机制

**决定**：共鸣形成即自动触发，不引入"手动触发"按钮。

**理由**：
- 对标原神元素反应，自动触发更顺畅
- 当前无 ActionTimeline，手动触发增加 UI 复杂度
- 鼓手"共鸣触发者"职责可在 ActionTimeline 后通过"强化共鸣"技能体现，而非"唯一可触发者"

### 2026-07-19 · Phase 1 用 POCO 而非 ScriptableObject 配置共鸣

**决定**：共鸣数值放在 `ResonanceCatalog` 静态类，暂不创建 ScriptableObject asset。

**理由**：
- 减少设计迭代时的来回编辑（改一处代码 vs 多个 asset）
- Phase 1 数值仍频繁调整
- 后续数值稳定后再迁移到 ScriptableObject，API 已留好 `IReadOnlyDictionary<ResonanceType, ResonanceConfig>` 参数

---

## 16. 术语对照

| 中文 | 英文 | 备注 |
|---|---|---|
| 音 | Tone | 自然音 C / D / E / F / G / A / B |
| 调性 | Tonality | Major / Minor / Dissonant / Silent |
| 音堆 | ToneStack | 单位身上的音集合 |
| 基础音 | Base Tone | ToneStack 中不可移除的音 |
| 共鸣 | Resonance | 音组合触发的效果 |
| 触发音 | Trigger Tone | 最后被添加到 stack 的音 |
| 形成路径 | Formation Path | 3 音和弦根据中间过程判定的子类型 |
| 和声护甲 | Harmonic Armor | Perfect Fifth 给敌人的减伤 buff |
| 完全五度 | Perfect Fifth | 五度音程 |
| 大三和弦 | Major Triad | 根音 + 大三度 + 纯五度 |
| 小三和弦 | Minor Triad | 根音 + 小三度 + 纯五度 |
| 不协和 | Dissonance | 相邻自然音 |
| 齐奏 | Unison | 相同音 |
| 行动条 | Action Timeline | 基于 Speed 的回合调度 |
| 调速 | Tempo | 影响行动条速度的机制 |
| 共鸣槽 | Resonance Gauge | 角色独立的大招充能条 |
| 节拍点 | Beat Points | 全队共享的技能消耗资源 |

---

## 附录 A：相关文件索引

| 类型 | 路径 |
|---|---|
| 设计源笔记 | `Project-Sonata/拉赫之梦 - Project Sonata.md` |
| 工程架构 | `Project-Sonata/README.md` |
| 战斗设计（本文档） | `Project-Sonata/Docs/BattleDesign.md` |
| 角色与乐器设计 | `Project-Sonata/Docs/CharacterDesign.md` |
| 剧情设计 | `Project-Sonata/Docs/Narrative/`（[索引](./Narrative/README.md)） |
| 规则层代码 | `Assets/ProjectSonata/Scripts/Battle/Domain/` |
| 表现层代码 | `Assets/ProjectSonata/Scripts/Battle/Runtime/` + `UI/` |
| 测试 | `Assets/ProjectSonata/Tests/EditMode/` |
| 共鸣数值 | `ResonanceCatalog.cs` |
| 默认数值配置 | `Assets/ProjectSonata/ScriptableObjects/Battle/` |
