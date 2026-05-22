using UnityEngine;
using GameCore;
using UI;

namespace GridSystem
{
    public class GridDebugger : MonoBehaviour
    {
        [Header("Settings")]
        public bool showDebugger = true;
        public bool showGridCoordinates = true;
        public bool showMousePosition = true;
        public bool showBuildPreview = true;
        
        [Header("Appearance")]
        public Color textColor = Color.white;
        public int fontSize = 14;
        public float textOffsetY = 0.2f;
        
        [Header("References")]
        public Camera mainCamera;
        
        private bool isPlacingBuilding = false;
        private BuildingType currentBuildingType;
        private GridPosition currentGridPosition;
        private Vector3 currentMouseWorldPos;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            // 监听建筑放置事件
            if (UI.BuildingUI.Instance != null)
            {
                UI.BuildingUI.OnBuildingSelected += OnBuildingSelected;
                UI.BuildingUI.OnBuildingModeExit += OnBuildingModeExit;
            }
        }

        private void OnDestroy()
        {
            UI.BuildingUI.OnBuildingSelected -= OnBuildingSelected;
            UI.BuildingUI.OnBuildingModeExit -= OnBuildingModeExit;
        }

        private void Update()
        {
            if (!showDebugger) return;
            
            if (mainCamera != null)
            {
                Vector3 mouseScreenPos = Input.mousePosition;
                mouseScreenPos.z = 10f;
                currentMouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                currentMouseWorldPos.z = 0f;
                
                currentGridPosition = GridManager.Instance.WorldToGrid(currentMouseWorldPos);
            }
        }

        private void OnGUI()
        {
            if (!showDebugger) return;
            
            // 显示调试信息面板
            DrawDebugPanel();
            
            // 显示网格坐标
            if (showGridCoordinates)
            {
                DrawGridCoordinates();
            }
            
            // 显示鼠标位置信息
            if (showMousePosition)
            {
                DrawMouseInfo();
            }
        }

        private void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200), "网格调试", GUI.skin.window);
            GUILayout.BeginVertical();
            
            GUILayout.Label($"Cell Size: {GridManager.Instance.cellSize}");
            GUILayout.Space(5);
            
            GUILayout.Label($"鼠标世界坐标:");
            GUILayout.Label($"  X: {currentMouseWorldPos.x:F2}");
            GUILayout.Label($"  Y: {currentMouseWorldPos.y:F2}");
            GUILayout.Space(5);
            
            GUILayout.Label($"网格坐标: ({currentGridPosition.x}, {currentGridPosition.y})");
            GUILayout.Space(5);
            
            if (isPlacingBuilding)
            {
                var buildingDef = DataConfig.GetBuilding(currentBuildingType);
                if (buildingDef != null)
                {
                    GUILayout.Label($"正在放置: {buildingDef.name}");
                    GUILayout.Label($"大小: {buildingDef.width} x {buildingDef.height}");
                    GUILayout.Label($"左下角: ({currentGridPosition.x}, {currentGridPosition.y})");
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawGridCoordinates()
        {
            if (mainCamera == null) return;
            
            // 只在摄像机视口范围内绘制坐标
            float viewportHeight = mainCamera.orthographicSize * 2f;
            float viewportWidth = viewportHeight * mainCamera.aspect;
            Vector3 cameraPos = mainCamera.transform.position;
            
            float startX = cameraPos.x - viewportWidth / 2f;
            float endX = cameraPos.x + viewportWidth / 2f;
            float startY = cameraPos.y - viewportHeight / 2f;
            float endY = cameraPos.y + viewportHeight / 2f;
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = textColor;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = fontSize;
            
            float cellSize = GridManager.Instance.cellSize;
            
            // 转换到网格坐标
            int gridStartX = Mathf.FloorToInt(startX / cellSize) - 1;
            int gridEndX = Mathf.CeilToInt(endX / cellSize) + 1;
            int gridStartY = Mathf.FloorToInt(startY / cellSize) - 1;
            int gridEndY = Mathf.CeilToInt(endY / cellSize) + 1;
            
            // 绘制每个格子的坐标
            for (int x = gridStartX; x <= gridEndX; x++)
            {
                for (int y = gridStartY; y <= gridEndY; y++)
                {
                    // 转换回世界坐标
                    Vector3 worldPos = new Vector3(
                        x * cellSize + cellSize / 2f,
                        y * cellSize + cellSize / 2f + textOffsetY,
                        0f
                    );
                    
                    // 转换到屏幕坐标
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                    screenPos.y = Screen.height - screenPos.y;
                    
                    // 绘制坐标
                    GUI.Label(new Rect(screenPos.x - 30, screenPos.y - 10, 60, 20), 
                              $"({x}, {y})", style);
                }
            }
        }

        private void DrawMouseInfo()
        {
            if (mainCamera == null) return;
            
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.y = Screen.height - mouseScreenPos.y;
            
            // 绘制高亮框
            if (showBuildPreview && isPlacingBuilding)
            {
                var buildingDef = DataConfig.GetBuilding(currentBuildingType);
                if (buildingDef != null)
                {
                    float cellSize = GridManager.Instance.cellSize;
                    Vector3 worldPos = new Vector3(
                        currentGridPosition.x * cellSize + buildingDef.width * cellSize / 2f,
                        currentGridPosition.y * cellSize + buildingDef.height * cellSize / 2f,
                        0f
                    );
                    
                    Vector3 bottomLeft = mainCamera.WorldToScreenPoint(
                        new Vector3(currentGridPosition.x * cellSize, currentGridPosition.y * cellSize, 0f)
                    );
                    bottomLeft.y = Screen.height - bottomLeft.y;
                    
                    float width = buildingDef.width * cellSize * mainCamera.WorldToScreenPoint(Vector3.right).x;
                    float height = buildingDef.height * cellSize * mainCamera.WorldToScreenPoint(Vector3.up).y;
                    
                    // 绘制预览框
                    Color previewColor = GridManager.Instance.CanPlaceBuilding(currentGridPosition, currentBuildingType) 
                        ? Color.green : Color.red;
                    
                    Rect previewRect = new Rect(bottomLeft.x, bottomLeft.y - height, width, height);
                    DrawRect(previewRect, new Color(previewColor.r, previewColor.g, previewColor.b, 0.3f));
                }
            }
        }

        private void DrawRect(Rect rect, Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            GUI.DrawTexture(rect, texture);
            Destroy(texture);
        }

        private void OnBuildingSelected(BuildingType buildingType)
        {
            isPlacingBuilding = true;
            currentBuildingType = buildingType;
            
            Debug.Log($"[GridDebugger] 选择建筑: {buildingType}");
        }

        private void OnBuildingModeExit()
        {
            isPlacingBuilding = false;
            Debug.Log($"[GridDebugger] 退出建筑放置模式");
        }
    }
}