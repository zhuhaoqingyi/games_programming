using UnityEngine;
using GameCore;
using UI;

namespace GridSystem
{
    public class BuildingPlacer : MonoBehaviour
    {
        public Camera mainCamera;
        public GameObject previewPrefab;
        
        private GameObject currentPreview;
        private BuildingType selectedBuilding = BuildingType.None;
        private bool isPlacing = false;

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
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            GridManager.Instance.ShowPlacementGrid(mouseWorldPos);
        }

        public void CancelPlacement()
        {
            isPlacing = false;
            selectedBuilding = BuildingType.None;
            DestroyPreview();
            GridManager.Instance.HidePlacementGrid();
        }

        private void CreatePreview()
        {
            if (previewPrefab != null)
            {
                currentPreview = Instantiate(previewPrefab);
                
                var buildingDef = DataConfig.GetBuilding(selectedBuilding);
                if (buildingDef != null)
                {
                    Renderer renderer = currentPreview.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.transform.localScale = new Vector3(
                            buildingDef.width * 0.9f,
                            buildingDef.height * 0.9f,
                            0.5f
                        );
                    }
                }
                
                UpdatePreviewPosition();
            }
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }

        private void HandlePlacementPreview()
        {
            if (currentPreview == null) return;
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            UpdatePreviewPosition(mouseWorldPos);
            UpdatePreviewValidity(mouseWorldPos);
            GridManager.Instance.UpdatePlacementGridPosition(mouseWorldPos);
        }

        private void UpdatePreviewPosition(Vector3 mouseWorldPos)
        {
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity(Vector3 mouseWorldPos)
        {
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding);
            
            Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = color;
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