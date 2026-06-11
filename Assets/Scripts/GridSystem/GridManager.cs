using UnityEngine;
using System.Collections.Generic;
using GameCore;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")]
        public int gridWidth = 100;
        public int gridHeight = 100;
        public float cellSize = 1f;
        public Color gridColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Header("Debug Settings")]
        public bool showDebugInfo = false;
        public bool showGridCoordinates = false;
        public Color textColor = Color.white;
        public float textScale = 0.3f;

        [Header("References")]
        public Transform buildingsContainer;

        [Header("Board Settings")]
        public Color boardColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        public Color boardHighlightColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        public Color boardInvalidColor = new Color(0.3f, 0.1f, 0.1f, 0.8f);

        private Dictionary<GridPosition, GridCell> grid = new Dictionary<GridPosition, GridCell>();
        private Dictionary<GridPosition, PlacedBuilding> placedBuildings = new Dictionary<GridPosition, PlacedBuilding>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeContainer();
                InitializeGrid();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeContainer()
        {
            if (buildingsContainer == null)
            {
                GameObject containerObj = new GameObject("Buildings");
                buildingsContainer = containerObj.transform;
            }
        }

        private void InitializeGrid()
        {
            grid.Clear();
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GridCell cell = new GridCell(new GridPosition(x, y));
                    grid[new GridPosition(x, y)] = cell;
                }
            }
        }

        public Vector3 WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector3(
                Mathf.Floor(worldPosition.x / cellSize),
                Mathf.Floor(worldPosition.y / cellSize),
                0f
            );
        }

        public GridPosition WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.y / cellSize);
            return new GridPosition(x, y);
        }

        public Vector3 GridToWorld(GridPosition gridPos)
        {
            return new Vector3(
                gridPos.x * cellSize + cellSize / 2f,
                gridPos.y * cellSize + cellSize / 2f,
                0f
            );
        }

        public Vector3 GridToWorldWithOffset(GridPosition gridPos, int width, int height)
        {
            Vector3 worldPos = GridToWorld(gridPos);
            float offsetX = (width - 1) * cellSize / 2f;
            float offsetY = (height - 1) * cellSize / 2f;
            worldPos.x += offsetX;
            worldPos.y += offsetY;
            return worldPos;
        }

        public GridCell GetCell(GridPosition pos)
        {
            if (grid.TryGetValue(pos, out GridCell cell))
                return cell;
            return new GridCell(pos);
        }

        public bool HasCell(GridPosition pos)
        {
            return grid.ContainsKey(pos);
        }

        public bool PlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            return PlaceBuildingWithDirection(position, buildingType, BuildDirection.East);
        }

        public bool RemoveBuilding(GridPosition position)
        {
            if (!placedBuildings.ContainsKey(position))
                return false;

            PlacedBuilding placed = placedBuildings[position];
            var buildingDef = placed.Definition;

            if (buildingDef != null && buildingDef.isCoreBuilding)
            {
                Debug.Log($"[GridManager] Cannot remove core building: {buildingDef.name}");
                return false;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 移除建筑: {buildingDef.name}");
                Debug.Log($"[GridManager] 位置: ({position.x}, {position.y})");
            }

            ClearCellsForBuilding(position, placed);
            placed.Destroy();
            placedBuildings.Remove(position);
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 建筑移除成功");
                Debug.Log("-----------------------------");
            }
            
            return true;
        }

        /// <summary>
        /// 清除所有建筑（用于加载存档）
        /// </summary>
        public void ClearAllBuildings()
        {
            foreach (var kvp in placedBuildings)
            {
                kvp.Value.Destroy();
            }
            placedBuildings.Clear();
            grid.Clear();
            Debug.Log("[GridManager] 所有建筑已清除");
        }

        public bool CanPlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;
            return CanPlaceBuildingWithDirection(position, buildingType, buildingDef.direction);
        }

        private bool IsPartOfBuilding(GridPosition checkPos, GridPosition buildingOrigin, PlacedBuilding building)
        {
            if (building == null) return false;
            int endX = buildingOrigin.x + building.DisplayWidth;
            int endY = buildingOrigin.y + building.DisplayHeight;
            return checkPos.x >= buildingOrigin.x && checkPos.x < endX &&
                   checkPos.y >= buildingOrigin.y && checkPos.y < endY;
        }

        private bool HasAdjacentBoard(GridPosition position, GridPosition ignoreBuilding)
        {
            GridPosition[] neighbors = new GridPosition[]
            {
                position.Offset(-1, 0),
                position.Offset(1, 0),
                position.Offset(0, -1),
                position.Offset(0, 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (GetCell(neighbor).HasBoard && !IsPartOfBuilding(neighbor, ignoreBuilding, GetBuildingByOrigin(ignoreBuilding)))
                {
                    return true;
                }
            }

            return false;
        }

        private PlacedBuilding GetBuildingByOrigin(GridPosition origin)
        {
            placedBuildings.TryGetValue(origin, out var building);
            return building;
        }

        public void UpdateBuildingCells(GridPosition position, BuildingType buildingType, BuildDirection newDirection)
        {
            if (!placedBuildings.TryGetValue(position, out PlacedBuilding placed))
                return;

            var buildingDef = placed.Definition;
            if (buildingDef == null) return;

            ClearCellsForBuilding(position, placed);

            placed.Rotate(newDirection);

            MarkCellsForBuilding(position, placed);
        }

        private void ClearCellsForBuilding(GridPosition origin, PlacedBuilding placed)
        {
            var buildingDef = placed.Definition;
            if (buildingDef == null) return;

            int displayWidth = placed.DisplayWidth;
            int displayHeight = placed.DisplayHeight;

            for (int dx = 0; dx < displayWidth; dx++)
            {
                for (int dy = 0; dy < displayHeight; dy++)
                {
                    GridPosition cellPos = origin.Offset(dx, dy);
                    GridCell cell = GetCell(cellPos);

                    if (buildingDef.isBoard)
                    {
                        GridCell newCell = new GridCell(cellPos);
                        newCell.SetBoard(BoardType.None);
                        grid[cellPos] = newCell;
                    }
                    else
                    {
                        GridCell newCell = cell;
                        newCell.SetBuilding(null);
                        grid[cellPos] = newCell;
                    }
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                // Functional area display dimensions based on direction
                // East/West: width=8 (extends outward), height=2
                // North/South: width=2, height=8 (extends upward)
                int faWidth = placed.Direction == BuildDirection.North || placed.Direction == BuildDirection.South
                    ? buildingDef.functionalAreaWidth
                    : buildingDef.functionalAreaHeight;
                int faHeight = placed.Direction == BuildDirection.North || placed.Direction == BuildDirection.South
                    ? buildingDef.functionalAreaHeight
                    : buildingDef.functionalAreaWidth;

                switch (placed.Direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = displayHeight;
                        funcEndX = faWidth; funcEndY = displayHeight + faHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -faHeight;
                        funcEndX = faWidth; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = displayWidth; funcStartY = 0;
                        funcEndX = displayWidth + faWidth; funcEndY = faHeight;
                        break;
                    case BuildDirection.West:
                        funcStartX = -faWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = faHeight;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition cellPos = origin.Offset(dx, dy);
                        GridCell cell = GetCell(cellPos);
                        GridCell newCell = cell;
                        newCell.SetFunctionalArea(null);
                        grid[cellPos] = newCell;
                    }
                }
            }
        }

        private void MarkCellsForBuilding(GridPosition origin, PlacedBuilding placed)
        {
            var buildingDef = placed.Definition;
            if (buildingDef == null) return;

            int displayWidth = placed.DisplayWidth;
            int displayHeight = placed.DisplayHeight;

            for (int dx = 0; dx < displayWidth; dx++)
            {
                for (int dy = 0; dy < displayHeight; dy++)
                {
                    GridPosition cellPos = origin.Offset(dx, dy);
                    GridCell cell = GetCell(cellPos);

                    if (buildingDef.isBoard)
                    {
                        BoardType boardType = GetBoardType(placed.BuildingType);
                        GridCell newCell = cell;
                        newCell.SetBoard(boardType);
                        grid[cellPos] = newCell;
                    }
                    else
                    {
                        GridCell newCell = cell;
                        newCell.SetBuilding(placed);
                        grid[cellPos] = newCell;
                    }
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                // Functional area display dimensions based on direction
                // East/West: width=8 (extends outward), height=2
                // North/South: width=2, height=8 (extends upward)
                int faWidth = placed.Direction == BuildDirection.North || placed.Direction == BuildDirection.South
                    ? buildingDef.functionalAreaWidth
                    : buildingDef.functionalAreaHeight;
                int faHeight = placed.Direction == BuildDirection.North || placed.Direction == BuildDirection.South
                    ? buildingDef.functionalAreaHeight
                    : buildingDef.functionalAreaWidth;

                switch (placed.Direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = displayHeight;
                        funcEndX = faWidth; funcEndY = displayHeight + faHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -faHeight;
                        funcEndX = faWidth; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = displayWidth; funcStartY = 0;
                        funcEndX = displayWidth + faWidth; funcEndY = faHeight;
                        break;
                    case BuildDirection.West:
                        funcStartX = -faWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = faHeight;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition cellPos = origin.Offset(dx, dy);
                        GridCell cell = GetCell(cellPos);
                        GridCell newCell = cell;
                        newCell.SetFunctionalArea(placed);
                        grid[cellPos] = newCell;
                    }
                }
            }
        }

        private BoardType GetBoardType(BuildingType buildingType)
        {
            switch (buildingType)
            {
                case BuildingType.BasicBoard: return BoardType.BasicBoard;
                default: return BoardType.None;
            }
        }

        public bool IsValidPosition(GridPosition pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        public bool HasBoardAt(GridPosition position)
        {
            return GetCell(position).HasBoard;
        }

        public bool HasAdjacentBoard(GridPosition position)
        {
            GridPosition[] neighbors = new GridPosition[]
            {
                position.Offset(-1, 0),
                position.Offset(1, 0),
                position.Offset(0, -1),
                position.Offset(0, 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (GetCell(neighbor).HasBoard)
                    return true;
            }

            return false;
        }

        public bool CanPlaceBoard(GridPosition position)
        {
            if (GetCell(position).HasBoard)
                return false;

            if (GetAllBoardPositions().Count == 0)
                return true;

            return HasAdjacentBoard(position);
        }

        public BoardType GetBoardAt(GridPosition position)
        {
            return GetCell(position).BoardType;
        }

        public HashSet<GridPosition> GetAllBoardPositions()
        {
            HashSet<GridPosition> result = new HashSet<GridPosition>();
            foreach (var kvp in grid)
            {
                if (kvp.Value.HasBoard)
                    result.Add(kvp.Key);
            }
            return result;
        }

        public BuildingType GetBuildingAt(GridPosition pos)
        {
            GridCell cell = GetCell(pos);
            if (cell.HasBuilding)
                return cell.Building.BuildingType;
            if (cell.HasBoard)
                return GetBuildingTypeFromBoard(cell.BoardType);
            return BuildingType.None;
        }

        private BuildingType GetBuildingTypeFromBoard(BoardType boardType)
        {
            switch (boardType)
            {
                case BoardType.BasicBoard: return BuildingType.BasicBoard;
                default: return BuildingType.None;
            }
        }

        /// <summary>
        /// 获取所有有太空板的格子位置和板类型（用于存档）
        /// </summary>
        public Dictionary<GridPosition, BoardType> GetAllBoardCells()
        {
            var result = new Dictionary<GridPosition, BoardType>();
            foreach (var kvp in grid)
            {
                if (kvp.Value.HasBoard)
                {
                    result[kvp.Key] = kvp.Value.BoardType;
                }
            }
            return result;
        }

        public GridPosition GetBuildingOrigin(GridPosition pos)
        {
            GridCell cell = GetCell(pos);
            if (cell.HasBuilding)
                return cell.Building.OriginPosition;
            if (cell.HasBoard)
                return FindBoardOrigin(pos, cell.BoardType);
            return new GridPosition(-1, -1);
        }

        private GridPosition FindBoardOrigin(GridPosition pos, BoardType boardType)
        {
            foreach (var kvp in placedBuildings)
            {
                if (kvp.Value.IsBoard)
                {
                    var def = kvp.Value.Definition;
                    if (def == null) continue;
                    if (kvp.Value.Direction == BuildDirection.North || kvp.Value.Direction == BuildDirection.South)
                    {
                        for (int dx = 0; dx < def.height; dx++)
                            for (int dy = 0; dy < def.width; dy++)
                                if (kvp.Key.Offset(dx, dy) == pos)
                                    return kvp.Key;
                    }
                    else
                    {
                        for (int dx = 0; dx < def.width; dx++)
                            for (int dy = 0; dy < def.height; dy++)
                                if (kvp.Key.Offset(dx, dy) == pos)
                                    return kvp.Key;
                    }
                }
            }
            return new GridPosition(-1, -1);
        }

        public Dictionary<GridPosition, BuildingType> GetAllBuildings()
        {
            Dictionary<GridPosition, BuildingType> result = new Dictionary<GridPosition, BuildingType>();
            foreach (var kvp in placedBuildings)
            {
                result[kvp.Key] = kvp.Value.BuildingType;
            }
            return result;
        }

        public string GetPlacementFailureReason(GridPosition position, BuildingType buildingType)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return "建筑定义不存在";
            return GetPlacementFailureReasonWithDirection(position, buildingType, buildingDef.direction);
        }

        private (int width, int height) GetDisplayDimensions(BuildingDefinition def, BuildDirection direction)
        {
            if (direction == BuildDirection.North || direction == BuildDirection.South)
                return (def.height, def.width);
            return (def.width, def.height);
        }

        public string GetPrefabPathForDirection(BuildingType type, BuildDirection direction)
        {
            var def = DataConfig.GetBuilding(type);
            if (def == null) return "";
            return def.GetPrefabPath(direction);
        }

        public bool CanPlaceBuildingWithDirection(GridPosition position, BuildingType buildingType, BuildDirection direction)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;

            (int displayWidth, int displayHeight) = GetDisplayDimensions(buildingDef, direction);

            if (!buildingDef.isBoard)
            {
                for (int dx = 0; dx < displayWidth; dx++)
                {
                    for (int dy = 0; dy < displayHeight; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        GridCell cell = GetCell(checkPos);
                        
                        if (cell.HasBuilding)
                            return false;
                        
                        if (cell.IsFunctionalArea)
                            return false;

                        if (!cell.HasBoard)
                            return false;
                    }
                }
            }
            else
            {
                for (int dx = 0; dx < displayWidth; dx++)
                {
                    for (int dy = 0; dy < displayHeight; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        GridCell cell = GetCell(checkPos);
                        
                        if (cell.HasBuilding)
                            return false;
                        
                        if (cell.HasBoard)
                            return false;
                        
                        if (GetAllBoardPositions().Count > 0)
                        {
                            if (!HasAdjacentBoard(checkPos, position))
                                return false;
                        }
                    }
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                // Functional area display dimensions based on direction
                // East/West: width=8 (extends outward), height=2
                // North/South: width=2, height=8 (extends upward)
                int faWidth = direction == BuildDirection.North || direction == BuildDirection.South
                    ? buildingDef.functionalAreaWidth
                    : buildingDef.functionalAreaHeight;
                int faHeight = direction == BuildDirection.North || direction == BuildDirection.South
                    ? buildingDef.functionalAreaHeight
                    : buildingDef.functionalAreaWidth;

                switch (direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = displayHeight;
                        funcEndX = faWidth; funcEndY = displayHeight + faHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -faHeight;
                        funcEndX = faWidth; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = displayWidth; funcStartY = 0;
                        funcEndX = displayWidth + faWidth; funcEndY = faHeight;
                        break;
                    case BuildDirection.West:
                        funcStartX = -faWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = faHeight;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        GridCell cell = GetCell(checkPos);
                        
                        if (cell.HasBuilding)
                            return false;

                        if (cell.HasBoard)
                            return false;
                    }
                }
            }

            return true;
        }

        public bool PlaceBuildingWithDirection(GridPosition position, BuildingType buildingType, BuildDirection direction)
        {
            return PlaceBuildingWithDirection(position, buildingType, direction, validate: true);
        }

        public bool PlaceBuildingWithDirection(GridPosition position, BuildingType buildingType, BuildDirection direction, bool validate)
        {
            if (validate && !CanPlaceBuildingWithDirection(position, buildingType, direction))
                return false;

            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;

            (int displayWidth, int displayHeight) = GetDisplayDimensions(buildingDef, direction);

            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 开始放置建筑: {buildingDef.name}, 方向: {direction}");
                Debug.Log($"[GridManager] 放置位置: 网格坐标 = ({position.x}, {position.y})");
                Debug.Log($"[GridManager] 显示大小: {displayWidth} x {displayHeight}");
            }

            GameObject buildingObj = null;

            string prefabPath = GetPrefabPathForDirection(buildingType, direction);
            if (!string.IsNullOrEmpty(prefabPath))
            {
                GameObject prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab != null)
                {
                    buildingObj = Instantiate(prefab);
                    if (showDebugInfo)
                        Debug.Log($"[GridManager] 成功加载建筑预制体: {prefabPath}");
                }
                else
                {
                    Debug.LogWarning($"[GridManager] 无法加载建筑预制体: {prefabPath}，将使用默认预制体");
                    if (!string.IsNullOrEmpty(buildingDef.prefabPath))
                    {
                        GameObject fallback = Resources.Load<GameObject>(buildingDef.prefabPath);
                        if (fallback != null)
                            buildingObj = Instantiate(fallback);
                    }
                }
            }

            if (buildingObj == null && !string.IsNullOrEmpty(buildingDef.prefabPath))
            {
                GameObject prefab = Resources.Load<GameObject>(buildingDef.prefabPath);
                if (prefab != null)
                    buildingObj = Instantiate(prefab);
            }

            if (buildingObj == null)
            {
                buildingObj = CreateDefaultBuilding(buildingDef);
            }

            BuildingComponent component = null;
            if (buildingObj != null)
            {
                Vector3 worldPos = new Vector3(
                    position.x * cellSize + displayWidth * cellSize / 2f,
                    position.y * cellSize + displayHeight * cellSize / 2f,
                    0f
                );
                
                buildingObj.transform.position = worldPos;
                buildingObj.transform.SetParent(buildingsContainer);
                buildingObj.name = $"{buildingDef.name}_{position.x}_{position.y}";

                component = buildingObj.GetComponent<BuildingComponent>();
                if (component != null)
                {
                    component.Initialize(position);
                    component.Activate();
                }
                else
                {
                    component = buildingObj.AddComponent<BuildingComponent>();
                    component.Initialize(position);
                    component.Activate();
                }
            }

            PlacedBuilding placed = new PlacedBuilding(buildingType, position, direction, buildingObj, component);
            placedBuildings[position] = placed;
            MarkCellsForBuilding(position, placed);
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 建筑放置成功: {buildingDef.name}");
                Debug.Log("-----------------------------");
            }
            
            return true;
        }

        public PlacedBuilding SwapBuildingPrefab(GridPosition origin, BuildDirection newDirection)
        {
            if (!placedBuildings.TryGetValue(origin, out PlacedBuilding placed))
                return null;

            string newPrefabPath = GetPrefabPathForDirection(placed.BuildingType, newDirection);
            if (string.IsNullOrEmpty(newPrefabPath))
            {
                placed.Rotate(newDirection);
                return placed;
            }

            GameObject newPrefab = Resources.Load<GameObject>(newPrefabPath);
            if (newPrefab == null)
            {
                placed.Rotate(newDirection);
                return placed;
            }

            Vector3 oldPosition = placed.WorldPosition;

            placed.Destroy();

            GameObject newObj = Instantiate(newPrefab);
            newObj.transform.position = oldPosition;
            newObj.transform.SetParent(buildingsContainer);

            var def = placed.Definition;
            newObj.name = $"{def.name}_{origin.x}_{origin.y}";

            BuildingComponent newComp = newObj.GetComponent<BuildingComponent>();
            if (newComp == null)
            {
                newComp = newObj.AddComponent<BuildingComponent>();
            }
            newComp.Initialize(origin);
            newComp.Activate();

            placed = new PlacedBuilding(placed.BuildingType, origin, newDirection, newObj, newComp);
            placedBuildings[origin] = placed;

            return placed;
        }

        private GameObject CreateDefaultBuilding(BuildingDefinition buildingDef)
        {
            GameObject buildingObj = new GameObject(buildingDef.name);

            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.transform.SetParent(buildingObj.transform);
            
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(buildingDef.width * cellSize * 0.9f, buildingDef.height * cellSize * 0.9f, 0.5f);

            Renderer renderer = visualObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                switch (buildingDef.category)
                {
                    case BuildingCategory.Core:        renderer.material.color = new Color(0.8f, 0.4f, 0.8f); break;
                    case BuildingCategory.Power:       renderer.material.color = new Color(0.4f, 0.8f, 0.4f); break;
                    case BuildingCategory.Production:  renderer.material.color = new Color(0.8f, 0.6f, 0.4f); break;
                    case BuildingCategory.Storage:     renderer.material.color = new Color(0.6f, 0.6f, 0.6f); break;
                    case BuildingCategory.Propulsion:  renderer.material.color = new Color(1f, 0.5f, 0.2f); break;
                    case BuildingCategory.Board:       renderer.material.color = new Color(0.3f, 0.3f, 0.3f); break;
                }
            }

            return buildingObj;
        }

        public string GetPlacementFailureReasonWithDirection(GridPosition position, BuildingType buildingType, BuildDirection direction)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return "建筑定义不存在";

            (int displayWidth, int displayHeight) = GetDisplayDimensions(buildingDef, direction);

            for (int dx = 0; dx < displayWidth; dx++)
            {
                for (int dy = 0; dy < displayHeight; dy++)
                {
                    GridPosition checkPos = position.Offset(dx, dy);
                    GridCell cell = GetCell(checkPos);
                    
                    if (cell.HasBuilding)
                        return $"与已有建筑重叠: ({checkPos.x}, {checkPos.y})";
                        
                    if (cell.IsFunctionalArea)
                        return $"与功能区域重叠: ({checkPos.x}, {checkPos.y})";

                    if (!buildingDef.isBoard && !cell.HasBoard)
                        return $"缺少太空板支撑: ({checkPos.x}, {checkPos.y})";
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                // Functional area display dimensions based on direction
                // East/West: width=8 (extends outward), height=2
                // North/South: width=2, height=8 (extends upward)
                int faWidth = direction == BuildDirection.North || direction == BuildDirection.South
                    ? buildingDef.functionalAreaWidth
                    : buildingDef.functionalAreaHeight;
                int faHeight = direction == BuildDirection.North || direction == BuildDirection.South
                    ? buildingDef.functionalAreaHeight
                    : buildingDef.functionalAreaWidth;

                switch (direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = displayHeight;
                        funcEndX = faWidth; funcEndY = displayHeight + faHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -faHeight;
                        funcEndX = faWidth; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = displayWidth; funcStartY = 0;
                        funcEndX = displayWidth + faWidth; funcEndY = faHeight;
                        break;
                    case BuildDirection.West:
                        funcStartX = -faWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = faHeight;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        GridCell cell = GetCell(checkPos);
                        
                        if (cell.HasBuilding)
                            return $"功能区域与已有建筑重叠: ({checkPos.x}, {checkPos.y})";

                        if (cell.HasBoard)
                            return $"功能区域与太空板重叠: ({checkPos.x}, {checkPos.y})";
                    }
                }
            }

            return "";
        }

        private Vector3 GetRotationEuler(BuildDirection direction)
        {
            switch (direction)
            {
                case BuildDirection.East:   return Vector3.zero;
                case BuildDirection.South:  return new Vector3(0, 0, -90);
                case BuildDirection.West:   return new Vector3(0, 0, -180);
                case BuildDirection.North:  return new Vector3(0, 0, -270);
                default:                    return Vector3.zero;
            }
        }

        public PlacedBuilding GetPlacedBuildingAt(GridPosition origin)
        {
            placedBuildings.TryGetValue(origin, out var building);
            return building;
        }

        public Dictionary<GridPosition, PlacedBuilding> GetAllPlacedBuildings()
        {
            return new Dictionary<GridPosition, PlacedBuilding>(placedBuildings);
        }

        private void OnDrawGizmos()
        {
            DrawGrid();
            
            if (showGridCoordinates && Application.isPlaying)
            {
                DrawGridCoordinates();
            }
        }

        private void DrawGridCoordinates()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = textColor;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = Mathf.RoundToInt(12 * textScale);
        }

        private void DrawGrid()
        {
            Gizmos.color = gridColor;
            
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = new Vector3(x * cellSize, 0, 0);
                Vector3 end = new Vector3(x * cellSize, gridHeight * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
            
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = new Vector3(0, y * cellSize, 0);
                Vector3 end = new Vector3(gridWidth * cellSize, y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
