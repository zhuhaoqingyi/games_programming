using UnityEngine;
using GameCore;
using PowerSystem;
using GameResources;

namespace ProductionSystem
{
    public class MiningBuilding : GridSystem.BuildingComponent
    {
        [Header("采矿设置")]
        public int collectionRange = 2;
        public float collectionInterval = 2f;
        public ResourceType minedResource = ResourceType.SpaceOre;
        
        private float timer;
        private PowerConsumer powerConsumer;

        protected override void Awake()
        {
            base.Awake();
            powerConsumer = GetComponent<PowerConsumer>();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!CanMine()) return;
            
            timer += deltaTime;
            
            if (timer >= collectionInterval)
            {
                timer = 0;
                CollectResourcesInRange();
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

        private void CollectResourcesInRange()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                transform.position,
                new Vector2(collectionRange, collectionRange),
                0f
            );
            
            foreach (Collider2D collider in hitColliders)
            {
                SpaceOre ore = collider.GetComponent<SpaceOre>();
                if (ore != null && !ore.IsCollected())
                {
                    ore.Collect();
                }
            }
        }

        public int GetCollectionRange()
        {
            return collectionRange;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, new Vector3(collectionRange, collectionRange, 0.1f));
        }
    }
}