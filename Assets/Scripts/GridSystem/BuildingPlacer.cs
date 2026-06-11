using UnityEngine;
using System.Collections.Generic;
using GameCore;
using UI;
using LogisticsSystem;

namespace GridSystem
{
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("Settings")]
        public Camera mainCamera;
        public GameObject previewPrefab;
        public Color validPlacementColor = new Color(1f, 1f, 1f, 0.7f);
        public Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.7f);
        public Color firstBoardPlacementColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        public Color deleteModeColor = new Color(1f, 0.5f, 0f, 0.7f);
        public Color selectedBuildingColor = new Color(1f, 1f, 0f, 0.9f);

        public Color functionalAreaValidColor = new Color(0.3f, 0.6f, 1f, 0.25f);
        public Color functionalAreaInvalidColor = new Color(1f, 0.2f, 0.2f, 0.25f);
        
        private GameObject currentPreview;
        private GameObject functionalAreaPreview;
        private Renderer buildingPreviewRenderer;
        private Renderer functionalAreaPreviewRenderer;
        private BuildingType selectedBuilding = BuildingType.None;
        private bool isPlacing = false;
        private Material originalMaterial;
        private Color originalColor;

        private BuildDirection currentRotation = BuildDirection.East;

        private bool isDeleteMode = false;
        private GameObject selectedPlacedBuilding;
        private GridPosition selectedBuildingOrigin;
        private BuildDirection selectedBuildingDirection;

        public delegate void BuildingPlaced(GridPosition position, BuildingType type);
        public event BuildingPlaced OnBuildingPlaced;

        public delegate void BuildingRemoved(GridPosition position, BuildingType type);
        public event BuildingRemoved OnBuildingRemoved;

        public delegate void DeleteModeChanged(bool isDeleteMode);
        public event DeleteModeChanged OnDeleteModeChanged;

        public bool IsDeleteMode => isDeleteMode;

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
            if (isDeleteMode)
            {
                HandleDeleteModeInput();
            }
            else if (selectedBuilding != BuildingType.None && isPlacing)
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
                    RotatePreviewBuilding();
                }
            }
        }

        private void HandleDeleteModeInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                DeselectPlacedBuilding();
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                
                GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
                BuildingType buildingAtPos = GridManager.Instance.GetBuildingAt(gridPos);
                
                if (buildingAtPos != BuildingType.None)
                {
                    GridPosition originPos = GridManager.Instance.GetBuildingOrigin(gridPos);
                    TryRemoveBuilding(originPos);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.R) && selectedPlacedBuilding != null)
            {
                RotatePlacedBuilding();
            }
        }

        private void RotatePreviewBuilding()
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null || !buildingDef.canRotate) return;

            int currentVal = (int)currentRotation;
            currentVal = (currentVal + 1) % 4;
            currentRotation = (BuildDirection)currentVal;

            Debug.Log($"[BuildingPlacer] Building rotation: direction -> {currentRotation}");

            // Recreate functional area preview with new direction dimensions
            DestroyFunctionalAreaPreview();
            CreateFunctionalAreaPreview(buildingDef);

            UpdatePreviewPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            UpdatePreviewValidity(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            UpdatePreviewScale();
        }

        private void DestroyFunctionalAreaPreview()
        {
            if (functionalAreaPreview != null)
            {
                Destroy(functionalAreaPreview);
            }
            functionalAreaPreview = null;
            functionalAreaPreviewRenderer = null;
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
            Debug.Log("[BuildingPlacer] CancelPlacement called");
            
            isPlacing = false;
            selectedBuilding = BuildingType.None;
            DestroyPreview();

            // 隐藏网格
            if (GridRenderer.Instance != null)
            {
                GridRenderer.Instance.HideGrid();
                Debug.Log("[BuildingPlacer] Grid hidden");
            }
            
            Debug.Log("[BuildingPlacer] CancelPlacement completed");
        }

        public void ToggleDeleteMode()
        {
            isDeleteMode = !isDeleteMode;
            
            if (isDeleteMode)
            {
                CancelPlacement();
                DeselectPlacedBuilding();
                // 隐藏建筑UI（与放置预览一致）
                if (UI.BuildingUI.Instance != null)
                {
                    UI.BuildingUI.Instance.HideUI();
                }
            }
            else
            {
                // 退出删除模式时恢复建筑UI
                if (UI.BuildingUI.Instance != null)
                {
                    UI.BuildingUI.Instance.ShowUI();
                }
            }
            
            OnDeleteModeChanged?.Invoke(isDeleteMode);
            Debug.Log($"[BuildingPlacer] Delete mode: {(isDeleteMode ? "ON" : "OFF")}");
        }

        public void SetDeleteMode(bool enabled)
        {
            if (isDeleteMode != enabled)
            {
                ToggleDeleteMode();
            }
        }

        private void CreatePreview()
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;

            currentPreview = CreateDefaultPreview(buildingDef);
            
            CreateFunctionalAreaPreview(buildingDef);
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            UpdatePreviewPosition(mouseWorldPos);
            UpdatePreviewValidity(mouseWorldPos);
            UpdatePreviewScale();
        }

        private int GetCurrentWidth(BuildingDefinition def)
        {
            if (currentRotation == BuildDirection.North || currentRotation == BuildDirection.South)
                return def.height;
            return def.width;
        }

        private int GetCurrentHeight(BuildingDefinition def)
        {
            if (currentRotation == BuildDirection.North || currentRotation == BuildDirection.South)
                return def.width;
            return def.height;
        }

        private void UpdatePreviewScale()
        {
            if (currentPreview == null) return;
            
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;

            int displayWidth = GetCurrentWidth(buildingDef);
            int displayHeight = GetCurrentHeight(buildingDef);

            Transform visualObj = currentPreview.transform.GetChild(0);
            if (visualObj != null)
            {
                visualObj.localScale = new Vector3(
                    displayWidth * GridManager.Instance.cellSize * 0.9f,
                    displayHeight * GridManager.Instance.cellSize * 0.9f,
                    0.1f
                );
            }

            UpdateFunctionalAreaPreviewScale(buildingDef);
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

            buildingPreviewRenderer = visualObj.GetComponent<Renderer>();
            if (buildingPreviewRenderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = validPlacementColor;
                buildingPreviewRenderer.material = mat;
            }

            return previewObj;
        }

        private void CreateFunctionalAreaPreview(BuildingDefinition buildingDef)
        {
            Debug.Log($"[BuildingPlacer] CreateFunctionalAreaPreview called. FA Width={buildingDef.functionalAreaWidth}, FA Height={buildingDef.functionalAreaHeight}");
            
            if (buildingDef.functionalAreaWidth <= 0 || buildingDef.functionalAreaHeight <= 0)
            {
                Debug.Log("[BuildingPlacer] Functional area dimensions are zero or negative, skipping creation.");
                return;
            }

            functionalAreaPreview = new GameObject("FunctionalAreaPreview");
            functionalAreaPreview.transform.SetParent(currentPreview.transform);
            functionalAreaPreview.transform.localPosition = Vector3.zero;

            GameObject faVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faVisual.transform.SetParent(functionalAreaPreview.transform);
            faVisual.transform.localPosition = Vector3.zero;

            functionalAreaPreviewRenderer = faVisual.GetComponent<Renderer>();
            if (functionalAreaPreviewRenderer != null)
            {
                Material faMat = new Material(Shader.Find("Unlit/Color"));
                faMat.color = functionalAreaValidColor;
                functionalAreaPreviewRenderer.material = faMat;
                Debug.Log($"[BuildingPlacer] Functional area preview created, color set to {functionalAreaValidColor}");
            }
            else
            {
                Debug.LogError("[BuildingPlacer] Could not get Renderer from functional area visual!");
            }

            Collider faCollider = faVisual.GetComponent<Collider>();
            if (faCollider != null)
            {
                Destroy(faCollider);
            }

            UpdateFunctionalAreaPreviewScale(buildingDef);
        }

        private void UpdateFunctionalAreaPreviewScale(BuildingDefinition buildingDef)
        {
            if (functionalAreaPreview == null) return;
            if (buildingDef.functionalAreaWidth <= 0 || buildingDef.functionalAreaHeight <= 0) return;

            float cs = GridManager.Instance.cellSize;

            // Preview parent is centered at grid cell center
            // Functional area offsets and scales per direction
            Vector3 offset = Vector3.zero;
            Vector3 scale = Vector3.zero;

            // Only use thruster-specific hardcoded values for Thruster building
            bool isThruster = (buildingDef.type == BuildingType.Thruster);

            if (isThruster)
            {
                // Thruster: 2x2 body, 2x8 exhaust area in specific directions
                switch (currentRotation)
                {
                    case BuildDirection.East:
                        offset = new Vector3(5f * cs, 0f, 0f);
                        scale = new Vector3(8f * cs, 2f * cs, 1f);
                        break;
                    case BuildDirection.West:
                        offset = new Vector3(-5f * cs, 0f, 0f);
                        scale = new Vector3(8f * cs, 2f * cs, 1f);
                        break;
                    case BuildDirection.North:
                        offset = new Vector3(0f, 5f * cs, 0f);
                        scale = new Vector3(2f * cs, 8f * cs, 1f);
                        break;
                    case BuildDirection.South:
                        offset = new Vector3(0f, -5f * cs, 0f);
                        scale = new Vector3(2f * cs, 8f * cs, 1f);
                        break;
                }
            }
            else
            {
                // All other buildings: use functional area dimensions from building definition
                // Calculate display dimensions based on rotation
                int displayWidth = GetCurrentWidth(buildingDef);
                int displayHeight = GetCurrentHeight(buildingDef);

                // Functional area dimensions after rotation
                int faDisplayWidth = buildingDef.functionalAreaWidth;
                int faDisplayHeight = buildingDef.functionalAreaHeight;
                if (currentRotation == BuildDirection.North || currentRotation == BuildDirection.South)
                {
                    faDisplayWidth = buildingDef.functionalAreaHeight;
                    faDisplayHeight = buildingDef.functionalAreaWidth;
                }

                // Offset from building center to functional area center
                // East/West: functional area extends along Y, offset is perpendicular
                // North/South: functional area extends along X, offset is perpendicular
                float offsetX = 0f;
                float offsetY = 0f;

                switch (currentRotation)
                {
                    case BuildDirection.East:
                        // Functional area starts at the east edge of building
                        offsetX = (displayWidth / 2f + faDisplayWidth / 2f) * cs;
                        break;
                    case BuildDirection.West:
                        offsetX = -(displayWidth / 2f + faDisplayWidth / 2f) * cs;
                        break;
                    case BuildDirection.North:
                        offsetY = (displayHeight / 2f + faDisplayHeight / 2f) * cs;
                        break;
                    case BuildDirection.South:
                        offsetY = -(displayHeight / 2f + faDisplayHeight / 2f) * cs;
                        break;
                }

                offset = new Vector3(offsetX, offsetY, 0f);
                scale = new Vector3(faDisplayWidth * cs, faDisplayHeight * cs, 1f);
            }

            functionalAreaPreview.transform.localPosition = offset;

            Transform faVisual = functionalAreaPreview.transform.GetChild(0);
            if (faVisual != null)
            {
                faVisual.localScale = scale;
            }
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            currentPreview = null;
            functionalAreaPreview = null;
            buildingPreviewRenderer = null;
            functionalAreaPreviewRenderer = null;
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
            
            int displayWidth = GetCurrentWidth(buildingDef);
            int displayHeight = GetCurrentHeight(buildingDef);

            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Vector3 worldPos = new Vector3(
                gridPos.x * GridManager.Instance.cellSize + displayWidth * GridManager.Instance.cellSize / 2f,
                gridPos.y * GridManager.Instance.cellSize + displayHeight * GridManager.Instance.cellSize / 2f,
                -0.1f
            );
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity(Vector3 mouseWorldPos)
        {
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            if (buildingDef == null) return;

            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = GridManager.Instance.CanPlaceBuildingWithDirection(gridPos, selectedBuilding, currentRotation);
            
            Color buildingColor;
            if (!isValid)
            {
                buildingColor = invalidPlacementColor;
            }
            else if (buildingDef.isBoard)
            {
                if (GridManager.Instance.GetAllBoardPositions().Count == 0)
                {
                    buildingColor = firstBoardPlacementColor;
                }
                else
                {
                    buildingColor = validPlacementColor;
                }
            }
            else
            {
                buildingColor = validPlacementColor;
            }
            
            Color faColor = isValid ? functionalAreaValidColor : functionalAreaInvalidColor;
            
            if (buildingPreviewRenderer != null)
            {
                buildingPreviewRenderer.material.color = buildingColor;
            }
            if (functionalAreaPreviewRenderer != null)
            {
                functionalAreaPreviewRenderer.material.color = faColor;
            }
        }

        public void TryPlaceBuilding()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            
            if (buildingDef == null) return;
            
            if (GameManager.Instance != null)
            {
                if (!buildingDef.CanAfford(GameManager.Instance.GetAllResources()))
                {
                    Debug.Log("Insufficient resources to build!");
                    return;
                }
            }
            
            bool isValid = GridManager.Instance.CanPlaceBuildingWithDirection(gridPos, selectedBuilding, currentRotation);
            if (isValid)
            {
                bool placed = GridManager.Instance.PlaceBuildingWithDirection(gridPos, selectedBuilding, currentRotation);
                if (placed)
                {
                    AudioManager.Instance?.PlayBuildingPlace();
                    OnBuildingPlaced?.Invoke(gridPos, selectedBuilding);
                }
            }
            else
            {
                string reason = GridManager.Instance.GetPlacementFailureReasonWithDirection(gridPos, selectedBuilding, currentRotation);
                Debug.Log($"[BuildingPlacer] Building placement failed: {buildingDef.name} at ({gridPos.x}, {gridPos.y}): {reason}");
            }
        }

        private void HandleBuildingPlaced(GridPosition position, BuildingType type)
        {
            if (BuildingUI.Instance != null)
            {
                BuildingUI.Instance.OnBuildingPlacedSuccess(position, type);
            }

            var buildingDef = DataConfig.GetBuilding(type);
            if (buildingDef != null)
            {
                if (buildingDef.storageCapacity > 0)
                {
                    GameManager.Instance?.AddStorageCapacity(buildingDef.storageCapacity);
                    Debug.Log($"[BuildingPlacer] Storage capacity +{buildingDef.storageCapacity}");
                }

                PlacedBuilding placed = GridManager.Instance.GetPlacedBuildingAt(position);
                if (placed != null && placed.GameObject != null)
                {
                    ContainerComponent container = placed.GameObject.GetComponentInChildren<ContainerComponent>();
                    if (container != null)
                    {
                        Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();
                        foreach (var rc in container.resourceCapacities)
                        {
                            capacities[rc.resourceType] = rc.capacity;
                        }
                        GameManager.Instance?.AddContainer(capacities, container.GetTotalCapacity());
                        Debug.Log($"[BuildingPlacer] Container registered with {container.resourceCapacities.Count} resource types");
                    }
                }
            }
        }

        private void TryRemoveBuilding(GridPosition originPos)
        {
            if (!GridManager.Instance.HasCell(originPos)) return;
            
            BuildingType buildingType = GridManager.Instance.GetBuildingAt(originPos);
            if (buildingType == BuildingType.None) return;
            
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return;

            // 核心建筑不能被删除
            if (buildingDef.isCoreBuilding)
            {
                Debug.Log($"[BuildingPlacer] Cannot remove core building: {buildingDef.name}");
                return;
            }

            PlacedBuilding placed = GridManager.Instance.GetPlacedBuildingAt(originPos);
            if (placed != null && placed.GameObject != null)
            {
                ContainerComponent container = placed.GameObject.GetComponentInChildren<ContainerComponent>();
                if (container != null)
                {
                    Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();
                    foreach (var rc in container.resourceCapacities)
                    {
                        capacities[rc.resourceType] = rc.capacity;
                    }
                    GameManager.Instance?.RemoveContainer(capacities, container.GetTotalCapacity());
                    Debug.Log($"[BuildingPlacer] Container deregistered");
                }
            }

            if (buildingDef.storageCapacity > 0)
            {
                GameManager.Instance?.RemoveStorageCapacity(buildingDef.storageCapacity);
                Debug.Log($"[BuildingPlacer] Storage capacity -{buildingDef.storageCapacity}");
            }

            bool removed = GridManager.Instance.RemoveBuilding(originPos);
            if (removed)
            {
                RefundResources(buildingDef);
                GameManager.Instance?.EnforceCapacityLimits();
                OnBuildingRemoved?.Invoke(originPos, buildingType);
                Debug.Log($"[BuildingPlacer] Building removed: {buildingDef.name}, refunding 50% resources");
            }
        }

        private void RefundResources(BuildingDefinition buildingDef)
        {
            if (GameManager.Instance == null || buildingDef.costs == null) return;

            foreach (var cost in buildingDef.costs)
            {
                int refundAmount = Mathf.CeilToInt(cost.amount * 0.5f);
                GameManager.Instance.AddResource(cost.resourceType, refundAmount);
                Debug.Log($"[Resource Refund] {cost.resourceType}: +{refundAmount}");
            }
        }

        private void SelectPlacedBuilding(GridPosition originPos)
        {
            DeselectPlacedBuilding();
            
            selectedBuildingOrigin = originPos;
            PlacedBuilding placed = GridManager.Instance.GetPlacedBuildingAt(originPos);
            
            if (placed == null) return;
            
            var buildingDef = placed.Definition;
            if (buildingDef == null) return;
            
            selectedPlacedBuilding = placed.GameObject;
            selectedBuildingDirection = placed.Direction;
            
            HighlightSelectedBuilding(true);
            Debug.Log($"[BuildingPlacer] Building selected: {buildingDef.name}, press R to rotate, right-click to delete");
        }

        private void DeselectPlacedBuilding()
        {
            if (selectedPlacedBuilding != null)
            {
                HighlightSelectedBuilding(false);
                selectedPlacedBuilding = null;
            }
            
            selectedBuildingOrigin = new GridPosition(-1, -1);
        }

        private void HighlightSelectedBuilding(bool highlight)
        {
            if (selectedPlacedBuilding == null) return;
            
            Renderer[] renderers = selectedPlacedBuilding.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (highlight)
                {
                    renderer.material.color = selectedBuildingColor;
                }
                else
                {
                    renderer.material.color = originalColor;
                }
            }
        }

        private void RotatePlacedBuilding()
        {
            if (selectedPlacedBuilding == null) return;
            
            PlacedBuilding placed = GridManager.Instance.GetPlacedBuildingAt(selectedBuildingOrigin);
            if (placed == null) return;
            
            var buildingDef = placed.Definition;
            if (buildingDef == null) return;
            
            if (!placed.CanRotate)
            {
                Debug.Log($"[BuildingPlacer] {buildingDef.name} does not support rotation");
                return;
            }

            int currentVal = (int)placed.Direction;
            BuildDirection newDirection = (BuildDirection)((currentVal + 1) % 4);

            if (GridManager.Instance.CanPlaceBuildingWithDirection(selectedBuildingOrigin, placed.BuildingType, newDirection))
            {
                GridManager.Instance.UpdateBuildingCells(selectedBuildingOrigin, placed.BuildingType, newDirection);
                
                PlacedBuilding swapped = GridManager.Instance.SwapBuildingPrefab(selectedBuildingOrigin, newDirection);
                if (swapped != null)
                {
                    selectedPlacedBuilding = swapped.GameObject;
                    selectedBuildingDirection = newDirection;
                }
                
                Debug.Log($"[BuildingPlacer] Building rotated successfully: {buildingDef.name} -> {newDirection}");
            }
            else
            {
                Debug.Log($"[BuildingPlacer] Cannot rotate: {buildingDef.name}, not enough space or overlaps with other buildings");
            }
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
    }
}
