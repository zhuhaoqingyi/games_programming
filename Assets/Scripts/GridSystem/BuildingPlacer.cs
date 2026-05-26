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
        public Color firstBoardPlacementColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        public Color deleteModeColor = new Color(1f, 0.5f, 0f, 0.7f);
        public Color selectedBuildingColor = new Color(1f, 1f, 0f, 0.9f);
        
        private GameObject currentPreview;
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

            Debug.Log($"[BuildingPlacer] 建筑旋转: 方向 -> {currentRotation}");
            UpdatePreviewPosition(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            UpdatePreviewValidity(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            UpdatePreviewScale();
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

        public void ToggleDeleteMode()
        {
            isDeleteMode = !isDeleteMode;
            
            if (isDeleteMode)
            {
                CancelPlacement();
                DeselectPlacedBuilding();
            }
            
            OnDeleteModeChanged?.Invoke(isDeleteMode);
            Debug.Log($"[BuildingPlacer] 删除模式: {(isDeleteMode ? "开启" : "关闭")}");
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
            
            Color targetColor;
            if (!isValid)
            {
                targetColor = invalidPlacementColor;
            }
            else if (buildingDef.isBoard)
            {
                if (GridManager.Instance.GetAllBoardPositions().Count == 0)
                {
                    targetColor = firstBoardPlacementColor;
                }
                else
                {
                    targetColor = validPlacementColor;
                }
            }
            else
            {
                targetColor = validPlacementColor;
            }
            
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
            
            if (GameManager.Instance != null)
            {
                if (!buildingDef.CanAfford(GameManager.Instance.GetAllResources()))
                {
                    Debug.Log("资源不足，无法建造！");
                    return;
                }
            }
            
            bool isValid = GridManager.Instance.CanPlaceBuildingWithDirection(gridPos, selectedBuilding, currentRotation);
            if (isValid)
            {
                bool placed = GridManager.Instance.PlaceBuildingWithDirection(gridPos, selectedBuilding, currentRotation);
                if (placed)
                {
                    OnBuildingPlaced?.Invoke(gridPos, selectedBuilding);
                }
            }
            else
            {
                string reason = GridManager.Instance.GetPlacementFailureReasonWithDirection(gridPos, selectedBuilding, currentRotation);
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

        private void TryRemoveBuilding(GridPosition originPos)
        {
            if (originPos.x < 0 || originPos.y < 0) return;
            
            BuildingType buildingType = GridManager.Instance.GetBuildingAt(originPos);
            if (buildingType == BuildingType.None) return;
            
            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) return;

            bool removed = GridManager.Instance.RemoveBuilding(originPos);
            if (removed)
            {
                RefundResources(buildingDef);
                OnBuildingRemoved?.Invoke(originPos, buildingType);
                Debug.Log($"[BuildingPlacer] 删除建筑: {buildingDef.name}，返还50%资源");
            }
        }

        private void RefundResources(BuildingDefinition buildingDef)
        {
            if (GameManager.Instance == null || buildingDef.costs == null) return;

            foreach (var cost in buildingDef.costs)
            {
                int refundAmount = Mathf.CeilToInt(cost.amount * 0.5f);
                GameManager.Instance.AddResource(cost.resourceType, refundAmount);
                Debug.Log($"[资源返还] {cost.resourceType}: +{refundAmount}");
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
            Debug.Log($"[BuildingPlacer] 选中建筑: {buildingDef.name}，按R旋转，右键删除");
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
                Debug.Log($"[BuildingPlacer] {buildingDef.name} 不支持旋转");
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
                
                Debug.Log($"[BuildingPlacer] 建筑旋转成功: {buildingDef.name} -> {newDirection}");
            }
            else
            {
                Debug.Log($"[BuildingPlacer] 无法旋转: {buildingDef.name}，空间不足或与其他建筑重叠");
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
