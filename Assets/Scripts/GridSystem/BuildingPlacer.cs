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
            }
            else if (Input.GetMouseButtonDown(1))
            {
                TryRemoveBuilding();
            }
        }

        public void SelectBuilding(BuildingType buildingType)
        {
            selectedBuilding = buildingType;
            isPlacing = true;
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
            if (previewPrefab != null)
            {
                currentPreview = Instantiate(previewPrefab);
                currentPreview.name = "BuildingPreview";
                
                var buildingDef = DataConfig.GetBuilding(selectedBuilding);
                if (buildingDef != null)
                {
                    Transform visualTransform = currentPreview.transform;
                    visualTransform.localScale = new Vector3(
                        buildingDef.width * 0.9f,
                        buildingDef.height * 0.9f,
                        0.5f
                    );
                }
                
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
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            worldPos.z = -0.1f;
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity(Vector3 mouseWorldPos)
        {
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding);
            
            Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;
            
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = targetColor;
            }
        }

        private void TryPlaceBuilding()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            var buildingDef = DataConfig.GetBuilding(selectedBuilding);
            
            if (buildingDef != null && GameManager.Instance != null)
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