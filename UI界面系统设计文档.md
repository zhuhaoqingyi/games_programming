# 《异星太空工厂》UI界面系统设计文档

## 一、UI系统概述

### 1.1 设计目标

本UI系统旨在为《异星太空工厂》游戏提供直观、高效的用户交互界面，支持玩家进行建筑建造、资源管理、生产监控等核心操作。

### 1.2 UI架构

```
UI系统
├── 主菜单UI
├── 游戏内UI
│   ├── 顶部资源面板
│   ├── 左侧建筑选择面板
│   ├── 右侧生产监控面板
│   ├── 底部状态栏
│   └── 建筑信息弹窗
└── 胜利/失败界面
```

### 1.3 设计原则

- **简洁直观**：避免信息过载，核心信息一目了然
- **深空风格**：采用深色主题，契合太空工厂氛围
- **响应式布局**：适配不同屏幕分辨率
- **即时反馈**：操作后立即显示结果

---

## 二、主菜单UI

### 2.1 功能需求

| 功能 | 描述 |
|------|------|
| 开始新游戏 | 初始化新游戏存档 |
| 继续游戏 | 加载最近存档 |
| 存档管理 | 查看、删除存档 |
| 设置 | 调整游戏参数（音效、画质等） |
| 退出游戏 | 返回桌面 |

### 2.2 界面布局

```
┌─────────────────────────────────────┐
│         游戏标题：异星太空工厂         │
│           [游戏logo/背景图]           │
├─────────────────────────────────────┤
│          ▶ 开始新游戏               │
│          ▶ 继续游戏                 │
│          ▶ 存档管理                 │
│          ▶ 设置                     │
│          ▶ 退出游戏                 │
├─────────────────────────────────────┤
│           版本号 v1.0               │
└─────────────────────────────────────┘
```

### 2.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| TitleText | Text | 显示游戏标题 |
| StartButton | Button | 开始新游戏 |
| ContinueButton | Button | 继续游戏 |
| SaveManageButton | Button | 存档管理 |
| SettingsButton | Button | 设置界面 |
| ExitButton | Button | 退出游戏 |

---

## 三、游戏内UI

### 3.1 顶部资源面板

#### 3.1.1 功能需求

实时显示玩家当前拥有的各类资源数量和电力状态。

#### 3.1.2 界面布局

```
┌─────────────────────────────────────────────────────────────────┐
│ [矿石图标] 50    [垃圾图标] 30    [合金锭] 15    [零件] 8      │
│ [电子元件] 3     [高级合金] 0                                   │
│                                                                 │
│ ⚡ 电力: ████████████░░░░░░  500/1000  (+30/-25)                │
└─────────────────────────────────────────────────────────────────┘
```

#### 3.1.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| ResourceIcons | Image[] | 资源图标 |
| ResourceAmounts | Text[] | 资源数量 |
| PowerBar | Slider | 电力存储进度条 |
| PowerText | Text | 电力数值和净功率 |

#### 3.1.4 数据来源

- 资源数据：`GameManager.Instance.GetAllResources()`
- 电力数据：`PowerManager.Instance`

---

### 3.2 左侧建筑选择面板

#### 3.2.1 功能需求

提供建筑列表，玩家可选择要建造的建筑。

#### 3.2.2 界面布局

```
┌─────────────┐
│   建筑菜单   │
├─────────────┤
│ ● 采矿平台   │  ← 当前选中
│ ● 发电站     │
│ ● 仓库       │
│ ● 精炼厂     │
│ ● 零件厂     │
│ ● 高级工厂   │
│ ● 传送带     │
│ ● 分拣器     │
│ ● 飞船平台   │
└─────────────┘
```

#### 3.2.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| BuildingList | ScrollRect | 建筑列表容器 |
| BuildingButton | Button[] | 建筑选择按钮 |
| BuildingIcon | Image | 建筑图标 |
| BuildingName | Text | 建筑名称 |

#### 3.2.4 交互逻辑

1. 点击建筑按钮 → 选中建筑
2. 显示建筑建造成本
3. 鼠标移动到地图上显示预览
4. 左键放置建筑
5. 右键取消选择

---

### 3.3 右侧生产监控面板

#### 3.3.1 功能需求

显示当前生产建筑的运行状态和生产进度。

#### 3.3.2 界面布局

```
┌─────────────────────┐
│    生产监控         │
├─────────────────────┤
│ [熔炉精炼厂]        │
│  状态: 运行中       │
│  进度: ██████░░░░   │
│  产出: 合金锭 x1    │
├─────────────────────┤
│ [零件组装厂]        │
│  状态: 等待原料     │
│  进度: 0%           │
│  所需: 合金锭 x2    │
└─────────────────────┘
```

#### 3.3.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| ProductionList | ScrollRect | 生产建筑列表 |
| BuildingStatus | Text | 运行状态 |
| ProgressBar | Slider | 生产进度条 |
| RecipeInfo | Text | 配方信息 |

---

### 3.4 底部状态栏

#### 3.4.1 功能需求

显示游戏时间、当前选中建筑信息、操作提示。

#### 3.4.2 界面布局

```
┌─────────────────────────────────────────────────────────────────┐
│ 第 1 天  08:30    │    选中: 采矿平台    │    左键放置 / 右键删除 │
└─────────────────────────────────────────────────────────────────┘
```

#### 3.4.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| GameTimeText | Text | 游戏时间显示 |
| SelectedBuildingText | Text | 当前选中建筑 |
| HintText | Text | 操作提示 |

---

### 3.5 建筑信息弹窗

#### 3.5.1 功能需求

点击已放置建筑时显示详细信息和操作选项。

#### 3.5.2 界面布局

```
┌─────────────────────┐
│    采矿平台         │
├─────────────────────┤
│ 状态: 运行中       │
│ 功率消耗: 10W      │
│ 采矿速度: 1/3秒    │
│ 产出: 太空矿石     │
├─────────────────────┤
│ [暂停]  [拆除]      │
└─────────────────────┘
```

#### 3.5.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| BuildingName | Text | 建筑名称 |
| StatusText | Text | 运行状态 |
| PowerText | Text | 电力消耗/产出 |
| ProductionText | Text | 生产信息 |
| PauseButton | Button | 暂停/恢复 |
| DestroyButton | Button | 拆除建筑 |

---

## 四、胜利界面

### 4.1 功能需求

达成胜利条件时显示通关界面。

### 4.2 界面布局

```
┌─────────────────────────────────────┐
│         🎉 恭喜通关！🎉              │
│                                    │
│    您已成功建造星际飞船！            │
│                                    │
│    游戏时长: 2小时35分钟            │
│    建造建筑: 45座                   │
│    生产物资: 1250单位               │
├─────────────────────────────────────┤
│          ▶ 继续探索                 │
│          ▶ 重新开始                 │
│          ▶ 返回主菜单               │
└─────────────────────────────────────┘
```

### 4.3 UI组件

| 组件名称 | 类型 | 功能 |
|----------|------|------|
| VictoryTitle | Text | 胜利标题 |
| StatsPanel | Panel | 统计信息面板 |
| ContinueButton | Button | 继续探索 |
| RestartButton | Button | 重新开始 |
| MainMenuButton | Button | 返回主菜单 |

---

## 五、交互流程图

### 5.1 建筑建造流程

```
玩家点击建筑按钮
       │
       ▼
选中建筑 → 显示建造成本
       │
       ▼
鼠标移动 → 显示建筑预览
       │
       ▼
左键点击 → 检查资源是否足够
       │
       ├── 足够 → 扣除资源 → 放置建筑
       │
       └── 不足 → 显示红色预览 → 提示资源不足
```

### 5.2 资源采集流程

```
采矿平台运行
       │
       ▼
自动产出资源 → 更新资源面板
       │
       ▼
触发资源变化事件 → UI刷新
```

### 5.3 生产流程

```
生产建筑启动
       │
       ▼
检查原料是否足够
       │
       ├── 足够 → 消耗原料 → 开始生产
       │              │
       │              ▼
       │         更新进度条
       │              │
       │              ▼
       │         生产完成 → 产出产品
       │
       └── 不足 → 显示等待状态
```

---

## 六、UI数据绑定

### 6.1 资源面板数据绑定

```csharp
// 监听资源变化事件
GameManager.Instance.OnResourceChanged += UpdateResourcePanel;

private void UpdateResourcePanel(Dictionary<ResourceType, int> resources)
{
    foreach (var resource in resources)
    {
        UpdateResourceIcon(resource.Key, resource.Value);
    }
}
```

### 6.2 电力面板数据绑定

```csharp
// 每帧更新电力状态
private void Update()
{
    float powerPercent = PowerManager.Instance.GetPowerPercentage();
    powerBar.value = powerPercent;
    powerText.text = $"{PowerManager.Instance.powerStorage}/{PowerManager.Instance.maxPowerStorage}";
}
```

### 6.3 生产面板数据绑定

```csharp
// 监听生产建筑注册
GameManager.Instance.OnBuildingPlaced += (pos, type) =>
{
    if (IsProductionBuilding(type))
    {
        AddProductionPanel(type);
    }
};
```

---

## 七、UI组件清单

| 序号 | 组件名称 | 所属面板 | 预制件路径 |
|------|----------|----------|------------|
| 1 | ResourcePanel | 顶部资源面板 | UI/Panels/ResourcePanel |
| 2 | BuildingPanel | 左侧建筑面板 | UI/Panels/BuildingPanel |
| 3 | ProductionPanel | 右侧生产面板 | UI/Panels/ProductionPanel |
| 4 | StatusBar | 底部状态栏 | UI/Panels/StatusBar |
| 5 | BuildingInfoPopup | 建筑信息弹窗 | UI/Popups/BuildingInfoPopup |
| 6 | VictoryScreen | 胜利界面 | UI/Screens/VictoryScreen |
| 7 | MainMenu | 主菜单 | UI/Screens/MainMenu |

---

## 八、样式规范

### 8.1 颜色方案

| 元素 | 颜色 | Hex值 |
|------|------|-------|
| 主背景 | 深空黑 | #0a0a1a |
| 面板背景 | 深蓝灰 | #1a1a2e |
| 边框高亮 | 科技蓝 | #00d4ff |
| 资源文字 | 白色 | #ffffff |
| 电力充足 | 绿色 | #00ff88 |
| 电力不足 | 红色 | #ff4444 |

### 8.2 字体规范

| 用途 | 字体大小 | 字体样式 |
|------|----------|----------|
| 标题 | 32px | 粗体 |
| 面板标题 | 20px | 粗体 |
| 内容文字 | 16px | 常规 |
| 资源数量 | 18px | 粗体 |

---

## 九、Unity实现步骤

### 9.1 创建UI Canvas

1. 在Hierarchy中右键 → UI → Canvas
2. 设置Render Mode为Screen Space - Overlay
3. 添加EventSystem

### 9.2 创建面板

1. 在Canvas下创建Panel作为容器
2. 设置RectTransform锚点和大小
3. 添加Image组件设置背景颜色

### 9.3 添加组件

1. 添加Text、Image、Button等组件
2. 设置组件属性
3. 绑定事件监听器

### 9.4 编写UI Manager

创建 `UIManager.cs` 统一管理所有UI面板的显示和隐藏。

---

## 十、与其他系统的交互

### 10.1 与游戏管理器的交互

```csharp
public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.OnResourceChanged += UpdateResources;
        GameManager.Instance.OnBuildingPlaced += UpdateBuildingList;
        GameManager.Instance.OnVictory += ShowVictoryScreen;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnResourceChanged -= UpdateResources;
        GameManager.Instance.OnBuildingPlaced -= UpdateBuildingList;
        GameManager.Instance.OnVictory -= ShowVictoryScreen;
    }
}
```

### 10.2 与建筑放置系统的交互

```csharp
public void OnBuildingSelected(BuildingType type)
{
    BuildingPlacer.Instance.SelectBuilding(type);
}
```

---

**文档版本**: v1.0  
**生成日期**: 2026年4月25日  
**项目**: 异星太空工厂