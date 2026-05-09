using UnityEngine;

namespace ProductionSystem
{
    public class MiningBuilding : MonoBehaviour
    {
        [Header("采矿设置")]
        public int collectionRange = 2;
        public float collectionInterval = 2f;

        private float timer;

        protected virtual void Awake()
        {
        }

        protected virtual void Update()
        {
            if (!CanMine()) return;
            
            timer += Time.deltaTime;
            
            if (timer >= collectionInterval)
            {
                timer = 0;
                CollectResourcesInRange();
            }
        }

        public virtual bool CanWork()
        {
            return true;
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