# 建筑图标和Prefab绑定说明

## 已完成的工作

1. ✅ 更新 [DataConfig.cs](Assets/Scripts/GameCore/DataConfig.cs) - 为每个建筑添加了 `prefabPath` 配置
2. ✅ 更新 [GridManager.cs](Assets/Scripts/GridSystem/GridManager.cs) - 支持从 Resources 加载并实例化 Prefab
3. ✅ 更新 [BuildingPlacer.cs](Assets/Scripts/GridSystem/BuildingPlacer.cs) - 支持动态调整预览大小
4. ✅ 创建 [BuildingPrefabGenerator.cs](Assets/Scripts/Editor/BuildingPrefabGenerator.cs) - 一键生成预制体工具
5. ✅ 创建文件夹结构 - `Assets/Resources/Prefabs/Buildings`

## 在Unity中的设置步骤

### 1. 生成建筑预制体

在Unity编辑器中执行：
`菜单栏 → Tools → 生成建筑预制体`

这将自动：
- 为所有建筑类型生成预制体
- 配置 BuildingComponent 组件
- 创建预览预制体

### 2. 配置 GridManager

在场景中选中 `GridManager` 对象，配置：
- `buildingsContainer` 可以留空（会自动创建）
- 其他参数保持默认即可

### 3. 配置 BuildingPlacer

在场景中选中 `BuildingPlacer` 对象，配置：
- `previewPrefab` → 拖拽 `Assets/Resources/Prefabs/BuildingPreview.prefab`
- `mainCamera` → 场景中的主摄像机

### 4. 配置 BuildingCategoryPanel（建筑分类面板）

每个分类面板需要：
- `category` → 选择对应分类
- `categoryButton` → 折叠/展开按钮
- `categoryNameText` → 显示分类名称
- `contentContainer` → 图标容器（Viewport 的 Content）
- `buildingIconPrefab` → 建筑图标预制体
- `scrollView` → ScrollView 对象
- `scrollRect` → ScrollRect 组件
- `iconsPerRow` → 每行图标数（推荐4）

### 5. 创建建筑图标预制体（如果还没有）

如果你还没有建筑图标预制体，按以下步骤创建：
1. 创建 Button UI 组件
2. 添加 BuildingIconButton 脚本
3. 创建子对象：
   - `IconImage` (Image 组件) - 显示建筑图标
   - `BuildingNameText` (Text 组件) - 显示建筑名称
   - `SelectedIndicator` (Image 组件) - 选中边框（可选）
   - `LockedOverlay` (Image 组件) - 锁定遮罩（可选）
4. 在脚本中配置各个组件的引用
5. 保存为预制体到 `Assets/Prefabs/UI`

## 使用说明

### 运行游戏后
1. 按 **B 键** 打开建筑UI
2. 点击建筑图标选中
3. 鼠标移动显示预览（绿色=可放置，红色=不可放置）
4. **左键点击** 放置建筑
5. **右键** 或 **ESC** 取消放置
6. **右键** 选中已放置建筑可以拆除

### 建筑分类颜色
- 核心设施 - 紫色
- 能源设施 - 绿色
- 生产设施 - 橙色
- 物流设施 - 蓝色
- 仓储设施 - 灰色
- 特殊设施 - 金色

## 注意事项

1. **预制体必须在 Resources 文件夹下** - 这样才能通过 Resources.Load 加载
2. **buildingIconPrefab 的 IconImage 需要保持空** - 运行时会动态加载
3. **图标资源路径需要正确** - 放在 `Assets/Resources/Icons/Buildings` 下
4. **GridManager 的 cellSize 需要与场景匹配** - 默认1.0f

## 文件结构

```
Assets/
├── Resources/
│   ├── Icons/
│   │   └── Buildings/          # 建筑图标
│   └── Prefabs/
│       ├── Buildings/          # 建筑预制体（自动生成）
│       └── BuildingPreview.prefab  # 预览预制体
├── Prefabs/
│   └── UI/
│       └── BuildingIconButton.prefab  # UI图标按钮
└── Scripts/
    ├── Editor/
    │   └── BuildingPrefabGenerator.cs
    ├── GameCore/
    └── GridSystem/
```

## 自定义建筑

要添加新建筑：
1. 在 Enums.cs 添加新的 BuildingType
2. 在 DataConfig.cs 的 InitializeBuildings 添加配置
3. 运行 `Tools → 生成建筑预制体`
4. （可选）手动美化预制体的视觉效果
