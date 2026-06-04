using UnityEngine;
using GameResources;
using GameCore;

namespace ProductionSystem
{
    public class MiningCollector : MonoBehaviour
    {
        private Collider2D collectorCollider;
        private Rigidbody2D rb;

        private void Awake()
        {
            SetupRigidbody();
            SetupCollider();
        }

        public void Initialize(MiningMachine machine)
        {
        }

        private void SetupRigidbody()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        private void SetupCollider()
        {
            collectorCollider = gameObject.GetComponent<Collider2D>();
            if (collectorCollider == null)
            {
                collectorCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            collectorCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null && !ore.IsCollected())
            {
                ore.Collect();
                
                // 增加矿石资源
                if (GameManager.Instance != null)
                {
                    int beforeAmount = GameManager.Instance.GetResourceAmount(ResourceType.SpaceOre);
                    GameManager.Instance.AddResource(ResourceType.SpaceOre, 10);
                    int afterAmount = GameManager.Instance.GetResourceAmount(ResourceType.SpaceOre);
                    Debug.Log($"[MiningCollector] 采集矿石: {beforeAmount} → {afterAmount} (+10)");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            if (collectorCollider is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position, box.size);
            }
            else if (collectorCollider is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position, circle.radius);
            }
            else
            {
                Gizmos.DrawCube(transform.position, Vector3.one * 2f);
            }
        }
    }
}
