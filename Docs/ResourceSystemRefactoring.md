# SpaceWar1 资源系统重构文档

## 1. 重构概述

### 1.1 重构目标
将原有的6种资源体系简化为4种资源，并移除传送带物流系统，改为所有建筑直接从全局库存中扣除原料和添加成品。

### 1.2 新的资源体系

| 资源类型 | 中文名 | 描述 | 获取方式 |
|---------|-------|------|---------|
| SpaceOre | 太空矿石 | 漂浮在太空中的天然矿石 | 采矿平台自动采集 |
| MetalMaterial | 金属材料 | 精炼后的高级金属材料 | 熔炉精炼厂生产 |
| BasicPart | 初级零件 | 精密加工的机械部件 | 零件组装厂生产 |
| AdvancedPart | 高级零件 | 用于建造飞船的顶级材料 | 高级加工厂生产 |

### 1.3 生产链条

```
太空矿石 → (熔炉精炼厂) → 金属材料 → (零件组装厂) → 初级零件 → (高级加工厂) → 高级零件
```

---

## 2. 文件变更清单

### 2.1 核心枚举修改

#### [Enums.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameCore/Enums.cs#L3-L9)
**变更内容**：将ResourceType从6种改为4种

```csharp
// 旧版本
public enum ResourceType
{
    None,
    SpaceOre,
    SpaceDebris,        // 已移除
    AlloyIngot,         // 已移除
    MechanicalPart,     // 已移除
    ElectronicComponent,// 已移除
    AdvancedAlloy       // 已移除
}

// 新版本
public enum ResourceType
{
    None,
    SpaceOre,
    MetalMaterial,      // 新增：金属材料
    BasicPart,          // 新增：初级零件
    AdvancedPart        // 新增：高级零件
}
```

### 2.2 数据配置修改

#### [DataConfig.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameCore/DataConfig.cs)
**变更内容**：
1. 更新资源定义（4种资源）
2. 更新配方定义（3个配方）
3. 更新建筑成本和产出

**新配方定义**：
```csharp
// 熔炉精炼厂：太空矿石 → 金属材料
var refineMetal = new RecipeDefinition("精炼金属材料", 5f, BuildingType.FurnaceRefinery);
refineMetal.AddIngredient(ResourceType.SpaceOre, 3);
refineMetal.SetOutput(ResourceType.MetalMaterial, 1);

// 零件组装厂：金属材料 → 初级零件
var makeBasicPart = new RecipeDefinition("制造初级零件", 8f, BuildingType.PartAssembly);
makeBasicPart.AddIngredient(ResourceType.MetalMaterial, 2);
makeBasicPart.SetOutput(ResourceType.BasicPart, 1);

// 高级加工厂：初级零件 + 金属材料 → 高级零件
var makeAdvancedPart = new RecipeDefinition("制造高级零件", 12f, BuildingType.AdvancedFactory);
makeAdvancedPart.AddIngredient(ResourceType.BasicPart, 3);
makeAdvancedPart.AddIngredient(ResourceType.MetalMaterial, 2);
makeAdvancedPart.SetOutput(ResourceType.AdvancedPart, 1);
```

### 2.3 存储管理器重构

#### [StorageManager.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/LogisticsSystem\StorageManager.cs)
**变更内容**：移除分散式存储，改为纯全局库存管理

**主要变化**：
- 移除 `storageComponents` 列表（不再有分散的建筑存储）
- 所有资源操作直接作用于 `globalInventory`
- 移除 `RegisterStorage/UnregisterStorage` 方法
- 移除容量相关方法（`GetTotalStorageCapacity`, `GetUsedStorageCapacity`）
- 新增 `GetTotalItemCount()` 方法

### 2.4 游戏管理器重构

#### [GameManager.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameManager.cs)
**变更内容**：移除传送带系统，简化资源管理

**主要变化**：
- 移除 `ConveyorSystem` 引用
- 移除传送带注册方法（`RegisterConveyor`, `UnregisterConveyor`）
- 移除分拣器注册方法（`RegisterSorter`, `UnregisterSorter`）
- 移除仓储注册方法（`RegisterStorage`, `UnregisterStorage`）
- 移除 `TryTransferResource` 方法
- 简化初始资源（仅太空矿石 50）
- 更新胜利条件（AdvancedPart 100 + BasicPart 80）

### 2.5 建筑系统修改

#### [BuildingComponent.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GridSystem\BuildingComponent.cs)
**变更内容**：自动注册生产建筑到ProductionManager

**新增功能**：
```csharp
// 在Awake中自动注册生产建筑
productionBuilding = GetComponent<ProductionBuilding>();
if (productionBuilding != null && GameManager.Instance != null)
{
    GameManager.Instance.RegisterProductionBuilding(productionBuilding);
}

// 在OnDestroy中自动注销
protected virtual void OnDestroy()
{
    if (productionBuilding != null && GameManager.Instance != null)
    {
        GameManager.Instance.UnregisterProductionBuilding(productionBuilding);
    }
}
```

#### [MiningBuilding.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/ProductionSystem\MiningBuilding.cs)
**变更内容**：采集矿石时自动添加到全局库存

**新增功能**：
```csharp
// 新增miningAmount字段
public int miningAmount = 1;

// 采集时添加到全局库存
ore.Collect();
if (GameManager.Instance != null)
{
    GameManager.Instance.AddResource(ResourceType.SpaceOre, miningAmount);
}
```

### 2.6 电力系统修改

#### [PowerProducer.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/PowerSystem\PowerProducer.cs)
**变更内容**：支持燃料消耗（如核能发电消耗矿石）

**新增功能**：
```csharp
// 新增燃料相关字段
[SerializeField] protected ResourceType fuelResource = ResourceType.None;
[SerializeField] protected int fuelConsumptionPerSecond = 0;

// 每秒检查燃料消耗
protected virtual void Update()
{
    if (fuelResource != ResourceType.None && fuelConsumptionPerSecond > 0)
    {
        if (fuelTimer >= 1f)
        {
            if (GameManager.Instance.HasEnoughResource(fuelResource, fuelConsumptionPerSecond))
            {
                GameManager.Instance.RemoveResource(fuelResource, fuelConsumptionPerSecond);
            }
            else
            {
                isActive = false; // 燃料不足时停止发电
            }
        }
    }
}
```

### 2.7 UI代码更新

#### [CheatButton.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/UI\CheatButton.cs)
**变更内容**：适配新的4种资源

```csharp
GameManager.Instance.AddResource(ResourceType.SpaceOre, 9999);
GameManager.Instance.AddResource(ResourceType.MetalMaterial, 9999);
GameManager.Instance.AddResource(ResourceType.BasicPart, 9999);
GameManager.Instance.AddResource(ResourceType.AdvancedPart, 9999);
```

---

## 3. 废弃/待清理内容

### 3.1 传送带系统（LogisticsSystem）
以下文件在新架构中不再使用，但保留代码以备后续可能的需求：

| 文件 | 状态 | 说明 |
|-----|------|------|
| [ConveyorSystem.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/LogisticsSystem/ConveyorSystem.cs) | 废弃 | GameManager中已移除引用 |
| [ConveyorBelt.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/LogisticsSystem/ConveyorBelt.cs) | 废弃 | 仍会自动注册但无实际作用 |
| [SorterComponent.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/LogisticsSystem/SorterComponent.cs) | 废弃 | 同上 |
| [StorageComponent.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/LogisticsSystem\StorageComponent.cs) | 废弃 | StorageManager不再使用分散存储 |

### 3.2 资源相关
| 内容 | 状态 |
|-----|------|
| SpaceDebris（太空垃圾） | 已移除 |
| AlloyIngot（太空合金锭） | 已移除 |
| MechanicalPart（星际机械零件） | 已移除 |
| ElectronicComponent（电子航天元件） | 已移除 |
| AdvancedAlloy（高级合金） | 已移除 |

---

## 4. 数据流图

### 4.1 资源获取流程

```
太空矿石生成 → SpaceOre漂浮
    ↓
采矿平台检测到矿石
    ↓
MiningBuilding.CollectResourcesInRange()
    ↓
GameManager.AddResource(SpaceOre, amount)
    ↓
StorageManager.globalInventory更新
    ↓
ResourceDisplayUI自动更新显示
```

### 4.2 生产流程

```
玩家选择配方 → ProductionBuilding.StartProduction(recipe)
    ↓
检查全局库存是否充足 → GameManager.HasEnoughResource(recipe)
    ↓
扣除原料 → GameManager.RemoveResource(ingredient)
    ↓
等待生产时间 → productionProgress增加
    ↓
生产完成 → CompleteProduction()
    ↓
添加成品到全局库存 → GameManager.AddResource(output)
```

### 4.3 电力系统流程

```
PowerProducer.Update()
    ↓
检查燃料消耗 → fuelTimer >= 1f
    ↓
检查库存 → GameManager.HasEnoughResource(fuelResource)
    ↓
扣除燃料 → GameManager.RemoveResource(fuelResource)
    ↓
发电 → PowerManager.RegisterProducer(this)
```

---

## 5. Unity Inspector配置

### 5.1 核能发电模块（需要配置燃料）
```
NuclearReactor (Prefab)
├── PowerProducer
│   ├── Fuel Resource: SpaceOre
│   ├── Fuel Consumption Per Second: 1 (可调整)
│   ├── Power Output: 50
── BuildingComponent
│   ├── Building Type: NuclearReactor
```

### 5.2 采矿平台配置
```
MiningPlatform (Prefab)
├── MiningBuilding
│   ├── Collection Range: 2
│   ├── Collection Interval: 2
│   ├── Mining Amount: 1 (每次采集获得矿石数量)
├── BuildingComponent
│   ├── Building Type: MiningPlatform
```

### 5.3 生产建筑配置
```
FurnaceRefinery (Prefab)
├── ProductionBuilding (自动注册到GameManager)
│   ├── PowerConsumer (耗电检查)
── BuildingComponent
│   ├── Building Type: FurnaceRefinery
```

---

## 6. 建筑定义总览

### 6.1 核心设施
| 建筑 | 成本 | 说明 |
|-----|------|------|
| 太空紧急避难仓 | 无 | 玩家开局核心根基 |

### 6.2 能源设施
| 建筑 | 成本 | 发电 | 燃料 |
|-----|------|------|------|
| 太空核能发电模块 | 太空矿石 x50 | 50 | 太空矿石（需配置） |
| 太空太阳能发电阵列 | 金属材料 x30, 初级零件 x10 | 30 | 无 |

### 6.3 生产设施
| 建筑 | 成本 | 耗电 | 功能 |
|-----|------|------|------|
| 太空漂浮采矿平台 | 无 | 10 | 自动采集太空矿石 |
| 熔炉精炼厂 | 太空矿石 x40 | 15 | 矿石→金属材料 |
| 零件组装厂 | 金属材料 x30, 太空矿石 x20 | 20 | 金属材料→初级零件 |
| 高级加工厂 | 金属材料 x50, 初级零件 x30 | 40 | 初级零件→高级零件 |

### 6.4 仓储设施
| 建筑 | 成本 | 容量 |
|-----|------|------|
| 太空仓储对接舱 | 太空矿石 x30 | 500 |

### 6.5 特殊设施
| 建筑 | 成本 | 耗电 | 功能 |
|-----|------|------|------|
| 飞船组装平台 | 高级零件 x100, 高级零件 x50, 初级零件 x80 | 100 | 胜利条件 |

---

## 7. 配方总览

| 配方 | 建筑 | 时间 | 输入 | 输出 |
|-----|------|------|------|------|
| 精炼金属材料 | 熔炉精炼厂 | 5秒 | 太空矿石 x3 | 金属材料 x1 |
| 制造初级零件 | 零件组装厂 | 8秒 | 金属材料 x2 | 初级零件 x1 |
| 制造高级零件 | 高级加工厂 | 12秒 | 初级零件 x3, 金属材料 x2 | 高级零件 x1 |

---

## 8. 胜利条件

```csharp
// 需要以下资源才能建造飞船
HasEnoughResource(ResourceType.AdvancedPart, 100) &&
HasEnoughResource(ResourceType.BasicPart, 80)
```

---

## 9. 注意事项

### 9.1 需要手动配置的内容
1. 核能发电模块的 `Fuel Resource` 和 `Fuel Consumption Per Second` 需要在Unity Inspector中配置
2. 采矿平台的 `Mining Amount` 可根据需要调整每次采集获得的矿石数量
3. 初始资源在 `GameManager.InitializeInitialResources()` 中配置

### 9.2 待优化的内容
1. 核能发电模块缺少专用的NuclearReactor脚本（当前使用PowerProducer组件）
2. 传送带系统代码虽然废弃但未删除，可根据需要清理
3. StorageComponent组件不再使用，可从仓储建筑Prefab中移除
4. 生产建筑的配方选择UI尚未实现

### 9.3 兼容性问题
1. 所有旧资源类型（SpaceDebris、AlloyIngot等）已移除，确保场景中无相关引用
2. 建筑定义中的成本已更新，确保Prefab配置与代码一致
3. 胜利条件已更新为新的资源类型

---

## 10. 测试建议

1. **资源采集测试**：放置采矿平台，验证太空矿石是否正确添加到全局库存
2. **生产流程测试**：依次测试3个配方的生产流程
3. **电力系统测试**：验证核能发电的燃料消耗逻辑
4. **UI更新测试**：验证资源数量变化时UI是否自动更新
5. **胜利条件测试**：收集足够资源验证胜利触发
6. **建筑拆除测试**：拆除生产建筑验证是否正确注销
