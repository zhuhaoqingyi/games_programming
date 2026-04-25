using UnityEngine;
using GameCore;

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
            
            UpdatePreviewPosition();
            UpdatePreviewValidity();
        }

        private void UpdatePreviewPosition()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding);
            
            Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            Renderer renderer = currentPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        private void TryPlaceBuilding()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            
            if (GridManager.Instance.CanPlaceBuilding(gridPos, selectedBuilding))
            {
                bool placed = GridManager.Instance.PlaceBuilding(gridPos, selectedBuilding);
                if (placed)
                {
                    OnBuildingPlaced?.Invoke(gridPos, selectedBuilding);
                    CancelPlacement();
                }
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