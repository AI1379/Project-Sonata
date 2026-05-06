# Project Sonata

Project Sonata 是一个 2D 固定视角 / 2.5D 舞台式探索 + 指令式卡牌/回合制 RPG 原型。项目核心不是传统下落式音游，而是把古典乐理中的音符、和声、调性、节奏拆成战斗规则：玩家通过给目标挂音符、补全和弦、手动触发共鸣，在回合决策里获得“正在组织旋律”的感觉。

当前 Unity 工程版本：`Unity 6000.4.5f1`  
当前渲染方向：`URP 2D`

## 设计来源

本 README 根据 Obsidian 笔记 `技术/项目点子/拉赫之梦 - Project Sonata.md` 整理并转化为 Unity 实施架构。

核心设定：

- 世界是一首停在“休止符”上的未完成交响曲，风雪是信号缺失的白噪音，极光是天际漂浮的磁带。
- 主角是留声机/收音机化形的“寻频者”，负责记录、播放并接续断掉的旋律。
- 美术气质偏欧洲中世纪/近代、神秘学、古典乐、雪、雾、月光、极光、舞台布景感。
- 探索推荐使用固定视角 2.5D / 伪 3D 舞台式场景，而不是完整横板动作平台跳跃。

## 第一阶段目标

Phase 1 的目标不是做完整 Demo，而是做两个可验证原型：

1. 战斗原型：验证“挂音符 -> 补音 -> 触发共鸣”的核心循环是否成立。
2. 探索灰盒：验证固定视角 2.5D 舞台式探索在 Unity 里是否好做、好读、好扩展。

第一阶段不要追求美术、音乐系统、剧情演出、复杂养成或正式存档。先把会影响项目成败的交互结构验证掉。

### Phase 1 必做交付物

- 一个可重复进入的测试战斗场景。
- 三名测试角色：小提琴、长笛、鼓。
- 两种普通敌人和一个小 Boss 的占位配置。
- 音符标记上限、覆盖规则、共鸣识别和触发规则。
- 行动条 / Tempo 系统的最小实现。
- 一个固定视角 2.5D 探索灰盒场景，包含移动、调查、进门、上下楼、视差层。
- 一套最小 UI：单位血量、行动条、目标身上的音符、可用技能、当前调性。

### Phase 1 不做内容

- 不做完整剧情章节。
- 不接 FMOD/Wwise，先用 Unity AudioSource / AudioMixer 占位。
- 不做正式 Live2D 管线。
- 不做 Addressables，等资源规模变大再引入。
- 不做复杂装备、抽卡、角色培养、永久进度。
- 不做真正的平台跳跃手感，探索灰盒只验证空间、镜头、路径、交互。

## 推荐工程结构

Unity 的 `Assets` 目录建议从第一天就按功能域拆开。不要把脚本、Prefab、材质、音频都堆在默认目录里。

```text
Assets/
  ProjectSonata/
    Art/
      Characters/
      Environments/
      UI/
      VFX/
    Audio/
      Music/
      SFX/
      Mixer/
    Prefabs/
      Actors/
      Battle/
      Exploration/
      UI/
    Scenes/
      00_Boot.unity
      01_BattlePrototype.unity
      02_ExplorationGreybox.unity
    ScriptableObjects/
      Battle/
        Characters/
        Enemies/
        Skills/
        Resonances/
      Exploration/
      Audio/
    Scripts/
      Core/
      Battle/
      Exploration/
      Presentation/
      UI/
      Infrastructure/
      DebugTools/
    Settings/
    Tests/
      EditMode/
      PlayMode/
```

保留 Unity 默认生成的 `Assets/Scenes` 和 `Assets/Settings` 可以。`Assets/Scenes/SampleScene.unity` 是模板自带示例场景，后续可以作为参考或删除；项目自有内容统一放进 `Assets/ProjectSonata`。这样以后升级模板、导入包、清理资源时，项目自有资产边界更清楚。

## 代码架构

建议采用“纯 C# 规则层 + Unity 表现层”的结构。你有 OOP/C# 经验，这会比把所有逻辑写进 MonoBehaviour 更容易维护。

```text
Battle Domain 纯规则层
  负责：单位数据、音符、技能效果、共鸣识别、行动条、战斗状态推进。
  特点：尽量不引用 UnityEngine，可做 EditMode 单元测试。

Battle Runtime Unity运行层
  负责：把规则层挂到场景对象、驱动回合流程、接收 UI 输入、播放动画和音效。
  特点：可以引用 MonoBehaviour、Prefab、Coroutine。

Presentation 表现层
  负责：血条、音符图标、行动条、伤害数字、选中框、简单特效。
  特点：只订阅战斗事件，不直接决定规则。

Data ScriptableObject 数据层
  负责：角色、敌人、技能、共鸣、调性等可调配置。
  特点：让数值和技能定义从代码里拿出来。
```

### 建议脚本模块

```text
Scripts/Core/
  GameSceneLoader.cs
  ServiceRegistry.cs
  EventBus.cs

Scripts/Battle/Domain/
  BattleState.cs
  BattleUnit.cs
  BattleTeam.cs
  ActionTimeline.cs
  ToneMark.cs
  ToneSet.cs
  Tonality.cs
  SkillDefinition.cs
  SkillResolver.cs
  ResonanceDefinition.cs
  ResonanceDetector.cs
  BattleCommand.cs
  BattleResult.cs

Scripts/Battle/Runtime/
  BattleController.cs
  BattleBootstrapper.cs
  BattleInputController.cs
  BattleUnitView.cs
  BattleEventPlayer.cs

Scripts/Exploration/
  ExplorationController.cs
  PlayerMover2D.cs
  Interactable.cs
  SceneDoor.cs
  GreyboxPath.cs
  CameraRig2_5D.cs
  ParallaxLayer.cs

Scripts/UI/
  BattleHUD.cs
  UnitStatusView.cs
  ToneMarkView.cs
  TimelineView.cs
  SkillButtonView.cs
```

### Assembly Definition

建议早期就加 asmdef，避免后期所有脚本互相引用。

```text
ProjectSonata.Core
ProjectSonata.Battle
ProjectSonata.Exploration
ProjectSonata.Presentation
ProjectSonata.Tests
```

最重要的是让 `Battle/Domain` 尽量独立。比如 `ResonanceDetector` 不应该知道 UI 图标是什么，也不应该依赖场景里的 GameObject。它只接收音符列表，返回识别到的共鸣类型。

## 战斗原型架构

Phase 1 战斗只需要三名角色：

```text
小提琴：铺音符、群体标记。
长笛：精准补音、单体爆发。
鼓：触发共鸣、推进己方行动条、延迟敌方行动。
```

### 基础数据

```csharp
public enum Tone
{
    C, D, E, F, G, A, B
}

public enum Tonality
{
    Major,
    Minor,
    Dissonant,
    Silent
}

public enum ResonanceType
{
    None,
    Unison,
    Dissonance,
    MajorTriad,
    MinorTriad,
    PerfectFifth
}
```

每个单位身上最多保留 3 个音符。超过 3 个时，新音符顶掉最早的音符。

```text
敌人当前音符：[C, E]
长笛追加 G
敌人当前音符：[C, E, G]
形成 C-E-G，可被鼓手技能触发为大三和弦共鸣
```

### 共鸣规则

Phase 1 只做 5 类：

| 组合 | 名称 | 原型效果 |
| --- | --- | --- |
| C-E-G / F-A-C / G-B-D | 大三和弦 | 高额伤害 + 己方攻击提升 |
| A-C-E / D-F-A / E-G-B | 小三和弦 | 中等伤害 + DoT + 降低敌方 Tempo |
| C-D / E-F 等相邻音 | 不协和音 | 真实伤害 + 当前角色受到反噬 |
| C-C 等相同音 | 齐奏 | 强化同音技能效果 |
| C-G / D-A 等五度 | 完全五度 | 护盾 / 净化 / 稳定效果 |

实现上先硬编码表驱动，不急着做复杂乐理推导。

```text
ResonanceDetector
  输入：目标身上的 Tone 列表
  输出：ResonanceMatch { type, tones, canManualTrigger }
```

### 行动条 / Tempo

不要先做“我方全员行动 -> 敌方全员行动”的传统回合。这个项目的音乐主题需要节奏感，所以建议第一版就用行动条。

最小规则：

```text
每个单位有 speed 和 actionGauge。
每次 tick：actionGauge += speed * delta。
actionGauge >= 100 时进入 Ready 状态。
玩家或 AI 执行动作后，actionGauge -= 100。
鼓类技能可以让己方 actionGauge +10 或敌方 actionGauge -10。
```

Unity 实现上，第一版可以不做真实时间流逝，而是做“离散推进”：

```text
没有单位 Ready 时，系统自动推进到下一个单位 Ready。
有玩家单位 Ready 时，暂停推进，等待玩家选择技能。
敌人 Ready 时，AI 自动执行。
```

这样既保留行动条策略，又避免一开始处理复杂暂停、动画时长、输入抢占。

## 探索灰盒是什么

灰盒，也叫 blockout / greybox，是在没有正式美术资源前，用简单几何体、色块、占位材质把场景结构、镜头、动线、交互、尺寸关系先搭出来。

对 Project Sonata 来说，灰盒的目标不是“看起来漂亮”，而是回答这些问题：

- 固定视角 2.5D 是否能表达花园、楼梯、塔、倒置空间。
- 玩家角色在场景里移动时，遮挡和层次是否清楚。
- 镜头是否能稳定展示路径、门、调查点、敌人。
- 场景切换、上楼、下楼、进门是否自然。
- 这种探索形式会不会让玩家误以为这是平台动作游戏。

## Phase 1 灰盒做法

### 1. 新建灰盒场景

创建场景：

```text
Assets/ProjectSonata/Scenes/02_ExplorationGreybox.unity
```

场景内容先做“雾中花园 + 小屋入口 + 一段上楼路径”的最小组合。

```text
左侧入口
  -> 花园小路
  -> 泉水调查点
  -> 小屋门
  -> 后景楼梯
  -> 高处平台
```

这个结构能一次验证移动、调查、进门、上下楼、前中后景遮挡。

### 2. 使用 2D 项目里的 3D 空间

虽然工程是 2D URP，但 Unity 2D 并不禁止你使用 3D 坐标。固定视角 2.5D 推荐这样做：

```text
X：左右
Y：上下
Z：前后层次
```

相机用 Orthographic。场景对象可以是 SpriteRenderer，也可以先用 Cube / Quad 作为占位。

建议层次：

```text
Z =  5  远景：天空、远处树影、小屋背景
Z =  0  中景：玩家可走路径、门、泉水
Z = -2  前景：树枝、栏杆、遮挡物
```

Unity 里相机默认看向 Z 轴方向，所以需要统一约定。更稳的做法是让所有 2D Sprite 放在 XY 平面，用 Sorting Layer 管视觉前后；Z 只用于视差和空间参考，不依赖 Z 做主要排序。

### 3. 先画可走区域

不要一开始就做 Tilemap。灰盒阶段建议用简单 PolygonCollider2D 或 BoxCollider2D 表示可走区域。

```text
WalkableArea
  BoxCollider2D / PolygonCollider2D

BlockedArea
  BoxCollider2D

Player
  Rigidbody2D
  Collider2D
  PlayerMover2D
```

移动方式：

```text
WASD / 左摇杆移动
Rigidbody2D.MovePosition 推进
不要直接 transform.position += ...
```

原因是 Unity 的物理和碰撞更新有自己的节奏。直接改 Transform 很容易穿墙、抖动，或者和触发器行为不一致。

### 4. 做固定镜头，而不是跟随镜头

Project Sonata 的探索更像舞台布景，不像动作游戏。第一阶段建议用固定镜头或按区域切换镜头。

```text
CameraZone_Garden
CameraZone_HouseDoor
CameraZone_UpperStair
```

玩家进入不同区域时，镜头轻微切换或移动到预设位置。不要让相机一直紧跟玩家，否则会削弱“构图”和“舞台感”。

可以先不接 Cinemachine，自己写一个简单 `CameraRig2_5D`：

```text
目标位置 targetPosition
当前相机位置 currentPosition
每帧 SmoothDamp 到 targetPosition
```

### 5. 用颜色表达功能

灰盒不是全灰更好。建议固定颜色语义：

```text
白色 / 浅灰：可走地面
深灰：墙体和不可走区域
蓝色：门和场景切换
黄色：调查点
红色：敌人触发点
绿色：镜头区域
半透明黑：前景遮挡
```

这样测试时能快速看出“我为什么走不过去”“这里是不是能调查”“这个区域是否会切镜头”。

### 6. 实现最小交互

用一个通用接口：

```csharp
public interface IInteractable
{
    string Prompt { get; }
    void Interact(InteractionContext context);
}
```

Phase 1 只需要三类交互：

```text
InspectPoint：显示一行调试文本。
SceneDoor：切换到另一个场景或移动到另一个入口点。
BattleTrigger：进入测试战斗场景。
```

先把交互文本显示在简单 UI Text 上即可，不需要正式对话系统。

### 7. 做一条“上楼”路径

不要做跳跃。上楼可以只是一个斜向可走区域：

```text
楼梯底部在左下，顶部在右上。
玩家沿斜坡走，视觉上像上楼。
到顶部时触发 CameraZone_UpperStair。
```

如果要更有 2.5D 感，可以让玩家在楼梯区域内移动时稍微改变 Sorting Order，让角色从栏杆后面走到栏杆前面。但第一版只要验证“上楼这件事是否读得懂”。

### 8. 做视差层

给前景和远景挂 `ParallaxLayer`：

```text
远景移动倍率：0.15
中景移动倍率：0.4
前景移动倍率：0.8
```

固定镜头下视差不需要很复杂。镜头从花园移动到小屋门时，远景慢一点、前景快一点，就能初步验证舞台层次。

### 9. 灰盒验收标准

完成后用这些问题验收：

- 不看说明，能不能看出哪里能走、哪里不能走。
- 玩家能否在 30 秒内找到调查点、门和楼梯。
- 镜头切换是否让人知道自己到了新区域。
- 前景遮挡是否增强层次，而不是挡住玩家。
- 场景是否像“舞台式探索”，而不是横板跳跃关卡。
- 从探索进入战斗、再回到探索，流程是否顺。

## Unity 新手重点说明

### GameObject、Component、Prefab

Unity 不是传统“new 一个对象就进入游戏世界”的模型。

```text
GameObject：场景里的实体容器。
Component：挂在 GameObject 上的功能。
Prefab：可复用的 GameObject 模板。
ScriptableObject：可保存到磁盘的数据资产。
```

例如一个敌人通常不是一个 C# 类实例，而是：

```text
EnemyPrefab
  GameObject
    SpriteRenderer
    Collider2D
    BattleUnitView
    EnemyIntentView
```

真正的战斗数据可以在纯 C# `BattleUnit` 里，场景里的 `BattleUnitView` 只负责显示。

### MonoBehaviour 不适合承载全部业务逻辑

初学 Unity 很容易把所有代码都写在 MonoBehaviour 的 `Update()` 里。这个项目应该避免这种写法。

更推荐：

```text
MonoBehaviour：接输入、连场景、播表现。
纯 C# 类：算规则、管状态、做测试。
ScriptableObject：放可调数据。
```

例如：

```text
SkillButtonView 点击按钮
  -> BattleInputController 创建 BattleCommand
  -> BattleController 交给 SkillResolver
  -> SkillResolver 修改 BattleState
  -> BattleController 发出 BattleResult
  -> UI 和动画根据结果刷新
```

### Update、FixedUpdate、Coroutine

常用规则：

```text
Update：读输入、更新 UI、非物理逻辑。
FixedUpdate：物理移动、Rigidbody2D。
Coroutine：等待动画、延迟演出、分步骤流程。
```

战斗流程可以用 Coroutine 做演出串联，但核心结算不要依赖 Coroutine。规则先同步算完，再把结果交给表现层逐步播放。

### Scene、Prefab、ScriptableObject 的职责

```text
Scene：摆放当前关卡里有哪些对象。
Prefab：定义可复用对象长什么样、有哪些组件。
ScriptableObject：定义角色、技能、敌人、共鸣、调性的配置。
Code：定义规则如何运行。
```

不要把重要数值只写在 Scene 里的某个组件字段上，否则以后复制场景、做平衡、查问题都会很痛苦。

## 常踩坑

- 把业务规则写进 UI：例如按钮点击后直接扣血。应让按钮只发命令，规则层负责结算。
- 滥用 `FindObjectOfType` / `GameObject.Find`：小项目能跑，大一点会混乱且慢。用序列化字段、Bootstrapper 或简单服务注册。
- 直接改 `transform.position` 做 2D 物理移动：容易穿墙。玩家移动优先用 `Rigidbody2D.MovePosition`。
- 忘记 Prefab 修改和 Scene 实例修改的区别：改了场景里的实例，不一定改到 Prefab。
- 在 `Update()` 里反复创建对象：伤害数字、音符特效以后需要对象池，原型阶段也要留意识。
- Sorting Layer 和 Z 深度混用：2D 项目里视觉排序优先统一用 Sorting Layer / Order in Layer。
- UI 全放一个 Canvas：动态元素会导致 Canvas 频繁重建。后期应拆静态 Canvas 和动态 Canvas。
- 过早引入大型框架：Addressables、FMOD、复杂状态机、依赖注入都可以等原型成立后再接。
- 过早追求正式美术：灰盒阶段资源越精致，越容易舍不得改结构。
- 没有 EditMode 测试：共鸣识别、行动条、技能结算都很适合纯 C# 单元测试。

## 推荐开发顺序

1. 建目录和 asmdef。
2. 做 `Tone`、`ToneSet`、`ResonanceDetector` 的纯 C# 实现和测试。
3. 做 `BattleState`、`BattleUnit`、`ActionTimeline`。
4. 做最小战斗 UI，把三名角色和一个敌人跑起来。
5. 接入技能配置 ScriptableObject。
6. 实现鼓手手动触发共鸣。
7. 做第二种敌人和小 Boss。
8. 新建探索灰盒场景，实现移动、碰撞、调查、门、战斗入口。
9. 加固定镜头区域和视差层。
10. 做一次从探索进战斗再返回探索的完整闭环。

## Phase 1 验收标准

战斗侧：

- 玩家能在 3 分钟内理解“挂音符 -> 补音 -> 触发共鸣”。
- 玩家会为了凑和弦改变技能选择，而不是永远点伤害最高的按钮。
- 不协和音既有收益，也有清楚的反噬风险。
- 鼓手的 Tempo 和触发共鸣职责不可替代。

探索侧：

- 场景不靠正式美术也能读懂空间层次。
- 固定镜头不会遮挡玩家和关键交互点。
- 上楼、进门、调查、遇敌四种行为能跑通。
- 2.5D 舞台感成立，但不会让人期待复杂动作平台玩法。

工程侧：

- 战斗核心规则可以在 EditMode 下测试。
- UI 和表现层不直接写战斗结算。
- 角色、技能、敌人至少一部分数据已经迁移到 ScriptableObject。
- 场景对象命名清楚，Prefab 和 Scene 实例边界清楚。

## 后续路线

Phase 2 再做 10-15 分钟 Demo：

- 场景：雪国列车 -> 雾中花园。
- 队伍：小提琴、长笛、鼓。
- 敌人：音乐妖精、失谐乐器。
- Boss：钟表/铃声主题敌人。
- 音乐：先用 Unity AudioMixer 做简单分层。
- 剧情：完成一次“世界停在休止符上”的开场表达。

Phase 3 再做垂直切片：

- 完成一个完整章节。
- 加入钢琴角色或钢琴 Boss，用于验证调性切换。
- 完成 1-2 条角色支线。
- 建立动态音乐规则：战斗层、调性层、Boss 阶段层。
