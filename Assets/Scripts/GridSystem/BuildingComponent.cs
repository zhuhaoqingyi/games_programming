using UnityEngine;
using GameCore;

namespace GridSystem
{
    public class BuildingComponent : MonoBehaviour
    {
        [SerializeField] protected BuildingType buildingType;
        [SerializeField] protected BuildingStatus status = BuildingStatus.Placed;
        
        protected GridPosition gridPosition;
        protected BuildingDefinition buildingDef;
        protected float powerOutput;
        protected float powerInput;

        public BuildingType Type => buildingType;
        public BuildingStatus Status => status;
        public GridPosition GridPosition => gridPosition;

        protected virtual void Awake()
        {
            buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef != null)
            {
                powerOutput = buildingDef.powerProduction;
                powerInput = buildingDef.powerConsumption;
            }
        }

        public virtual void Initialize(GridPosition pos)
        {
            gridPosition = pos;
            transform.position = GridManager.Instance.GridToWorld(pos);
        }

        public virtual void Activate()
        {
            status = BuildingStatus.Active;
        }

        public virtual void Deactivate()
        {
            status = BuildingStatus.Inactive;
        }

        public virtual float GetPowerOutput()
        {
            return status == BuildingStatus.Active ? powerOutput : 0;
        }

        public virtual float GetPowerInput()
        {
            return status == BuildingStatus.Active ? powerInput : 0;
        }

        public virtual bool CanWork()
        {
            return status == BuildingStatus.Active;
        }

        public virtual void UpdateBuilding(float deltaTime)
        {
            if (status == BuildingStatus.Active)
            {
                OnUpdate(deltaTime);
            }
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }
    }
}