using UnityEngine;
using GameCore;

namespace GridSystem
{
    public class PlacedBuilding
    {
        public BuildingType BuildingType { get; private set; }
        public GridPosition OriginPosition { get; private set; }
        public BuildDirection Direction { get; private set; }
        public GameObject GameObject { get; private set; }
        public BuildingComponent Component { get; private set; }

        public Vector3 WorldPosition => GameObject != null ? GameObject.transform.position : Vector3.zero;

        public BuildingDefinition Definition => DataConfig.GetBuilding(BuildingType);
        public int DisplayWidth
        {
            get
            {
                var def = Definition;
                if (def == null) return 0;
                if (Direction == BuildDirection.North || Direction == BuildDirection.South)
                    return def.height;
                return def.width;
            }
        }
        public int DisplayHeight
        {
            get
            {
                var def = Definition;
                if (def == null) return 0;
                if (Direction == BuildDirection.North || Direction == BuildDirection.South)
                    return def.width;
                return def.height;
            }
        }

        public bool IsBoard => Definition != null && Definition.isBoard;
        public bool CanRotate => Definition != null && Definition.canRotate;

        public PlacedBuilding(BuildingType type, GridPosition origin, BuildDirection direction, GameObject obj, BuildingComponent comp)
        {
            BuildingType = type;
            OriginPosition = origin;
            Direction = direction;
            GameObject = obj;
            Component = comp;
        }

        public void UpdateWorldPosition()
        {
            if (GameObject != null)
            {
                var gm = GridManager.Instance;
                if (gm == null) return;

                float cellSize = gm.cellSize;
                float worldX = OriginPosition.x * cellSize + DisplayWidth * cellSize / 2f;
                float worldY = OriginPosition.y * cellSize + DisplayHeight * cellSize / 2f;
                GameObject.transform.position = new Vector3(worldX, worldY, 0f);
            }
        }

        public void Rotate(BuildDirection newDirection)
        {
            Direction = newDirection;
            if (GameObject != null)
            {
                GameObject.transform.rotation = Quaternion.Euler(GetRotationEuler(newDirection));
            }
        }

        public void ApplyRotationOffset(Vector3 offset)
        {
            if (GameObject != null)
            {
                GameObject.transform.position += offset;
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

        public void Destroy()
        {
            if (GameObject != null)
            {
                Object.Destroy(GameObject);
                GameObject = null;
            }
            Component = null;
        }
    }
}
