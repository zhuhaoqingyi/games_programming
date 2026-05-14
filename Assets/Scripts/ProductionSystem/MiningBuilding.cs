using UnityEngine;
using GameResources;

namespace ProductionSystem
{
    public class MiningBuilding : BuildingBase
    {
        [Header("采矿设置")]
        public int collectionRange = 2;
        public float collectionInterval = 2f;

        private float timer;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[MiningBuilding] {name} 检测到碰撞: {other.gameObject.name}");

            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null && !ore.IsCollected())
            {
                Debug.Log($"[MiningBuilding] {name} 检测到矿石，造成伤害并销毁");
                TakeDamage(ore.damageToBuilding);
                ore.Collect();
            }
        }

        protected override void SetupCollider()
        {
            base.SetupCollider();
        }

        private void Update()
        {
            if (!CanMine()) return;

            timer += Time.deltaTime;

            if (timer >= collectionInterval)
            {
                timer = 0;
                CollectResourcesInRange();
            }
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