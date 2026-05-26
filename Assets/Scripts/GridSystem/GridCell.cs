using GameCore;

namespace GridSystem
{
    public struct GridCell
    {
        public GridPosition Position;

        public bool HasBuilding => Building != null;
        public PlacedBuilding Building { get; private set; }

        public bool HasBoard => BoardType != BoardType.None;
        public BoardType BoardType { get; private set; }

        public bool IsFunctionalArea => FunctionalBuilding != null;
        public PlacedBuilding FunctionalBuilding { get; private set; }

        public bool IsEmpty => !HasBuilding && !HasBoard && !IsFunctionalArea;

        public GridCell(GridPosition pos)
        {
            Position = pos;
            Building = null;
            BoardType = BoardType.None;
            FunctionalBuilding = null;
        }

        public void SetBuilding(PlacedBuilding building)
        {
            Building = building;
        }

        public void SetBoard(BoardType type)
        {
            BoardType = type;
        }

        public void SetFunctionalArea(PlacedBuilding building)
        {
            FunctionalBuilding = building;
        }

        public void Clear()
        {
            Building = null;
            BoardType = BoardType.None;
            FunctionalBuilding = null;
        }
    }
}
