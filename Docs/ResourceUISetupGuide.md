# 资源UI配置指南

## 目标
在游戏场景顶部显示4种资源的图标和数量：太空矿石、金属材料、初级零件、高级零件。

---

## 第一步：准备资源图标

### 1.1 图标文件准备
在项目目录 `Assets/Resources/Icons/Resources/` 下放置以下4张图片：

| 文件名 | 对应资源 |
|-------|---------|
| `SpaceOre.png` | 太空矿石 |
| `MetalMaterial.png` | 金属材料 |
| `BasicPart.png` | 初级零件 |
| `AdvancedPart.png` | 高级零件 |

### 1.2 Unity导入设置
选中每张图片，在Inspector中设置：
- **Texture Type**: Sprite (2D and UI)
- **Sprite Mode**: Single
- **Pixels Per Unit**: 100
- **Compression**: None

---

## 第二步：创建UI层级

### 2.1 在GameScene中创建结构

```
GameCanvas (已存在)
├── ResourceBarPanel (新建 Panel)
│   ├── Background (新建 Image，可选背景图)
│   └── ResourceContainer (新建空GameObject) ← 条目将在这里生成
── ResourceDisplay (新建空GameObject) ← 挂载主脚本
```

### 2.2 创建步骤

1. 在 Hierarchy 中选中 `GameCanvas`
2. 右键 → UI → Panel，命名为 `ResourceBarPanel`
3. 选中 `ResourceBarPanel`，设置：
   - **Rect Transform**:
     - Anchors: Stretch Top (顶部拉伸)
     - Left: 0, Right: 0, Top: 0, Height: 60
     - Pivot: (0.5, 1)

4. 在 `ResourceBarPanel` 下创建 `Background` (Image，可选)
5. 在 `ResourceBarPanel` 下创建空GameObject，命名为 `ResourceContainer`
6. 在 `GameCanvas` 下创建空GameObject，命名为 `ResourceDisplay`

---

## 第三步：配置 ResourceContainer

### 3.1 添加 HorizontalLayoutGroup

选中 `ResourceContainer`，在 Inspector 中：
1. Add Component → Layout → **HorizontalLayoutGroup**
2. 设置：
   - **Child Alignment**: Middle Left
   - **Child Force Expand**: Width 取消勾选，Height 勾选
   - **Spacing**: 10
   - **Padding**: Left=10, Right=10, Top=5, Bottom=5

### 3.2 添加 ContentSizeFitter (可选)

Add Component → Layout → **ContentSizeFitter**
- Horizontal Fit: Preferred Size
- Vertical Fit: Unconstrained

---

## 第四步：挂载 ResourceDisplayUI 脚本

### 4.1 添加脚本

选中 `ResourceDisplay` GameObject：
1. Add Component → 搜索 **ResourceDisplayUI**
2. 点击添加

### 4.2 配置 Inspector 字段

| 字段 | 配置方法 |
|-----|---------|
| **Container** | 拖入 `ResourceContainer` 的 RectTransform |
| **Resource Entry Prefab** | 留空（不填会自动生成） |
| **Space Ore Icon** | 拖入 `SpaceOre.png` 的Sprite |
| **Metal Material Icon** | 拖入 `MetalMaterial.png` 的Sprite |
| **Basic Part Icon** | 拖入 `BasicPart.png` 的Sprite |
| **Advanced Part Icon** | 拖入 `AdvancedPart.png` 的Sprite |
| **Resource Font** | 拖入项目字体（如 JazzCreateBubble） |
| **Text Color** | 白色 #FFFFFF |
| **Font Size** | 16 |
| **Entry Spacing** | 10 |
| **Entry Width** | 180 |
| **Entry Height** | 50 |
| **Icon Size** | 36 |

---

## 第五步：测试运行

### 5.1 运行游戏
1. 点击 Unity 编辑器顶部的 **Play** 按钮
2. 观察屏幕顶部是否出现4个资源条目

### 5.2 预期效果

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│  ┌──────────┐ ┌────────── ┌──────────┐ ┌──────────┐  │
│  │        │ │          │ │          │ │          │  │
│  │ 太空矿石    │ │ 金属材料    │ │ 初级零件    │ │ 高级零件    │  │
│  │ : 50     │ │ : 0      │ │ : 0      │ │ : 0      │  │
│  └──────────┘ ──────────┘ └──────────┘ ──────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 5.3 验证功能
- 放置采矿平台后，太空矿石数量应自动增加
- 点击作弊按钮，所有资源数量应变为9999

---

## 第六步：可选优化

### 6.1 创建预制体（如果需要自定义样式）

1. 在 `ResourceContainer` 下创建一个资源条目模板
2. 结构调整：
```
ResourceEntryPrefab (GameObject)
├── Image (背景，半透明深色)
├── Icon (Image) - 子对象，左侧
├── Text (Text) - 子对象，右侧
└── ResourceEntryUI (脚本)
```

3. 拖入 `Assets/Prefab/UI/ResourceEntryPrefab.prefab`
4. 在 `ResourceDisplayUI` 的 **Resource Entry Prefab** 字段中拖入该预制体

### 6.2 调整位置

如果资源栏遮挡游戏内容，调整 `ResourceBarPanel` 的 Height 值：
- 推荐值: 50-70 像素

---

## 常见问题

### Q: 资源条目不显示
**A**: 检查以下项：
1. `Container` 字段是否正确拖入了 `ResourceContainer`
2. 图标Sprite是否正确拖入4个Icon字段
3. `ResourceDisplay` GameObject是否处于激活状态

### Q: 资源数量不更新
**A**: 检查：
1. `GameManager.Instance` 是否存在
2. 采矿平台是否正确采集到矿石

### Q: 文字显示为方块
**A**: 检查 `Resource Font` 字段是否拖入了包含中文字符的字体

---

## 文件清单

### 已存在的脚本
| 文件路径 | 说明 |
|---------|------|
| `Assets/Scripts/UI/ResourceDisplayUI.cs` | 资源显示主控制器 |
| `Assets/Scripts/UI/ResourceEntryUI.cs` | 单个资源条目管理 |

### 需要准备的资源
| 路径 | 说明 |
|-----|------|
| `Assets/Resources/Icons/Resources/SpaceOre.png` | 太空矿石图标 |
| `Assets/Resources/Icons/Resources/MetalMaterial.png` | 金属材料图标 |
| `Assets/Resources/Icons/Resources/BasicPart.png` | 初级零件图标 |
| `Assets/Resources/Icons/Resources/AdvancedPart.png` | 高级零件图标 |

---

## 完成

按照以上步骤操作后，你的游戏顶部将显示4种资源的图标和数量，并自动更新。
