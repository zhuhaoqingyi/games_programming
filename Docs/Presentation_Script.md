# Space Constructor (太空建造师) — Presentation Script

---

## Slide 1: Title / 标题

**EN:** Space Constructor — A Space Survival & Base-Building Game  
**CN:** 太空建造师 — 一款太空生存建造游戏

**EN:** Presented by: [Your Name] | Student ID: [Your ID]  
**CN:** 演示者：[姓名] | 学号：[学号]

---

## Slide 2: Game Overview / 游戏概述 (30s)

**EN:**
- **Genre:** 2D Space Survival / Base-Building / Resource Management
- **Platform:** Unity (C#), Windows
- **Background Story:** You were the commander of a powerful interstellar spaceship. A sudden meteor storm destroyed your ship completely. Only an emergency shelter and a few crew members survived. Now you must start from scratch — gather resources, build facilities, and rebuild a self-sufficient spaceship.

**CN:**
- **类型：** 2D 太空生存 / 基地建造 / 资源管理
- **平台：** Unity (C#)，Windows
- **背景故事：** 你曾是一艘强大星际飞船的指挥官。一场突如其来的陨石风暴摧毁了你的飞船，只剩下一个紧急避难所和少数幸存船员。你必须从零开始，收集资源，建造设施，重建一艘自给自足的强大飞船。

---

## Slide 3: Core Gameplay Loop / 核心玩法循环 (40s)

**EN:**
1. **Explore** — Navigate your ship through space to find floating space ores
2. **Collect** — Mining Platforms automatically harvest nearby ores
3. **Produce** — Refine ores into metal, assemble parts, craft advanced materials
4. **Build** — Expand your spaceship with new boards, production facilities, power plants, and thrusters
5. **Survive** — Defend against drifting ore collisions that damage your buildings

**CN:**
1. **探索** — 驾驶飞船在太空中寻找漂浮的太空矿石
2. **收集** — 采矿机自动采集附近的矿石
3. **生产** — 将矿石精炼成金属，组装零件，制造高级材料
4. **建造** — 用太空板、生产设施、发电厂和推进器扩建飞船
5. **生存** — 抵御漂浮矿石碰撞对建筑造成的伤害

---

## Slide 4: Key Features Demonstration / 核心功能演示 (2 min)

### 4.1 Build Mode / 建造模式
**EN:** Press B to open the building UI. Select a building from the category panel. Move the mouse to preview placement (green = valid, red = invalid). Left-click to place, press R to rotate. Buildings require resources to construct.

**CN:** 按 B 键打开建筑 UI。从分类面板选择建筑。移动鼠标预览放置位置（绿色=有效，红色=无效）。左键放置，按 R 旋转。建筑需要消耗资源。

### 4.2 Flight Mode / 飞行模式
**EN:** Press Tab to toggle flight mode. W/S to accelerate/decelerate, A/D to steer left/right. The ship has drag — it gradually slows when keys are released. Thrusters produce exhaust effects in the direction they face. The background wraps every 5000 units to prevent floating-point issues.

**CN:** 按 Tab 键切换飞行模式。W/S 加速/减速，A/D 左右转向。飞船有阻力——松开按键后逐渐减速。推进器在朝向方向产生喷射效果。背景每 5000 单位回退防止浮点精度问题。

### 4.3 Delete Mode / 删除模式
**EN:** Click the "Delete Mode" button in the building UI, then click on any placed building to demolish it. Demolishing refunds 50% of the building's construction cost. Core buildings (Emergency Shelter) cannot be deleted.

**CN:** 在建筑 UI 中点击"删除模式"按钮，然后点击已放置的建筑进行拆除。拆除返还 50% 的建筑成本。核心建筑（紧急避难所）不可删除。

### 4.4 Production Chain / 生产链
**EN:** Four resource types with a progressive production chain:
- **Space Ore** → mined automatically by Mining Platforms
- **Metal Material** → refined from ore in Furnace Refinery (1 ore → 1 metal, 5s)
- **Basic Parts** → assembled in Part Assembly (4 metal → 1 part, 8s)
- **Advanced Parts** → crafted in Advanced Factory (4 basic parts → 1 advanced, 12s)

**CN:** 四种资源类型，逐级生产链：
- **太空矿石** → 采矿机自动采集
- **金属材料** → 熔炉精炼（1矿石→1金属，5秒）
- **初级零件** → 零件组装（4金属→1零件，8秒）
- **高级零件** → 高级工厂（4初级零件→1高级，12秒）

### 4.5 Power System / 电力系统
**EN:** Buildings require power to operate. Two power sources:
- **Solar Array** — 10 power output, costs 20 metal
- **Nuclear Reactor** — 50 power output, no resource consumption, costs 50 metal + 30 basic parts + 5 advanced parts

When power is insufficient, buildings show a red warning icon and stop working.

**CN:** 建筑需要电力运行。两种电源：
- **太阳能板** — 10 电力输出，花费 20 金属
- **核能反应堆** — 50 电力输出，无需消耗，花费 50 金属 + 30 初级零件 + 5 高级零件

电力不足时，建筑显示红色警告图标并停止工作。

### 4.6 Save & Load / 存档系统
**EN:** Press Ctrl+S to save the game. All buildings, resources, ship position, and game time are serialized to JSON. From the main menu, click "Load" to restore a saved game. Buildings are saved in correct order (boards first, then other buildings).

**CN:** 按 Ctrl+S 保存游戏。所有建筑、资源、飞船位置和游戏时间序列化为 JSON。在主菜单点击"Load"恢复存档。建筑按正确顺序保存（先板后建筑）。

---

## Slide 5: Technical Architecture / 技术架构详解 (2 min)

### 5.1 Singleton Pattern (Manager Layer) / 单例模式（管理层）

**EN:** The project uses **11 Singletons** as global managers. Each manager owns a distinct domain, avoiding a single "God Manager" anti-pattern:

| Singleton | Responsibility |
|-----------|---------------|
| `GameManager` | Game lifecycle, resource initialization, save/load orchestration |
| `GridManager` | Grid cell management, building placement, validation, cell queries |
| `GridRenderer` | Visual grid rendering and background display |
| `BoardManager` | Board-specific placement and management |
| `BuildingPlacer` (non-singleton) | Mouse interaction, preview, placement events |
| `PowerManager` | Power production/consumption registry and balance calculation |
| `AudioManager` | BGM + SFX with configurable duration/loop, cross-scene persistence |
| `SaveSystem` | JSON serialization/deserialization, backup mechanism |
| `ThrustManager` | Ship movement, coordinate wrapping every 5000 units |
| `BuildingUI` | Building category panels, selection, delete mode toggle |
| `ResourceDisplayUI` | Real-time resource display updates |
| `MainMenuManager` | Main menu scene: New Game, Load, Tutorial |

All singletons follow the same pattern: `Instance` static property, `Awake()` with null-check and `Destroy(gameObject)` for duplicates. `AudioManager` additionally uses `DontDestroyOnLoad()` for cross-scene persistence.

**CN:** 项目使用 **11 个单例** 作为全局管理器。每个管理器负责独立的领域，避免单一"上帝管理器"反模式。所有单例遵循统一模式：`Instance` 静态属性 + `Awake()` 空检查 + 重复时 `Destroy(gameObject)`。`AudioManager` 额外使用 `DontDestroyOnLoad()` 实现跨场景持久化。

---

### 5.2 Delegate/Event System (Decoupled Communication) / 委托事件系统（解耦通信）

**EN:** Custom delegates + C# events enable loose coupling between modules:

```csharp
// BuildingPlacer.cs — fires events when buildings are placed/removed
public delegate void BuildingPlaced(GridPosition position, BuildingType type);
public event BuildingPlaced OnBuildingPlaced;

public delegate void BuildingRemoved(GridPosition position, BuildingType type);
public event BuildingRemoved OnBuildingRemoved;

public delegate void DeleteModeChanged(bool isDeleteMode);
public event DeleteModeChanged OnDeleteModeChanged;
```

**Why delegates instead of UnityEvents?**  
- Type-safe parameter passing with custom signatures  
- Compile-time checking (UnityEvent parameters are serialized, error-prone)  
- Better performance (no reflection-based serialization overhead)  
- Cleaner code — no Inspector wiring needed for code-to-code communication

**Usage flow:** `BuildingUI` selects a building → calls `BuildingPlacer.SelectBuilding()` → `BuildingPlacer` handles mouse input → fires `OnBuildingPlaced` → subscribed systems (GameManager, AudioManager, UI) react accordingly.

**CN:** 自定义委托 + C# 事件实现模块间松耦合。**为什么用委托而非 UnityEvent？** 类型安全的参数传递、编译期检查、更好的性能（无反射序列化开销）、代码间通信无需 Inspector 连线。

---

### 5.3 Namespace Modularity (Separation of Concerns) / 命名空间模块化（关注点分离）

**EN:** Codebase organized into **7 namespaces** following domain-driven design:

| Namespace | Purpose | Key Files |
|-----------|---------|-----------|
| `GameCore` | Data definitions, enums, configs | DataConfig.cs, Enums.cs, BuildingData.cs, ResourceData.cs, RecipeData.cs |
| `GridSystem` | Grid logic, building placement, cells | GridManager.cs, GridCell.cs, BuildingComponent.cs, BuildingPlacer.cs, PlacedBuilding.cs |
| `PowerSystem` | Power grid, producers, consumers | PowerManager.cs, PowerProducer.cs, PowerConsumer.cs |
| `ProductionSystem` | Factories, mining, health | BuildingBase.cs, Productor.cs, ProductionManager.cs, MiningCollector.cs |
| `LogisticsSystem` | Storage, conveyors, sorting | StorageManager.cs, StorageComponent.cs, ConveyorBelt.cs, ConveyorSystem.cs |
| `UI` | All UI panels and HUD | BuildingUI.cs, ResourceDisplayUI.cs, BuildingCategoryPanel.cs, TutorialManager.cs |
| `GameResources` | Resource spawning, ore behavior | ResourceSpawner.cs, SpaceOre.cs |

**CN:** 代码按领域驱动设计组织为 **7 个命名空间**，每个命名空间职责明确，互不交叉。

---

### 5.4 Component-Based Architecture (Unity ECS-Lite) / 组件化架构

**EN:** Buildings use a **composition-over-inheritance** approach via Unity's `MonoBehaviour` components:

```
GameObject (EmergencyShelter)
  ├── BuildingComponent      ← grid position, type, status, power data
  ├── BuildingBase           ← health, collider, damage, destruction
  ├── ContainerComponent     ← storage capacity registration
  ├── PowerConsumer          ← registers with PowerManager
  └── (optional) Productor   ← production recipes, crafting logic
```

**Key design decisions:**
- `BuildingComponent` — minimum required component, holds building identity and grid position
- `BuildingBase` — handles health, collision detection, destruction (inherits MonoBehaviour directly, NOT BuildingComponent — composition, not inheritance)
- `ContainerComponent` / `PowerConsumer` / `Productor` — optional add-on components that register with their respective managers in `Awake()`/`Start()`
- Components auto-add missing dependencies (`SetupRigidbody()`, `SetupCollider()` use `AddComponent` if not present)

**CN:** 建筑使用 **组合优于继承** 的方式，通过 Unity `MonoBehaviour` 组件实现：
- `BuildingComponent` — 最小必需组件，持有建筑标识和网格位置
- `BuildingBase` — 处理血量、碰撞检测、销毁（直接继承 MonoBehaviour，使用组合而非继承）
- `ContainerComponent` / `PowerConsumer` / `Productor` — 可选附加组件，在 `Awake()`/`Start()` 中向各自管理器注册
- 组件自动添加缺失依赖（`SetupRigidbody()`、`SetupCollider()` 在缺失时自动 AddComponent）

---

### 5.5 Data-Driven Design (DataConfig) / 数据驱动设计

**EN:** All game data is centralized in `DataConfig` static class with `[Static Constructor]` pattern:

```csharp
static DataConfig()
{
    InitializeResources();   // 4 resource type definitions
    InitializeBoards();      // Board → Building mapping
    InitializeBuildings();   // 10 building definitions with costs, size, power, etc.
    InitializeRecipes();     // 3 production recipes with ingredients and time
}
```

**Benefits:**
- Adding a new building = one `BuildingDefinition` constructor call, no code changes elsewhere
- Costs, production times, power values all configurable in one place
- Recipe system supports arbitrary ingredient→output chains
- Directional prefab paths (`directionalPrefabPaths[BuildDirection.East]`) enable rotation-specific visuals

**CN:** 所有游戏数据集中在 `DataConfig` 静态类的静态构造函数中。**好处：** 添加新建筑只需一行构造函数调用；成本、生产时间、电力值集中配置；配方系统支持任意原料→产出链；方向预制体路径支持旋转特定视觉。

---

### 5.6 Awake/Start Initialization Ordering / 初始化顺序控制

**EN:** Critical for correctness — components must register with managers before managers use them:

| Phase | What Happens |
|-------|-------------|
| `DataConfig` static ctor | Definitions loaded (before any Awake) |
| `Awake()` | Singletons registered, `InitializeGame()` creates GameTime |
| `Start()` | Check `PendingLoad` flag → load or new game → `InitializeStartingBuildings()` → `RegisterInitialContainers()` → `InitializeInitialResources()` |

**Ordering bugs solved:**
- Resources must be initialized **before** UI `Start()` so `CanAfford()` works
- Containers must be registered **before** `InitializeInitialResources()` so capacity exists
- Save loading uses `validate: false` to bypass cell checks in cleared grid

**CN:** 初始化顺序对正确性至关重要：
- 资源必须在 UI `Start()` **之前** 初始化，确保 `CanAfford()` 有效
- 容器必须在 `InitializeInitialResources()` **之前** 注册，确保容量存在
- 存档加载使用 `validate: false` 跳过已清空网格的检查

---

### 5.7 Grid System (Sparse Dictionary Grid) / 网格系统（稀疏字典网格）

**EN:**
- **Data structure:** `Dictionary<GridPosition, GridCell>` — O(1) lookup, unlimited expansion, no fixed-size array memory waste
- **GridCell struct:** packed value type with `HasBoard`, `HasBuilding`, `BoardType`, `PlacedBuilding` reference, `IsFunctionalArea`
- **Two-layer system:** Boards (`HasBoard`/`BoardType`) and Buildings (`HasBuilding`/`PlacedBuilding`) coexist independently on the same cell
- **MarkCellsForBuilding:** iterates `displayWidth × displayHeight` area, setting board or building flags based on `isBoard` property
- **Functional areas:** exhaust zones, mining ranges stored as separate cell flags, checked during placement validation

**CN:**
- **数据结构：** `Dictionary<GridPosition, GridCell>` — O(1) 查找，无限扩展，无固定大小数组内存浪费
- **GridCell 结构体：** 紧凑值类型，包含 `HasBoard`、`HasBuilding`、`BoardType`、`PlacedBuilding` 引用、`IsFunctionalArea`
- **双层系统：** 太空板（`HasBoard`/`BoardType`）和建筑（`HasBuilding`/`PlacedBuilding`）在同一格子上独立共存
- **MarkCellsForBuilding：** 遍历 `displayWidth × displayHeight` 区域，根据 `isBoard` 属性设置板或建筑标志
- **功能区：** 排气区、采矿范围存储为独立格子标志，放置验证时检查

---

### 5.8 Save System (JSON Serialization + Ordering) / 存档系统（JSON 序列化 + 排序）

**EN:**
- **Format:** JSON via `[Serializable]` DTOs (`SaveData`, `BuildingEntry`, `ResourceEntry`)
- **Path:** `Application.persistentDataPath/savegame.json` with `_backup` rotation
- **Saving:** Boards collected from `GetAllBoardCells()` (grid cells) → non-board buildings from `GetAllPlacedBuildings()` (dictionary) → avoids dictionary-overwrite bug
- **Loading:** `ClearAllBuildings()` → sort buildings (boards first) → `PlaceBuildingWithDirection(validate: false)` → register containers → load resources
- **Cross-scene flag:** `GameManager.PendingLoad` static bool set by `MainMenuManager.OnLoadClicked()` using `File.Exists()` check

**CN:**
- **格式：** JSON 通过 `[Serializable]` DTO（`SaveData`、`BuildingEntry`、`ResourceEntry`）
- **路径：** `Application.persistentDataPath/savegame.json`，带 `_backup` 轮转
- **保存：** 板类从 `GetAllBoardCells()`（网格格子）收集 → 非板类从 `GetAllPlacedBuildings()`（字典）收集 → 避免字典覆盖 bug
- **加载：** `ClearAllBuildings()` → 排序建筑（板类优先）→ `PlaceBuildingWithDirection(validate: false)` → 注册容器 → 加载资源
- **跨场景标志：** `GameManager.PendingLoad` 静态布尔值，由 `MainMenuManager.OnLoadClicked()` 通过 `File.Exists()` 设置

---

### 5.9 Prefab Loading & Resource Management / 预制体加载与资源管理

**EN:**
- **Resources.Load<T>()** for dynamic prefab loading — buildings, icons, effects loaded at runtime
- **Directional prefabs:** `directionalPrefabPaths` dictionary enables different sprites for each rotation (e.g., `MiningPlatform1-4`, `Thruster1-4`)
- **SwapBuildingPrefab:** handles runtime sprite swapping when rotating buildings with direction-specific prefabs
- **Object pooling consideration:** Currently using `Instantiate`/`Destroy`; pooling could be added for performance at scale

**CN:**
- **Resources.Load<T>()** 动态加载预制体 — 建筑、图标、特效在运行时加载
- **方向预制体：** `directionalPrefabPaths` 字典为每个旋转方向使用不同精灵（如 `MiningPlatform1-4`、`Thruster1-4`）
- **SwapBuildingPrefab：** 旋转具有方向特定预制体的建筑时，处理运行时精灵交换
- **对象池考虑：** 当前使用 `Instantiate`/`Destroy`；可扩展对象池提升大规模场景性能

---

### 5.10 Architecture Summary Diagram / 架构总览图

```
┌─────────────────────────────────────────────────────────┐
│                     GameManager                          │
│  (Lifecycle: Init → Start → Save/Load orchestration)     │
└────────┬──────────┬──────────┬──────────┬───────────────┘
         │          │          │          │
    ┌────▼───┐ ┌───▼────┐ ┌───▼────┐ ┌───▼────┐
    │ Grid   │ │ Power  │ │Product │ │Logistic│
    │System  │ │System  │ │System  │ │System  │
    │Manager │ │Manager │ │Manager │ │Manager │
    └────┬───┘ └───┬────┘ └───┬────┘ └───┬────┘
         │         │         │         │
    ┌────▼─────────▼─────────▼─────────▼────┐
    │         DataConfig (Static)            │
    │  Resources | Buildings | Recipes       │
    └────────────────┬───────────────────────┘
                     │
    ┌────────────────▼───────────────────────┐
    │              UI Layer                   │
    │  BuildingUI | ResourceDisplayUI         │
    │  MainMenuManager | TutorialManager      │
    └────────────────────────────────────────┘

    Cross-Cutting:
    ┌──────────┐  ┌──────────┐  ┌──────────┐
    │  Audio   │  │  Save    │  │  Thrust  │
    │ Manager  │  │ System   │  │ Manager  │
    └──────────┘  └──────────┘  └──────────┘
```

---

## Slide 5.5: Object Lifecycle Management / 对象生命周期管理 (1 min)

### 5.5.1 Unity Message Execution Order / Unity 消息执行顺序

**EN:** This project carefully leverages Unity's deterministic lifecycle ordering:

```
Scene Load
  │
  ├─ DataConfig static constructor  ← Definitions loaded first (before any GameObject)
  │
  ├─ Awake() phase
  │    ├─ Singleton.Instance = this  ← All managers register themselves
  │    ├─ BuildingBase.SetupRigidbody() / SetupCollider()  ← Auto-add missing components
  │    └─ PowerConsumer: CreatePowerShortageMarker()  ← Create child GameObjects
  │
  ├─ OnEnable() phase
  │
  └─ Start() phase  ← Dependencies guaranteed available
       ├─ GameManager: Check PendingLoad → new game or load
       ├─ BuildingUI: HideUI(), SetCategory(0)
       ├─ PowerConsumer: RegisterWithManager()  ← PowerManager.Instance is ready
       ├─ Productor: UpdateVisual()
       └─ BuildingIconButton.UpdateAffordability()
```

**Why this matters:** If `PowerConsumer.RegisterWithManager()` ran in `Awake()` but `PowerManager.Instance` was set in another `Awake()`, the order is non-deterministic. Moving registration to `Start()` guarantees all singletons are initialized. The project also has a **defensive retry** in `PowerConsumer.Start()`: if `PowerManager.Instance` is still null (init order edge case), it retries registration there.

**CN:** 项目精确利用 Unity 确定性的生命周期顺序：
- `Awake()` 设置自身状态（Rigidbody、Collider、子 GameObject）
- `Start()` 依赖外部服务（PowerManager 注册、UI 更新）
- 防御性重试：`PowerConsumer.Start()` 中如果 PowerManager 仍为 null 则重试注册

---

### 5.5.2 Building Full Lifecycle / 建筑完整生命周期

**EN:**

```
┌────────────┐    ┌──────────┐    ┌───────────┐    ┌──────────┐    ┌──────────┐
│ Instantiate │ →  │  Awake   │ →  │   Start   │ →  │  Update  │ →  │ OnDestroy│
│ (GridMgr)   │    │ (Setup)  │    │ (Register)│    │ (Work)   │    │ (Cleanup)│
└────────────┘    └──────────┘    └───────────┘    └──────────┘    └──────────┘

Awake():   SetupRigidbody() → auto-add if missing, kinematic, 0 gravity
           SetupCollider()  → auto-add BoxCollider2D if missing
                              size = buildingDef.width × height × cellSize
           currentHealth = maxHealth

Start():   [ContainerComponent] → register with GameManager (capacity)
           [PowerConsumer]      → register with PowerManager
           [Productor]          → UpdateVisual()

Update():  [Productor] → state machine: check power → check input → consume → produce → output
           [PowerConsumer] → UpdatePowerShortageMarker() (show/hide red icon)

OnDestroy(): [PowerConsumer/PowerProducer] → UnregisterFromManager()
             [ContainerComponent]          → remove capacity from GameManager
             [SorterComponent/ConveyorBelt] → virtual OnDestroy() for subclasses
```

**CN:** 建筑从 `Instantiate` 到 `OnDestroy` 的完整四阶段生命周期：
- **创建** — `GridManager.Instantiate()` 加载预制体，初始化 BuildingComponent
- **注册** — `Start()` 中向 GameManager、PowerManager 注册容器/电力
- **运行** — `Update()` 中 Productor 状态机自动生产
- **清理** — `OnDestroy()` 中注销所有管理器注册，从网格移除，强制容量限制

---

### 5.5.3 Resource (Ore) Lifecycle / 资源（矿石）生命周期

**EN:**

```
┌────────────┐    ┌──────────────┐    ┌─────────────────┐    ┌────────────┐
│  Spawn     │ →  │  Active      │ →  │  Collision/      │ →  │  Destroy   │
│ Initialize │    │  (Update)    │    │  Boundary/Collect│    │            │
└────────────┘    └──────────────┘    └─────────────────┘    └────────────┘

Spawn:      Instantiate(prefab, randomPos, Quaternion.identity)
            Initialize(randomDirection) → moveDirection + deviation, randomSpeed
            protectedTime = 3s (ignores boundary check initially)

Active:     Update() every frame:
              transform.position += moveDirection * speed * deltaTime
              Rotation + bobbing (sin wave on Z axis)
              CheckBoundary() → if out of camera bounds + protectedTime expired → Destroy

Collision:  OnTriggerEnter2D(Collider2D other)
              if other is BuildingBase:
                building.TakeDamage(damage)
                Instantiate(explosionPrefab) → Destroy(explosion, 1s)
                PlayOreCollisionSound()
                Destroy(gameObject) ← ore is consumed after collision

Mining:     MiningCollector.OnTriggerEnter2D()
              if other is SpaceOre:
                isCollected = true
                AddResource to GameManager
                PlayMiningSound()
                Destroy(gameObject)
```

**Key design:**
- Floating-point avoidance: ores are parented to the world container and moved via `transform.position`, synchronized with the ship's world offset in `ThrustManager.LateUpdate()`
- Ores exist outside the grid system — they are free-floating world-space entities
- Clean destruction in 4 scenarios: boundary exit, ship collision, mining collection, manual despawn

**CN:** 矿石的四阶段生命周期：
- **生成** — 在屏幕外随机位置 Instantiate，Initialize 设置随机方向和速度，3 秒保护期
- **活跃** — Update 中移动 + 旋转 + 上下浮动，超出边界后自动销毁
- **碰撞** — 碰到建筑 → 造成伤害 + 爆炸特效 1 秒 + 音效 + 自身销毁
- **采集** — 采矿机触发 → isCollected 标记 → 添加资源 → 销毁

---

### 5.5.4 Scene Lifecycle (New Game vs Load) / 场景生命周期（新游戏 vs 加载）

**EN:** Cross-scene state transfer without a persistent GameObject:

```
MainMenu Scene                         GameScene
─────────────                          ─────────
New Game clicked
  → GameManager.PendingLoad = false
  → SceneManager.LoadScene(GameScene)
                                        Start()
                                          PendingLoad == false?
                                            → InitializeStartingBuildings()
                                            → RegisterInitialContainers()
                                            → InitializeInitialResources()
                                            → SpawnInitialOres()
                                            → InitializeGame()

Load clicked
  → File.Exists(savegame.json)?
  → GameManager.PendingLoad = true
  → SceneManager.LoadScene(GameScene)
                                        Start()
                                          PendingLoad == true?
                                            → SaveSystem.LoadGame()
                                            → ClearAllBuildings()
                                            → Load buildings (sorted: boards first)
                                            → Register containers from buildings
                                            → Load resources
                                            → Restore ship position
```

**Why a static flag instead of DontDestroyOnLoad?** The `SaveSystem` MonoBehaviour only exists in GameScene — it would be meaningless in MainMenu. A static bool on `GameManager` is lightweight, doesn't require a persistent GameObject, and carries the intent across the scene boundary cleanly.

**CN:** 跨场景状态传递使用静态标志而非 DontDestroyOnLoad：
- `GameManager.PendingLoad` 静态布尔值由 MainMenu 设置
- 轻量级，无需持久化 GameObject
- 场景加载后 Start() 根据标志分支执行

---

### 5.5.5 Producer State Machine / 生产器状态机

**EN:** `Productor.Update()` implements a clean state machine without enum overhead:

```
     ┌──────────────────────────────────────┐
     │                                      │
     ▼                                      │
  [IDLE] ──isOn && hasPower──▶ [CONSUME] ──▶ [PRODUCING]
     ▲                              │              │
     │    no power / no input       │              │ progress >= time
     └──────────────────────────────┘              │
                                                   ▼
                                              [COMPLETE]
                                                   │
                                          output to storage
                                          reset isProducing = false
                                                   │
                                                   ▼
                                               [IDLE]
```

**States tracked by booleans** (not enum, to keep it simple):
- `isOn` — player toggled this producer on
- `hasPower` → `PowerConsumer.CanWork()` — power grid has enough supply
- `hasInput` → `HasEnoughInput()` — storage has enough raw materials
- `isProducing` — currently in the production interval
- `currentProgress` — accumulated deltaTime toward `productionTime`

All transitions happen in `Update()`, making the behavior frame-by-frame testable and predictable.

**CN:** Productor 用布尔标志实现简洁状态机（无需枚举开销）：
- 空闲 → 检查电力和原料 → 消耗原料开始生产 → 进度累积 → 完成输出 → 回到空闲
- 所有转换在 `Update()` 中完成，逐帧可测试、可预测

---

## Slide 5.6: Lifecycle Summary / 生命周期总结 (20s)

**EN:**
| Entity | Created By | Registered At | Active In | Destroyed By |
|--------|-----------|---------------|-----------|-------------|
| Singleton | Scene load | `Awake()` | Whole scene | Scene unload / duplicate check |
| Building | `GridManager.Instantiate()` | `Start()` (managers) | `Update()` (production) | `BuildingBase.DestroyBuilding()` |
| Ore | `ResourceSpawner` | N/A | `Update()` (movement) | Collision / boundary / mining |
| UI Entry | `BuildingCategoryPanel` | N/A | Manual refresh | Panel clear / rebuild |
| Save Data | `Ctrl+S` / `SaveSystem` | Serialized to JSON | On disk | Overwritten next save |

**CN:** 五类实体的创建→注册→活跃→销毁路径表，一目了然。

**EN:**
- **Why 2D?** — Simpler development, faster iteration, focus on gameplay systems over visual complexity
- **Why grid-based building?** — Clear placement rules, easy validation, familiar to strategy game players
- **Why progressive production chain?** — Creates meaningful progression, encourages strategic planning
- **Why movable ship?** — Adds a unique twist to base-building — your base IS your ship, and movement lets you explore and find resources
- **Why ore collision damage?** — Creates tension and urgency, makes defense and repair meaningful

**CN:**
- **为什么 2D？** — 开发更简单，迭代更快，专注于玩法系统而非视觉复杂度
- **为什么网格建造？** — 清晰的放置规则，易于验证，策略游戏玩家熟悉
- **为什么逐级生产链？** — 创造有意义的进程感，鼓励策略规划
- **为什么可移动飞船？** — 为基地建造添加独特元素——你的基地就是你的飞船，移动让你探索和寻找资源
- **为什么矿石碰撞伤害？** — 创造紧张感和紧迫感，使防御和修复有意义

---

## Slide 7: Challenges & Solutions / 挑战与解决方案 (30s)

**EN:**
| Challenge | Solution |
|-----------|----------|
| Board buildings lost when saving | Save boards from grid cells independently |
| Resource initialization order | Register containers before adding initial resources |
| Background floating-point drift | Wrap coordinates every 5000 units |
| Audio timing control | Custom SoundEffectSettings with duration & loop |
| Building UI buttons not responding | Ensure resources initialized in Awake() before UI Start() |

**CN:**
| 挑战 | 解决方案 |
|------|----------|
| 存档时太空板丢失 | 从网格格子独立保存太空板 |
| 资源初始化顺序 | 先注册容器再添加初始资源 |
| 背景浮点漂移 | 每 5000 单位回退坐标 |
| 音效时长控制 | 自定义 SoundEffectSettings 配置时长和循环 |
| 建筑 UI 按钮无响应 | 在 Awake() 中初始化资源，确保在 UI Start() 之前 |

---

## Slide 8: Future Improvements / 未来改进 (20s)

**EN:**
- Enemy AI and combat system
- Multiple ship decks/floors
- Research and technology tree
- More building types and resource varieties
- Random events (solar flares, asteroid fields, alien encounters)

**CN:**
- 敌人 AI 和战斗系统
- 多层飞船甲板
- 研究和科技树
- 更多建筑类型和资源种类
- 随机事件（太阳耀斑、小行星带、外星人遭遇）

---

## Slide 9: Thank You / 谢谢 (10s)

**EN:** Thank you for watching! I'm happy to answer any questions.  
**CN:** 感谢观看！欢迎提问。

**EN:** GitHub: [Your Repository Link]  
**CN:** 代码仓库：[仓库链接]

---

## Demo Flow / 演示流程 (5 min)

| Time | Action | EN Narration | CN 解说 |
|------|--------|-------------|---------|
| 0:00 | Launch game, show Main Menu | "This is the main menu. New Game, Load, and Tutorial options." | "这是主菜单，有新建游戏、加载和教程选项。" |
| 0:30 | Click New Game, show initial ship | "The game starts with a 4x4 platform, an emergency shelter, two mining platforms, and a thruster." | "游戏初始有一个4x4平台、紧急避难所、两个采矿机和一个推进器。" |
| 1:00 | Press B, show building UI | "Press B to open the building menu. Buildings are organized by category." | "按B键打开建造菜单，建筑按类别组织。" |
| 1:30 | Place a Solar Array | "Let me place a Solar Array. Green means valid. It costs 20 metal." | "放置一个太阳能板，绿色代表有效，消耗20金属。" |
| 2:00 | Press Tab, fly the ship | "Press Tab to enter flight mode. W to accelerate, A/D to steer." | "按Tab进入飞行模式，W加速，A/D转向。" |
| 2:30 | Show ore collection | "Mining Platforms automatically collect nearby ores. Watch the resource count increase." | "采矿机自动采集附近矿石，看资源数量增加。" |
| 3:00 | Place a Furnace Refinery | "Place a production building to refine ore into metal. The production chain is essential." | "放置生产建筑将矿石精炼成金属，生产链至关重要。" |
| 3:30 | Show power system | "Check the power status. Buildings show warning icons when power is low." | "查看电力状态，电力不足时建筑显示警告图标。" |
| 4:00 | Press Ctrl+S to save | "Ctrl+S saves the game. All state is serialized to JSON." | "Ctrl+S保存游戏，所有状态序列化为JSON。" |
| 4:30 | Show ore collision + explosion | "Ores collide with buildings and cause damage. An explosion effect plays." | "矿石碰撞建筑造成伤害，播放爆炸效果。" |
| 5:00 | End | "That concludes the demo. Thank you!" | "演示结束，谢谢！" |

---

## Q&A Preparation / 问答准备

**Q: Why did you choose Unity?**  
**EN:** Unity has strong 2D support, a large community, and C# is a robust language for game development. The component-based architecture fits well with this project's modular design.  
**CN:** Unity 有强大的 2D 支持、庞大的社区，C# 是游戏开发的优秀语言。组件化架构非常适合本项目的模块化设计。

**Q: How does the save system handle complex building states?**  
**EN:** Buildings are saved with their grid position, type, and direction. Boards are saved separately from grid cells to avoid the dictionary-overwrite issue. Production progress and power state can be extended into the save format.  
**CN:** 建筑以网格位置、类型和方向保存。太空板从网格格子单独保存，避免字典覆盖问题。生产进度和电力状态可扩展进存档格式。

**Q: What was the hardest bug you fixed?**  
**EN:** The board save bug — when a non-board building shared the same origin position as a board, the dictionary entry was overwritten. Fixing it required saving boards from grid cells instead of the placedBuildings dictionary.  
**CN:** 太空板存档 bug——当非板类建筑与板类共享同一原点位置时，字典条目被覆盖。修复需要从网格格子而非 placedBuildings 字典保存板类。

**Q: How does the power system work?**  
**EN:** Each power-consuming building registers with PowerManager. The manager sums all power production and consumption. If consumption exceeds production, buildings are marked as low-power and display warning icons, and production buildings stop working.  
**CN:** 每个耗电建筑向 PowerManager 注册。管理器汇总所有电力生产和消耗。如果消耗超过生产，建筑标记为缺电并显示警告图标，生产建筑停止工作。