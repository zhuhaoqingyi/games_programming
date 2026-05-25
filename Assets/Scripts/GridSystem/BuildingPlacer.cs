using UnityEngine;
using GameCore;
using UI;

namespace GridSystem
{
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("Settings")]
        public Camera mainCamera;
        public GameObject previewPrefab;
        public Color validPlacementColor = new Color(1f, 1f, 1f, 0.7f);
        public Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.7f);
        
        private GameObject currentPreview;
        private BuildingType selectedBuilding = BuildingType.None;
        private bool isPlacing = false;
        private Material originalMaterial;
        private Color originalColor;

        private BuildDirection currentRotation = BuildDirection.North;

        public delegate void BuildingPlaced(GridPosition position, BuildingType type);
        public event BuildingPlaced OnBuildingPlaced;

        public delegate void BuildingRemoved(GridPosition position, BuildingType type);
        public event BuildingRemoved OnBuildingRemoved;

        private void Start()
        {
            OnBuildingPlaced += HandleBuildingPlaced;
        }

        private void OnDestroy()
        {
            OnBuildingPlaced -= HandleBuildingPlaced;
        }

        private void Update()
        {
            if (selectedBuilding != BuildingType.None && isPlacing)
            {
                HandlePlacementPreview();
                
                if (Input.GetMouseButtonDown(0))
                {
                    TryPlaceBuilding();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    CancelPlacement();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    RotateBuilding();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                TryRemoveBuilding();
            }
        }

        private void RotateBuilding()
        {
            currentRotation = (BuildDirection)((int)currentRotation + 1);
            if ((int)currentRotation > 3)
            {
                currentRotation = BuildDirection.North;
            }

            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef != null)
            {
                buildingDef.direction = currentRotation;
                Debug.Log($"[BuildingPlacer] 建筑旋转: {buildingDef.name} 方向 -> {currentRotation}");
                UpdatePreviewPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
                UpdatePreviewValidity(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        public void SelectBuilding(BuildingType buildingType)
        {
            selectedBuilding = buildingType;
            isPlacing = true;

            var def = DataConfig.GetBuilding(buildingType);
            if (def != null)
            {
                currentRotation = def.direction;
            }

            CreatePreview();
        }

        public void CancelPlacement()
        {
            isPlacing = false;
            selectedBuilding = BuildingType.None;
            DestroyPreview();
        }

        private void CreatePreview()
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;

            buildingDef.direction = currentRotation;

            // 始终使用默认预览，确保和实际放置完全一致
            currentPreview = CreateDefaultPreview(buildingDef);
            
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                originalMaterial = renderers[0].material;
                originalColor = renderers[0].material.color;
            }
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            UpdatePreviewPosition(mouseWorldPos);
            UpdatePreviewValidity(mouseWorldPos);
        }

        private GameObject CreateDefaultPreview(BuildingDefinition buildingDef)
        {
            GameObject previewObj = new GameObject("BuildingPreview");
            
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.transform.SetParent(previewObj.transform);
            
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(
                buildingDef.width * GridManager.Instance.cellSize * 0.9f,
                buildingDef.height * GridManager.Instance.cellSize * 0.9f,
                0.1f
            );

            Renderer renderer = visualObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = validPlacementColor;
                renderer.material = mat;
            }

            return previewObj;
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
                originalMaterial = null;
            }
        }

        private void HandlePlacementPreview()
        {
            if (currentPreview == null) return;
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            UpdatePreviewPosition(mouseWorldPos);
            UpdatePreviewValidity(mouseWorldPos);
        }

        private void UpdatePreviewPosition(Vector3 mouseWorldPos)
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;
            
            buildingDef.direction = currentRotation;

            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Vector3 worldPos = new Vector3(
                gridPos.x * GridManager.Instance.cellSize + buildingDef.width * GridManager.Instance.cellSize / 2f,
                gridPos.y * GridManager.Instance.cellSize + buildingDef.height * GridManager.Instance.cellSize / 2f,
                -0.1f
            );
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity(Vector3 mouseWorldPos)
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;

            buildingDef.direction = currentRotation;

            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding);
            
            Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;
            
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = targetColor;
            }
        }

        public void TryPlaceBuilding()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            
            if (buildingDef == null) return;

            buildingDef.direction = currentRotation;
            
            if (GameManager.Instance != null)
            {
                if (!buildingDef.CanAfford(GameManager.Instance.GetAllResources()))
                {
                    Debug.Log("资源不足，无法建造！");
                    return;
                }
            }
            
            if (GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding))
            {
                bool placed = GridManager.Instance.PlaceBuilding(gridPos, selectedBuilding);
                if (placed)
                {
                    OnBuildingPlaced?.Invoke(gridPos, selectedBuilding);
                }
            }
            else
            {
                string reason = GridManager.Instance.GetPlacementFailureReason(gridPos, selectedBuilding);
                Debug.Log($"[建筑放置失败] {buildingDef.name} at ({gridPos.x}, {gridPos.y}): {reason}");
            }
        }

        private void HandleBuildingPlaced(GridPosition position, BuildingType type)
        {
            if (BuildingUI.Instance != null)
            {
                BuildingUI.Instance.OnBuildingPlacedSuccess(position, type);
            }
        }

        private void TryRemoveBuilding()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            BuildingType buildingAtPos = GridManager.Instance.GetBuildingAt(gridPos);
            
            if (buildingAtPos != BuildingType.None)
            {
                GridPosition originPos = GridManager.Instance.GetBuildingOrigin(gridPos);
                
                bool removed = GridManager.Instance.RemoveBuilding(originPos);
                if (removed)
                {
                    OnBuildingRemoved?.Invoke(originPos, buildingAtPos);
                }
            }
        }
    }
}
