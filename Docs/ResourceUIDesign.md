# SpaceWar1 资源UI设计文档

## 1. 资源系统概述

### 1.1 资源类型定义

项目定义了6种核心资源（见 [Enums.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameCore/Enums.cs#L3-L12)）：

| 资源类型 | 中文名 | 描述 | 密度 |
|---------|-------|------|-----|
| SpaceOre | 太空矿石 | 漂浮在太空中的天然矿石 | 2.5 |
| SpaceDebris | 太空垃圾 | 废弃星际文明遗留的残骸 | 1.2 |
| AlloyIngot | 太空合金锭 | 精炼后的高级金属材料 | 3.0 |
| MechanicalPart | 星际机械零件 | 精密加工的机械部件 | 1.5 |
| ElectronicComponent | 电子航天元件 | 高科技电子元件 | 0.8 |
| AdvancedAlloy | 高级合金 | 用于建造飞船的顶级材料 | 4.0 |

### 1.2 资源管理核心

- **全局管理**: [GameManager](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameManager.cs) 提供资源查询接口
  - `GetResourceAmount(type)` - 获取资源数量
  - `AddResource(type, amount)` - 增加资源
  - `RemoveResource(type, amount)` - 消耗资源
  - `HasEnoughResource(type, amount)` - 检查资源是否充足
  - `GetAllResources()` - 获取所有资源字典

- **存储系统**: `StorageManager` 管理实际库存
- **数据配置**: [DataConfig](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/GameCore/DataConfig.cs#L52-L315) 定义资源元数据

---

## 2. 现有UI架构分析

### 2.1 已存在的资源显示组件

[ResourceDisplayUI.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/UI/ResourceDisplayUI.cs) - 基础资源显示

**功能**:
- 动态生成资源条目
- 定时更新显示（默认0.5秒间隔）
- 支持自定义前缀/后缀、颜色、字号

**依赖组件**:
- `RectTransform container` - 容器
- `Text resourceText` - 文本模板
- `GameObject resourceEntryPrefab` - 条目预制体（可选）
- `VerticalLayoutGroup layoutGroup` - 垂直布局

### 2.2 建筑UI系统

[BuildingUI.cs](file:///d:/work/games_programming/SpaceWar1/Assets/Scripts/UI/BuildingUI.cs) - 建筑建造UI

**关联功能**:
- 建筑资源消耗检查 (`CanAfford`)
- 资源不足时按钮禁用
- 建造成功后自动更新所有面板资源状态

---

## 3. Unity需要实现的内容清单

### 3.1 Canvas层级结构

```
Canvas (Screen Space - Overlay 或 Camera)
├── ResourceBarPanel (顶部资源栏)
│   ├── ResourceBarContainer (HorizontalLayoutGroup)
│   │   ├── ResourceEntry_太空矿石 (Prefab实例)
│   │   ├── ResourceEntry_太空垃圾
│   │   ├── ResourceEntry_太空合金锭
│   │   ├── ResourceEntry_星际机械零件
│   │   ├── ResourceEntry_电子航天元件
│   │   └── ResourceEntry_高级合金
│   └── ResourceBarBackground (背景图片)
│
├── BuildingPanel (建筑面板 - 已存在)
│   ├── CategoryTabs
│   ├── ScrollContent
│   └── BuildingButtons
│
└── TooltipPanel (提示框 - 已存在)
    ├── TooltipName
    ├── TooltipDescription
    ├── TooltipCost
    └── TooltipStats
```

### 3.2 预制体制作

#### ResourceEntryPrefab (资源条目预制体)

**组件结构**:
```
ResourceEntryPrefab (GameObject)
├── RectTransform
│   ├── Anchors: 中心对齐
│   ├── SizeDelta: (200, 30)
│   └── Pivot: (0.5, 0.5)
├── Image (背景)
│   ├── Source Image: 可选背景图
│   └── Color: 半透明深色
├── IconImage (资源图标)
│   ├── RectTransform (左侧)
│   ├── Source Image: Resources/Icons/Resources/xxx.png
│   └── SizeDelta: (24, 24)
├── Text (资源名称+数量)
│   ├── RectTransform (右侧)
│   ├── Font: 项目字体
│   ├── Font Size: 14
│   ├── Color: White
│   └── Alignment: Middle Left
└── EventTriggerListener (可选，用于点击交互)
```

**脚本绑定**: 
- 可创建 `ResourceEntryUI.cs` 单独管理每个条目
- 或直接由 `ResourceDisplayUI` 统一管理

### 3.3 资源图标资源

需要在 `Assets/Resources/Icons/Resources/` 目录下创建：

| 文件名 | 对应资源 | 尺寸建议 |
|-------|---------|---------|
| SpaceOre.png | 太空矿石 | 64x64 / 128x128 |
| SpaceDebris.png | 太空垃圾 | 64x64 / 128x128 |
| AlloyIngot.png | 太空合金锭 | 64x64 / 128x128 |
| MechanicalPart.png | 星际机械零件 | 64x64 / 128x128 |
| ElectronicComponent.png | 电子航天元件 | 64x64 / 128x128 |
| AdvancedAlloy.png | 高级合金 | 64x64 / 128x128 |

**导入设置**:
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 100
- Generate Mip Maps: 取消勾选
- Compression: None 或 Normal Quality

### 3.4 UI美术资源

#### 背景装饰
- 资源栏背景条 (横条状，半透明科幻风格)
- 资源条目分隔线 (可选)
- 资源图标外框 (圆形/六边形/科幻边框)

#### 状态指示
- 资源充足: 正常颜色
- 资源不足: 红色高亮/闪烁效果
- 资源变化: +数字上浮动画 / -数字红色提示

**可复用现有资源**:
- `Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/bars/` - 进度条素材
- `Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/window/` - 窗口背景
- `DinV/Dynamic Space Background/Sprites/board.png` - 面板背景

---

## 4. 数据流与更新机制

### 4.1 更新流程图

```
GameManager 资源变化
    ↓
触发事件/轮询 (当前实现: 每0.5秒轮询)
    ↓
ResourceDisplayUI.UpdateResourceDisplay()
    ↓
遍历 resourceTexts 字典
    ↓
更新每个 Text.text = "资源名: 数量"
```

### 4.2 建议优化方案

#### 方案A: 事件驱动 (推荐)
```csharp
// 在 GameManager 添加事件
public static event Action<ResourceType, int> OnResourceChanged;

// 资源变化时触发
public bool AddResource(ResourceType type, int amount)
{
    if (storageManager.AddResource(type, amount))
    {
        OnResourceChanged?.Invoke(type, GetResourceAmount(type));
        return true;
    }
    return false;
}

// ResourceDisplayUI 订阅事件
private void OnEnable()
{
    GameManager.OnResourceChanged += OnResourceChanged;
}

private void OnResourceChanged(ResourceType type, int newAmount)
{
    if (resourceTexts.ContainsKey(type))
    {
        UpdateResourceEntry(type, newAmount);
    }
}
```

#### 方案B: 保持轮询 (当前实现)
- 优点: 简单，无需修改GameManager
- 缺点: 性能浪费，更新延迟

---

## 5. 交互设计

### 5.1 基础交互

| 操作 | 效果 |
|-----|------|
| 鼠标悬停资源条目 | 显示资源详情Tooltip |
| 点击资源条目 | (可扩展) 打开资源详情面板 |
| 资源数量变化 | 数字变化动画/颜色闪烁 |

### 5.2 Tooltip内容设计

悬停资源时显示：
```
[图标] 太空矿石
━━━━━━━━━━━━━━
描述: 漂浮在太空中的天然矿石
密度: 2.5

当前库存: 150

用途:
- 建造采矿平台 (需要20)
- 精炼合金锭 (需要2)
- 核能发电 (消耗品)
```

---

## 6. 场景配置步骤

### 6.1 GameScene设置

1. **打开场景**: `Assets/Scenes/GameScene.unity`

2. **创建Canvas** (如不存在)
   - GameObject → UI → Canvas
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: Match Width Or Height (0.5)

3. **创建ResourceBarPanel**
   - 在Canvas下创建Panel
   - 添加组件:
     - Image (背景)
     - HorizontalLayoutGroup (或让ResourceDisplayUI管理)
   - 设置Anchor: Top-Center
   - 设置位置: 屏幕顶部

4. **绑定ResourceDisplayUI**
   - 创建空GameObject: "ResourceDisplay"
   - 添加组件: `ResourceDisplayUI`
   - 配置Inspector:
     - Container: ResourceBarContainer的RectTransform
     - Resource Text: 拖入Text组件作为模板
     - Resource Entry Prefab: 拖入预制体（可选）
     - Resource Prefix: "" (或自定义)
     - Resource Suffix: "" (或自定义)
     - Text Color: White
     - Font Size: 14
     - Spacing: 5

5. **创建ResourceEntryPrefab**
   - 按3.2节结构创建
   - 保存为 `Assets/Prefab/UI/ResourceEntryPrefab.prefab`

### 6.2 测试验证

1. 运行游戏
2. 检查顶部是否正确显示6种资源
3. 使用作弊按钮或代码修改资源数量
4. 验证UI是否自动更新

---

## 7. 扩展功能规划

### 7.1 资源详情面板 (后续开发)

点击资源条目弹出：
- 资源获取来源
- 资源消耗建筑列表
- 当前生产效率统计
- 库存趋势图

### 7.2 资源预警系统

- 资源不足时红色闪烁
- 关键资源低于阈值时播放提示音
- 可配置预警阈值

### 7.3 生产队列显示

与建筑UI集成：
- 显示正在生产的物品
- 显示预计完成时间
- 显示所需资源是否充足

---

## 8. 文件清单汇总

### 8.1 需要创建的文件

| 路径 | 类型 | 说明 |
|-----|------|------|
| `Assets/Resources/Icons/Resources/SpaceOre.png` | Texture | 太空矿石图标 |
| `Assets/Resources/Icons/Resources/SpaceDebris.png` | Texture | 太空垃圾图标 |
| `Assets/Resources/Icons/Resources/AlloyIngot.png` | Texture | 太空合金锭图标 |
| `Assets/Resources/Icons/Resources/MechanicalPart.png` | Texture | 星际机械零件图标 |
| `Assets/Resources/Icons/Resources/ElectronicComponent.png` | Texture | 电子航天元件图标 |
| `Assets/Resources/Icons/Resources/AdvancedAlloy.png` | Texture | 高级合金图标 |
| `Assets/Prefab/UI/ResourceEntryPrefab.prefab` | Prefab | 资源条目预制体 |
| `Assets/Scripts/UI/ResourceEntryUI.cs` | Script | 单条目UI逻辑 (可选) |

### 8.2 现有文件（需使用）

| 路径 | 用途 |
|-----|------|
| `Assets/Scripts/UI/ResourceDisplayUI.cs` | 资源显示主控制器 |
| `Assets/Scripts/GameManager.cs` | 资源数据源 |
| `Assets/Scripts/GameCore/DataConfig.cs` | 资源定义配置 |
| `Assets/Scripts/GameCore/ResourceData.cs` | 资源数据结构 |
| `Assets/Scenes/GameScene.unity` | 游戏主场景 |

### 8.3 可复用美术资源

| 路径 | 用途 |
|-----|------|
| `Assets/Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/bars/` | 进度条/背景条 |
| `Assets/Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/window/` | 面板背景 |
| `Assets/JazzCreate/BubbleFontFree/Fonts/` | 字体资源 |

---

## 9. 开发优先级建议

### Phase 1: 基础显示 (必做)
1. 创建6个资源图标
2. 制作ResourceEntryPrefab
3. 在GameScene配置ResourceDisplayUI
4. 测试资源显示和更新

### Phase 2: 视觉优化 (建议做)
1. 设计资源栏背景
2. 添加图标边框
3. 优化字体和颜色
4. 添加资源变化动画

### Phase 3: 交互增强 (可选)
1. 实现资源Tooltip
2. 添加资源预警
3. 点击展开详情面板
4. 事件驱动更新优化
