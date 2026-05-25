using UnityEngine;
using GameCore;
using UI;

namespace GridSystem
{
    public class BoardPlacer : MonoBehaviour
    {
        [Header("Settings")]
        public Camera mainCamera;
        public Color validPlacementColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        public Color invalidPlacementColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        public Color firstPlacementColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        
        private GameObject currentPreview;
        private BoardType selectedBoard = BoardType.None;
        private bool isPlacing = false;
        private Material previewMaterial;

        public delegate void BoardPlaced(GridPosition position, BoardType type);
        public event BoardPlaced OnBoardPlaced;

        public delegate void BoardRemoved(GridPosition position, BoardType type);
        public event BoardRemoved OnBoardRemoved;

        private void Update()
        {
            if (selectedBoard != BoardType.None && isPlacing)
            {
                HandlePlacementPreview();
                
                if (Input.GetMouseButtonDown(0))
                {
                    TryPlaceBoard();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    CancelPlacement();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                TryRemoveBoard();
            }
        }

        public void SelectBoard(BoardType boardType)
        {
            selectedBoard = boardType;
            isPlacing = true;
            CreatePreview();
        }

        public void CancelPlacement()
        {
            isPlacing = false;
            selectedBoard = BoardType.None;
            DestroyPreview();
        }

        private void CreatePreview()
        {
            currentPreview = CreateDefaultPreview();
            
            Renderer renderer = currentPreview.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                previewMaterial = new Material(Shader.Find("Unlit/Color"));
                renderer.material = previewMaterial;
            }
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            UpdatePreviewPosition(mouseWorldPos);
            UpdatePreviewValidity(mouseWorldPos);
        }

        private GameObject CreateDefaultPreview()
        {
            GameObject previewObj = new GameObject("BoardPreview");
            
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.transform.SetParent(previewObj.transform);
            
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(
                GridManager.Instance.cellSize * 0.9f,
                GridManager.Instance.cellSize * 0.9f,
                0.05f
            );

            Renderer renderer = visualObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = validPlacementColor;
            }

            return previewObj;
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
                previewMaterial = null;
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
            Vector3 worldPos = new Vector3(
                gridPos.x * GridManager.Instance.cellSize + GridManager.Instance.cellSize / 2f,
                gridPos.y * GridManager.Instance.cellSize + GridManager.Instance.cellSize / 2f,
                -0.1f
            );
            
            currentPreview.transform.position = worldPos;
        }

        private void UpdatePreviewValidity(Vector3 mouseWorldPos)
        {
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            bool isValid = BoardManager.Instance.CanPlaceBoard(gridPos);
            
            Color targetColor;
            if (isValid)
            {
                if (BoardManager.Instance.GetAllBoardPositions().Count == 0)
                {
                    targetColor = firstPlacementColor;
                }
                else
                {
                    targetColor = validPlacementColor;
                }
            }
            else
            {
                targetColor = invalidPlacementColor;
            }
            
            Renderer renderer = currentPreview.GetComponentInChildren<Renderer>();
            if (renderer != null && previewMaterial != null)
            {
                previewMaterial.color = targetColor;
            }
        }

        public void TryPlaceBoard()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            Debug.Log($"[BoardPlacer.TryPlaceBoard] 尝试放置board at 网格坐标({gridPos.x}, {gridPos.y})");
            
            var boardDef = DataConfig.GetBoard(selectedBoard);
            
            if (boardDef == null) return;
            
            if (GameManager.Instance != null)
            {
                bool canAfford = true;
                foreach (var cost in boardDef.costs)
                {
                    if (GameManager.Instance.GetResourceAmount(cost.resourceType) < cost.amount)
                    {
                        canAfford = false;
                        break;
                    }
                }
                
                if (!canAfford)
                {
                    Debug.Log("资源不足，无法建造太空板！");
                    return;
                }
            }
            
            bool canPlace = BoardManager.Instance.CanPlaceBoard(gridPos);
            Debug.Log($"[BoardPlacer.TryPlaceBoard] CanPlaceBoard结果: {canPlace}");
            
            if (canPlace)
            {
                bool placed = BoardManager.Instance.PlaceBoard(gridPos, selectedBoard);
                Debug.Log($"[BoardPlacer.TryPlaceBoard] PlaceBoard结果: {placed}");
                if (placed)
                {
                    OnBoardPlaced?.Invoke(gridPos, selectedBoard);
                }
            }
        }

        private void TryRemoveBoard()
        {
            if (BoardManager.Instance == null) return;
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            
            if (BoardManager.Instance.HasBoardAt(gridPos))
            {
                BoardType boardType = BoardManager.Instance.GetBoardAt(gridPos);
                bool removed = BoardManager.Instance.RemoveBoard(gridPos);
                if (removed)
                {
                    OnBoardRemoved?.Invoke(gridPos, boardType);
                }
            }
        }
    }
}