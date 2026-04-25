using UnityEngine;
using GameCore;
using PowerSystem;

namespace ProductionSystem
{
    public class MiningBuilding : GridSystem.BuildingComponent
    {
        [SerializeField] private float miningInterval = 3f;
        [SerializeField] private ResourceType minedResource = ResourceType.SpaceOre;
        [SerializeField] private int miningAmount = 1;
        
        private float timer = 0f;
        private PowerConsumer powerConsumer;

        protected override void Awake()
        {
            base.Awake();
            powerConsumer = GetComponent<PowerConsumer>();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (CanMine())
            {
                timer += deltaTime;
                
                if (timer >= miningInterval)
                {
                    timer = 0f;
                    MineResource();
                }
            }
        }

        public override bool CanWork()
        {
            return base.CanWork() && powerConsumer != null && powerConsumer.CanWork();
        }

        private bool CanMine()
        {
            return CanWork();
        }

        private void MineResource()
        {
            GameManager.Instance?.AddResource(minedResource, miningAmount);
        }

        public float GetMiningProgress()
        {
            return (timer / miningInterval) * 100f;
        }

        public ResourceType GetMinedResource()
        {
            return minedResource;
        }

        public void SetMinedResource(ResourceType type)
        {
            minedResource = type;
        }
    }
}