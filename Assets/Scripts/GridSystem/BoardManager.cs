using UnityEngine;
using System.Collections.Generic;
using GameCore;

namespace GridSystem
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [Header("Board Settings")]
        public Transform boardsContainer;
        public Color boardColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        public Color boardHighlightColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        public Color boardInvalidColor = new Color(0.3f, 0.1f, 0.1f, 0.8f);

        private HashSet<GridPosition> placedBoards = new HashSet<GridPosition>();
        private Dictionary<GridPosition, BoardType> boardTypes = new Dictionary<GridPosition, BoardType>();
        private Dictionary<GridPosition, GameObject> boardObjects = new Dictionary<GridPosition, GameObject>();

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
            if (boardsContainer == null)
            {
                GameObject containerObj = new GameObject("Boards");
                boardsContainer = containerObj.transform;
            }
        }

        public bool CanPlaceBoard(GridPosition position)
        {
            if (placedBoards.Contains(position))
            {
                Debug.Log($"[BoardManager] 放置失败: 位置({position.x}, {position.y})已有board");
                return false;
            }

            if (placedBoards.Count == 0)
            {
                Debug.Log($"[BoardManager] 第一个board，可以放置在任意位置({position.x}, {position.y})");
                return true;
            }

            bool hasAdjacent = HasAdjacentBoard(position);
            if (!hasAdjacent)
            {
                Debug.Log($"[BoardManager] 放置失败: 位置({position.x}, {position.y})不与任何board相邻");
            }
            return hasAdjacent;
        }

        public bool HasAdjacentBoard(GridPosition position)
        {
            GridPosition[] neighbors = new GridPosition[]
            {
                position.Offset(-1, 0),
                position.Offset(1, 0),
                position.Offset(0, -1),
                position.Offset(0, 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (placedBoards.Contains(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PlaceBoard(GridPosition position, BoardType boardType)
        {
            if (!CanPlaceBoard(position))
                return false;

            var boardDef = DataConfig.GetBoard(boardType);
            if (boardDef == null) return false;

            GameObject boardObj = new GameObject($"Board_{position.x}_{position.y}");
            
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.transform.SetParent(boardObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(
                GridManager.Instance.cellSize * 0.95f,
                GridManager.Instance.cellSize * 0.95f,
                0.1f
            );

            Renderer renderer = visualObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = boardColor;
            }

            boardObj.transform.position = new Vector3(
                position.x * GridManager.Instance.cellSize + GridManager.Instance.cellSize / 2f,
                position.y * GridManager.Instance.cellSize + GridManager.Instance.cellSize / 2f,
                -0.05f
            );
            boardObj.transform.SetParent(boardsContainer);

            placedBoards.Add(position);
            boardTypes[position] = boardType;
            boardObjects[position] = boardObj;

            Debug.Log($"[BoardManager] 放置太空板: {boardDef.name} at ({position.x}, {position.y})");
            return true;
        }

        public bool RemoveBoard(GridPosition position)
        {
            if (!placedBoards.Contains(position))
                return false;

            if (boardObjects.ContainsKey(position))
            {
                Destroy(boardObjects[position]);
                boardObjects.Remove(position);
            }

            placedBoards.Remove(position);
            boardTypes.Remove(position);
            
            Debug.Log($"[BoardManager] 移除太空板 at ({position.x}, {position.y})");
            return true;
        }

        public bool HasBoardAt(GridPosition position)
        {
            return placedBoards.Contains(position);
        }

        public bool RegisterBoardPosition(GridPosition position)
        {
            Debug.Log($"[BoardManager.RegisterBoardPosition] 尝试注册位置({position.x}, {position.y})");
            
            if (placedBoards.Contains(position))
            {
                Debug.Log($"[BoardManager.RegisterBoardPosition] 失败: 位置({position.x}, {position.y})已有board");
                return false;
            }

            if (placedBoards.Count == 0)
            {
                Debug.Log($"[BoardManager.RegisterBoardPosition] 第一个board，允许注册");
                placedBoards.Add(position);
                return true;
            }

            if (HasAdjacentBoard(position))
            {
                Debug.Log($"[BoardManager.RegisterBoardPosition] 与已有board相邻，允许注册");
                placedBoards.Add(position);
                return true;
            }

            Debug.Log($"[BoardManager.RegisterBoardPosition] 失败: 位置({position.x}, {position.y})不与任何board相邻");
            return false;
        }

        public void UnregisterBoardPosition(GridPosition position)
        {
            placedBoards.Remove(position);
        }

        public BoardType GetBoardAt(GridPosition position)
        {
            boardTypes.TryGetValue(position, out var type);
            return type;
        }

        public HashSet<GridPosition> GetAllBoardPositions()
        {
            return new HashSet<GridPosition>(placedBoards);
        }

        public void HighlightBoard(GridPosition position, bool highlight)
        {
            if (!boardObjects.ContainsKey(position)) return;

            Renderer renderer = boardObjects[position].GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = highlight ? boardHighlightColor : boardColor;
            }
        }
    }
}