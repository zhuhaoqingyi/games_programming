using System.Collections.Generic;

namespace GameCore
{
    public struct GridPosition
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public GridPosition Offset(int dx, int dy)
        {
            return new GridPosition(x + dx, y + dy);
        }
    }

    public struct GameTime
    {
        public float totalTime;
        public float deltaTime;
        public int day;
        public float hour;

        public GameTime(float totalTime, float deltaTime)
        {
            this.totalTime = totalTime;
            this.deltaTime = deltaTime;
            this.day = (int)(totalTime / 86400f);
            this.hour = (totalTime % 86400f) / 3600f;
        }
    }

    public class GameSaveData
    {
        public string saveName;
        public GameTime gameTime;
        public Dictionary<ResourceType, int> globalInventory;
        public Dictionary<GridPosition, BuildingType> placedBuildings;
        public float totalPowerGenerated;
        public float totalPowerConsumed;

        public GameSaveData(string saveName)
        {
            this.saveName = saveName;
            this.globalInventory = new Dictionary<ResourceType, int>();
            this.placedBuildings = new Dictionary<GridPosition, BuildingType>();
        }
    }
}