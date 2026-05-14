using UnityEngine;
using GameResources;

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
            Debug.Log("[MiningCollector] 已初始化");
        }

        private void SetupRigidbody()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                Debug.Log("[MiningCollector] 自动添加 Rigidbody2D");
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
                Debug.Log("[MiningCollector] 自动添加 BoxCollider2D");
            }
            collectorCollider.isTrigger = true;
            Debug.Log($"[MiningCollector] 碰撞体设置完成，类型: {collectorCollider.GetType().Name}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[MiningCollector] 检测到碰撞: {other.gameObject.name}, Tag: {other.tag}");
            
            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null && !ore.IsCollected())
            {
                Debug.Log($"[MiningCollector] 矿石接触，开始采集: {ore.name}");
                ore.Collect();
            }
            else if (ore != null)
            {
                Debug.Log($"[MiningCollector] 矿石已被采集: {ore.name}");
            }
            else
            {
                Debug.Log($"[MiningCollector] 不是矿石对象，尝试获取组件失败");
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