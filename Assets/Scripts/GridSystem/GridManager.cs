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

        private HashSet<GridPosition> gridCells = new HashSet<GridPosition>();
        private HashSet<GridPosition> boardCells = new HashSet<GridPosition>();
        private HashSet<GridPosition> functionalAreaCells = new HashSet<GridPosition>();
        private Dictionary<GridPosition, BuildingType> placedBuildings = new Dictionary<GridPosition, BuildingType>();
        private Dictionary<GridPosition, GameObject> placedBuildingObjects = new Dictionary<GridPosition, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeContainer();
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

        public bool CanPlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;

            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition checkPos = position.Offset(dx, dy);
                    
                    if (gridCells.Contains(checkPos))
                        return false;
                        
                    if (functionalAreaCells.Contains(checkPos))
                        return false;

                    if (!buildingDef.isBoard && !boardCells.Contains(checkPos))
                        return false;
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = buildingDef.functionalAreaWidth;
                int funcEndY = buildingDef.functionalAreaHeight;

                switch (buildingDef.direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0;
                        funcStartY = buildingDef.height;
                        funcEndX = buildingDef.width;
                        funcEndY = buildingDef.height + buildingDef.functionalAreaHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0;
                        funcStartY = -buildingDef.functionalAreaHeight;
                        funcEndX = buildingDef.width;
                        funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = buildingDef.width;
                        funcStartY = 0;
                        funcEndX = buildingDef.width + buildingDef.functionalAreaWidth;
                        funcEndY = buildingDef.height;
                        break;
                    case BuildDirection.West:
                        funcStartX = -buildingDef.functionalAreaWidth;
                        funcStartY = 0;
                        funcEndX = 0;
                        funcEndY = buildingDef.height;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        
                        if (gridCells.Contains(checkPos))
                            return false;
                    }
                }
            }

            return true;
        }

        public bool PlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            if (!CanPlaceBuilding(position, buildingType))
                return false;

            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;

            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 开始放置建筑: {buildingDef.name}");
                Debug.Log($"[GridManager] 放置位置: 网格坐标 = ({position.x}, {position.y})");
                Debug.Log($"[GridManager] 建筑大小: {buildingDef.width} x {buildingDef.height}");
            }

            GameObject buildingObj = null;

            if (!string.IsNullOrEmpty(buildingDef.prefabPath))
            {
                GameObject prefab = Resources.Load<GameObject>(buildingDef.prefabPath);
                if (prefab != null)
                {
                    buildingObj = Instantiate(prefab);
                    if (showDebugInfo)
                        Debug.Log($"[GridManager] 成功加载建筑预制体: {buildingDef.prefabPath}");
                }
                else
                {
                    Debug.LogWarning($"[GridManager] 无法加载建筑预制体: {buildingDef.prefabPath}，将使用简单几何体");
                }
            }

            if (buildingObj == null)
            {
                buildingObj = CreateDefaultBuilding(buildingDef);
            }

            if (buildingObj != null)
            {
                Vector3 worldPos = new Vector3(
                    position.x * cellSize + buildingDef.width * cellSize / 2f,
                    position.y * cellSize + buildingDef.height * cellSize / 2f,
                    0f
                );
                
                if (showDebugInfo)
                {
                    Debug.Log($"[GridManager] 世界坐标: ({worldPos.x:F2}, {worldPos.y:F2})");
                    Debug.Log($"[GridManager] 左下角格子: ({position.x}, {position.y})");
                    Debug.Log($"[GridManager] 右上角格子: ({position.x + buildingDef.width - 1}, {position.y + buildingDef.height - 1})");
                }
                
                buildingObj.transform.position = worldPos;
                buildingObj.transform.SetParent(buildingsContainer);
                buildingObj.name = $"{buildingDef.name}_{position.x}_{position.y}";

                BuildingComponent component = buildingObj.GetComponent<BuildingComponent>();
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

                placedBuildingObjects[position] = buildingObj;
            }

            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition cellPos = position.Offset(dx, dy);
                    
                    if (buildingDef.isBoard)
                    {
                        boardCells.Add(cellPos);
                    }
                    else
                    {
                        gridCells.Add(cellPos);
                    }
                    
                    if (buildingDef.isBoard && BoardManager.Instance != null)
                    {
                        BoardManager.Instance.RegisterBoardPosition(cellPos);
                    }
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                switch (buildingDef.direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = buildingDef.height;
                        funcEndX = buildingDef.width; funcEndY = buildingDef.height + buildingDef.functionalAreaHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -buildingDef.functionalAreaHeight;
                        funcEndX = buildingDef.width; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = buildingDef.width; funcStartY = 0;
                        funcEndX = buildingDef.width + buildingDef.functionalAreaWidth; funcEndY = buildingDef.height;
                        break;
                    case BuildDirection.West:
                        funcStartX = -buildingDef.functionalAreaWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = buildingDef.height;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition cellPos = position.Offset(dx, dy);
                        functionalAreaCells.Add(cellPos);
                    }
                }
            }

            placedBuildings[position] = buildingType;
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 建筑放置成功: {buildingDef.name}");
                Debug.Log("-----------------------------");
            }
            
            return true;
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
                    case BuildingCategory.Core:
                        renderer.material.color = new Color(0.8f, 0.4f, 0.8f);
                        break;
                    case BuildingCategory.Power:
                        renderer.material.color = new Color(0.4f, 0.8f, 0.4f);
                        break;
                    case BuildingCategory.Production:
                        renderer.material.color = new Color(0.8f, 0.6f, 0.4f);
                        break;
                    case BuildingCategory.Logistics:
                        renderer.material.color = new Color(0.4f, 0.6f, 0.8f);
                        break;
                    case BuildingCategory.Storage:
                        renderer.material.color = new Color(0.6f, 0.6f, 0.6f);
                        break;
                    case BuildingCategory.Special:
                        renderer.material.color = new Color(1f, 0.8f, 0.2f);
                        break;
                    case BuildingCategory.Board:
                        renderer.material.color = new Color(0.3f, 0.3f, 0.3f);
                        break;
                }
            }

            return buildingObj;
        }

        public bool RemoveBuilding(GridPosition position)
        {
            if (!placedBuildings.ContainsKey(position))
                return false;

            var buildingType = placedBuildings[position];
            var buildingDef = DataConfig.GetBuilding(buildingType);
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 移除建筑: {buildingDef.name}");
                Debug.Log($"[GridManager] 位置: ({position.x}, {position.y})");
            }
            
            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition cellPos = position.Offset(dx, dy);
                    
                    if (buildingDef.isBoard)
                    {
                        boardCells.Remove(cellPos);
                    }
                    else
                    {
                        gridCells.Remove(cellPos);
                    }
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                switch (buildingDef.direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = buildingDef.height;
                        funcEndX = buildingDef.width; funcEndY = buildingDef.height + buildingDef.functionalAreaHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -buildingDef.functionalAreaHeight;
                        funcEndX = buildingDef.width; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = buildingDef.width; funcStartY = 0;
                        funcEndX = buildingDef.width + buildingDef.functionalAreaWidth; funcEndY = buildingDef.height;
                        break;
                    case BuildDirection.West:
                        funcStartX = -buildingDef.functionalAreaWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = buildingDef.height;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition cellPos = position.Offset(dx, dy);
                        functionalAreaCells.Remove(cellPos);
                    }
                }
            }

            if (placedBuildingObjects.ContainsKey(position))
            {
                Destroy(placedBuildingObjects[position]);
                placedBuildingObjects.Remove(position);
            }

            placedBuildings.Remove(position);
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] 建筑移除成功");
                Debug.Log("-----------------------------");
            }
            
            return true;
        }

        public bool IsValidPosition(GridPosition pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        public void RegisterBoardCell(GridPosition position)
        {
            boardCells.Add(position);
        }

        public void UnregisterBoardCell(GridPosition position)
        {
            boardCells.Remove(position);
        }

        public bool HasBoardAt(GridPosition position)
        {
            return boardCells.Contains(position);
        }

        public BuildingType GetBuildingAt(GridPosition pos)
        {
            foreach (var entry in placedBuildings)
            {
                var buildingDef = DataConfig.GetBuilding(entry.Value);
                if (buildingDef != null)
                {
                    int endX = entry.Key.x + buildingDef.width;
                    int endY = entry.Key.y + buildingDef.height;
                    
                    if (pos.x >= entry.Key.x && pos.x < endX &&
                        pos.y >= entry.Key.y && pos.y < endY)
                    {
                        return entry.Value;
                    }
                }
            }
            return BuildingType.None;
        }

        public GridPosition GetBuildingOrigin(GridPosition pos)
        {
            foreach (var entry in placedBuildings)
            {
                var buildingDef = DataConfig.GetBuilding(entry.Value);
                if (buildingDef != null)
                {
                    int endX = entry.Key.x + buildingDef.width;
                    int endY = entry.Key.y + buildingDef.height;
                    
                    if (pos.x >= entry.Key.x && pos.x < endX &&
                        pos.y >= entry.Key.y && pos.y < endY)
                    {
                        return entry.Key;
                    }
                }
            }
            return new GridPosition(-1, -1);
        }

        public Dictionary<GridPosition, BuildingType> GetAllBuildings()
        {
            return new Dictionary<GridPosition, BuildingType>(placedBuildings);
        }

        public string GetPlacementFailureReason(GridPosition position, BuildingType buildingType)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return "建筑定义不存在";

            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition checkPos = position.Offset(dx, dy);
                    
                    if (gridCells.Contains(checkPos))
                        return $"与已有建筑重叠: ({checkPos.x}, {checkPos.y})";
                        
                    if (functionalAreaCells.Contains(checkPos))
                        return $"与功能区域重叠: ({checkPos.x}, {checkPos.y})";

                    if (!buildingDef.isBoard && !boardCells.Contains(checkPos))
                        return $"缺少太空板支撑: ({checkPos.x}, {checkPos.y})";
                }
            }

            if (buildingDef.functionalAreaWidth > 0 || buildingDef.functionalAreaHeight > 0)
            {
                int funcStartX = 0, funcStartY = 0;
                int funcEndX = 0, funcEndY = 0;

                switch (buildingDef.direction)
                {
                    case BuildDirection.North:
                        funcStartX = 0; funcStartY = buildingDef.height;
                        funcEndX = buildingDef.width; funcEndY = buildingDef.height + buildingDef.functionalAreaHeight;
                        break;
                    case BuildDirection.South:
                        funcStartX = 0; funcStartY = -buildingDef.functionalAreaHeight;
                        funcEndX = buildingDef.width; funcEndY = 0;
                        break;
                    case BuildDirection.East:
                        funcStartX = buildingDef.width; funcStartY = 0;
                        funcEndX = buildingDef.width + buildingDef.functionalAreaWidth; funcEndY = buildingDef.height;
                        break;
                    case BuildDirection.West:
                        funcStartX = -buildingDef.functionalAreaWidth; funcStartY = 0;
                        funcEndX = 0; funcEndY = buildingDef.height;
                        break;
                }

                for (int dx = funcStartX; dx < funcEndX; dx++)
                {
                    for (int dy = funcStartY; dy < funcEndY; dy++)
                    {
                        GridPosition checkPos = position.Offset(dx, dy);
                        
                        if (gridCells.Contains(checkPos))
                            return $"功能区域与已有建筑重叠: ({checkPos.x}, {checkPos.y})";
                    }
                }
            }

            return "";
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
