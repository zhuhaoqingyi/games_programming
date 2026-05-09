# MiningBuilding.cs 采矿建筑脚本说明文档

## 一、脚本概述

`MiningBuilding.cs` 是《异星太空工厂》中的核心采矿组件，负责自动采集范围内的太空资源（如太空矿石、太空垃圾）。

### 功能定位
- 挂在采矿平台建筑上
- 自动扫描并采集范围内的资源
- 依赖电力系统运行
- 支持范围和间隔时间配置

---

## 二、参数说明

### 2.1 采矿设置

| 参数名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `collectionRange` | int | 2 | 采集范围（n×n格子） |
| `collectionInterval` | float | 2f | 采集间隔时间（秒） |
| `minedResource` | ResourceType | SpaceOre | 采集的资源类型 |

### 2.2 内部变量

| 变量名 | 类型 | 说明 |
|--------|------|------|
| `timer` | float | 采集计时器 |
| `powerConsumer` | PowerConsumer | 电力消费者组件引用 |

---

## 三、核心功能

### 3.1 范围采集

使用 `Physics2D.OverlapBoxAll` 进行碰撞检测，扫描指定范围内的所有矿石。

```csharp
private void CollectResourcesInRange()
{
    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
        transform.position,                      // 中心点：采矿平台位置
        new Vector2(collectionRange, collectionRange), // 范围大小
        0f                                      // 旋转角度
    );

    foreach (Collider2D collider in hitColliders)
    {
        SpaceOre ore = collider.GetComponent<SpaceOre>();
        if (ore != null && !ore.IsCollected())
        {
            ore.Collect();
        }
    }
}
```

### 3.2 电力检测

采矿平台需要消耗电力才能工作，只有在有足够电力时才会采集。

```csharp
public override bool CanWork()
{
    return base.CanWork() && powerConsumer != null && powerConsumer.CanWork();
}
```

### 3.3 定时采集

每间隔 `collectionInterval` 秒进行一次采集，而不是实时检测。

```csharp
timer += deltaTime;

if (timer >= collectionInterval)
{
    timer = 0;
    CollectResourcesInRange();
}
```

---

## 四、工作流程

```
游戏开始
    │
    ▼
检测是否有电力供应
    │
    ├── 无电力 → 停止工作
    │
    └── 有电力 → 继续
                │
                ▼
            计时器累加
                │
                ├── 未到采集间隔 → 等待
                │
                └── 到达采集间隔 → 执行采集
                                │
                                ▼
                            扫描范围内所有碰撞体
                                │
                                ▼
                            筛选出SpaceOre组件
                                │
                                ▼
                            调用Collect()方法
                                │
                                ▼
                            矿石销毁，资源入库
```

---

## 五、采集范围示意

```
collectionRange = 2 时，采集范围为 2×2 格子：

    ┌─────────┐
    │         │
    │  [M]    │  M = 采矿平台
    │         │  ★ = 可采集范围
    │    ★★   │
    │     ★★  │
    └─────────┘

实际检测区域：
    ┌─────────────────┐
    │                 │
    │                 │
    │      [M]        │
    │                 │
    │                 │
    └─────────────────┘
```

---

## 六、Unity组件依赖

### 6.1 必需组件

| 组件 | 说明 |
|------|------|
| BuildingComponent | 建筑基类组件 |
| PowerConsumer | 电力消费者，消耗电力 |
| Collider2D | 碰撞体（用于射线检测） |

### 6.2 推荐组件组合

```
MiningPlatform 预制件
├── BuildingComponent.cs
├── MiningBuilding.cs
├── PowerConsumer.cs (powerInput = 10)
└── BoxCollider2D (IsTrigger = true)
```

---

## 七、使用步骤

### 7.1 创建采矿平台预制件

1. 创建空物体，命名为 `MiningPlatform`
2. 添加 `BuildingComponent` 子类
3. 添加 `MiningBuilding.cs` 脚本
4. 设置参数：
   - `collectionRange` = 2
   - `collectionInterval` = 2f
   - `minedResource` = SpaceOre

### 7.2 配置依赖组件

1. 添加 `PowerConsumer.cs`
2. 设置 `powerInput` = 10
3. 添加 `BoxCollider2D`（用于检测）

### 7.3 设置预制件

1. 将配置好的物体拖入 `Assets/Prefabs/Buildings/`
2. 在游戏中通过建筑选择系统放置

---

## 八、扩展建议

### 8.1 支持多种资源

修改 `minedResource` 或添加资源类型数组：

```csharp
public ResourceType[] minedResources = new ResourceType[]
{
    ResourceType.SpaceOre,
    ResourceType.SpaceDebris
};
```

### 8.2 采集特效

在采集时播放粒子效果：

```csharp
private void PlayCollectEffect()
{
    // 实例化采集特效
    Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
}
```

### 8.3 采集声音

添加采集音效：

```csharp
private void PlayCollectSound()
{
    AudioSource.PlayClipAtPoint(collectSound, transform.position);
}
```

---

## 九、调试方法

### 9.1 查看采集范围

在Scene视图中选中采矿平台，可看到绿色半透明范围框：

```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = new Color(0, 1, 0, 0.3f);
    Gizmos.DrawCube(transform.position, new Vector3(collectionRange, collectionRange, 0.1f));
}
```

### 9.2 控制台日志

在关键位置添加日志：

```csharp
private void CollectResourcesInRange()
{
    Debug.Log($"[Mining] Collecting resources, found {hitColliders.Length} objects");
    // ...
}
```

---

## 十、完整代码

```csharp
using UnityEngine;
using GameCore;
using PowerSystem;
using GameResources;

namespace ProductionSystem
{
    public class MiningBuilding : GridSystem.BuildingComponent
    {
        [Header("采矿设置")]
        public int collectionRange = 2;
        public float collectionInterval = 2f;
        public ResourceType minedResource = ResourceType.SpaceOre;

        private float timer;
        private PowerConsumer powerConsumer;

        protected override void Awake()
        {
            base.Awake();
            powerConsumer = GetComponent<PowerConsumer>();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!CanMine()) return;

            timer += deltaTime;

            if (timer >= collectionInterval)
            {
                timer = 0;
                CollectResourcesInRange();
            }
        }

        public override bool CanWork()
        {
            return base.CanWork() && powerConsumer != null && powerConsumer.CanWork();
        }

        private bool CanMine()
        {
            return CanWork();
        }

        private void CollectResourcesInRange()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                transform.position,
                new Vector2(collectionRange, collectionRange),
                0f
            );

            foreach (Collider2D collider in hitColliders)
            {
                SpaceOre ore = collider.GetComponent<SpaceOre>();
                if (ore != null && !ore.IsCollected())
                {
                    ore.Collect();
                }
            }
        }

        public int GetCollectionRange()
        {
            return collectionRange;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, new Vector3(collectionRange, collectionRange, 0.1f));
        }
    }
}
```

---

**文档版本**: v1.0
**创建日期**: 2026年4月28日
**项目**: 异星太空工厂