using UnityEngine;
using System.Collections.Generic;
using GameCore;
using GridSystem;
using LogisticsSystem;

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
            
            // 从 GridManager 中移除建筑记录，并清理相关资源
            BuildingComponent buildingComp = GetComponent<BuildingComponent>();
            if (buildingComp != null && GridManager.Instance != null)
            {
                var buildingDef = DataConfig.GetBuilding(buildingComp.Type);
                
                // 清理容器注册
                ContainerComponent container = GetComponentInChildren<ContainerComponent>();
                if (container != null && GameManager.Instance != null)
                {
                    Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();
                    foreach (var rc in container.resourceCapacities)
                    {
                        capacities[rc.resourceType] = rc.capacity;
                    }
                    GameManager.Instance.RemoveContainer(capacities, container.GetTotalCapacity());
                    Debug.Log($"[BuildingBase] {name} 容器已注销");
                }
                
                // 清理存储容量
                if (buildingDef != null && buildingDef.storageCapacity > 0 && GameManager.Instance != null)
                {
                    GameManager.Instance.RemoveStorageCapacity(buildingDef.storageCapacity);
                    Debug.Log($"[BuildingBase] {name} 存储容量 -{buildingDef.storageCapacity}");
                }
                
                // 从网格中移除建筑
                GridManager.Instance.RemoveBuilding(buildingComp.GridPosition);
                
                // 强制执行容量限制
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EnforceCapacityLimits();
                }
            }
            
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