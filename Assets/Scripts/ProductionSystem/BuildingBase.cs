using UnityEngine;
using GameCore;
using GridSystem;

namespace ProductionSystem
{
    public class BuildingBase : MonoBehaviour
    {
        [Header("血量设置")]
        public int maxHealth = 100;
        public int currentHealth;

        protected Collider2D buildingCollider;
        protected Rigidbody2D rb;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            SetupRigidbody();
            SetupCollider();
            Debug.Log($"[BuildingBase] {name} 初始化完成，血量: {currentHealth}/{maxHealth}");
        }

        protected virtual void SetupRigidbody()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                Debug.Log($"[BuildingBase] {name} 添加 Rigidbody2D");
            }
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        protected virtual void SetupCollider()
        {
            buildingCollider = gameObject.GetComponent<Collider2D>();
            if (buildingCollider == null)
            {
                buildingCollider = gameObject.AddComponent<BoxCollider2D>();
                Debug.Log($"[BuildingBase] {name} 添加 BoxCollider2D");
            }
            buildingCollider.isTrigger = true;

            // 根据建筑定义设置 Collider 大小
            BuildingComponent buildingComp = GetComponent<BuildingComponent>();
            if (buildingComp != null)
            {
                var def = DataConfig.GetBuilding(buildingComp.Type);
                if (def != null)
                {
                    float cellSize = GridManager.Instance != null ? GridManager.Instance.cellSize : 1f;
                    Vector2 size = new Vector2(def.width * cellSize, def.height * cellSize);
                    ((BoxCollider2D)buildingCollider).size = size;
                    Debug.Log($"[BuildingBase] {name} 碰撞体大小: {size} (建筑: {def.width}x{def.height})");
                }
            }

            Debug.Log($"[BuildingBase] {name} 碰撞体设置完成");
        }

        public virtual void TakeDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log($"[BuildingBase] {name} 受到 {damage} 点伤害，当前血量: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }
        }

        protected virtual void DestroyBuilding()
        {
            Debug.Log($"[BuildingBase] {name} 被摧毁");
            Destroy(gameObject);
        }

        public virtual bool CanWork()
        {
            return currentHealth > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}