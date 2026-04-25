using UnityEngine;
using System.Collections.Generic;
using GameCore;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        public int gridWidth = 100;
        public int gridHeight = 100;
        public float cellSize = 1f;
        public Color gridColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private bool[,] gridCells;
        private Dictionary<GridPosition, BuildingType> placedBuildings = new Dictionary<GridPosition, BuildingType>();
        private Transform gridVisual;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGrid();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGrid()
        {
            gridCells = new bool[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    gridCells[x, y] = false;
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

        public bool CanPlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return false;

            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition checkPos = position.Offset(dx, dy);
                    
                    if (!IsValidPosition(checkPos))
                        return false;
                    
                    if (gridCells[checkPos.x, checkPos.y])
                        return false;
                }
            }
            return true;
        }

        public bool PlaceBuilding(GridPosition position, BuildingType buildingType)
        {
            if (!CanPlaceBuilding(position, buildingType))
                return false;

            var buildingDef = DataConfig.GetBuilding(buildingType);
            
            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition cellPos = position.Offset(dx, dy);
                    gridCells[cellPos.x, cellPos.y] = true;
                }
            }

            placedBuildings[position] = buildingType;
            return true;
        }

        public bool RemoveBuilding(GridPosition position)
        {
            if (!placedBuildings.ContainsKey(position))
                return false;

            var buildingType = placedBuildings[position];
            var buildingDef = DataConfig.GetBuilding(buildingType);
            
            for (int dx = 0; dx < buildingDef.width; dx++)
            {
                for (int dy = 0; dy < buildingDef.height; dy++)
                {
                    GridPosition cellPos = position.Offset(dx, dy);
                    gridCells[cellPos.x, cellPos.y] = false;
                }
            }

            placedBuildings.Remove(position);
            return true;
        }

        public bool IsValidPosition(GridPosition pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
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

        private void OnDrawGizmos()
        {
            DrawGrid();
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