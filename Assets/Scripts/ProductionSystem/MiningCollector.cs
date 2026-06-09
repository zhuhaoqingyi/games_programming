using UnityEngine;
using GameCore;
using PowerSystem;
using GameResources;

namespace ProductionSystem
{
    /// <summary>
    /// 采矿收集器 - 检测太空矿石碰撞，采集并添加到全局库存
    /// 挂载到采矿机物体上，直接通过碰撞检测采集矿石
    /// </summary>
    public class MiningCollector : MonoBehaviour
    {
        [Header("采矿设置")]
        [Tooltip("每次采集的矿石数量")]
        public int miningAmount = 10;

        [Header("采集范围")]
        [Tooltip("采集区域的宽度")]
        public float collectionWidth = 20f;
        [Tooltip("采集区域的高度")]
        public float collectionHeight = 20f;

        private Collider2D collectorCollider;
        private PowerConsumer powerConsumer;

        private void Awake()
        {
            SetupCollider();
            FindPowerConsumer();
        }

        private void SetupCollider()
        {
            BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
                Debug.Log($"[MiningCollector] {name} 添加 BoxCollider2D");
            }
            boxCollider.size = new Vector2(collectionWidth, collectionHeight);
            boxCollider.isTrigger = true;
            collectorCollider = boxCollider;

            // 添加 Rigidbody2D（kinematic）以确保 OnTriggerEnter2D 能正常触发
            // 两个 trigger collider 之间需要至少一个有 Rigidbody2D 才能触发碰撞
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        private void FindPowerConsumer()
        {
            // 查找父物体或兄弟物体上的 PowerConsumer
            powerConsumer = transform.parent?.GetComponent<PowerConsumer>();
            
            if (powerConsumer == null && transform.parent != null)
            {
                powerConsumer = transform.parent.GetComponentInChildren<PowerConsumer>();
            }
            
            if (powerConsumer == null)
            {
                Debug.LogWarning($"[MiningCollector] {name} 没有找到 PowerConsumer 组件，将默认可以工作");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 检查是否有电力供应
            if (!HasPower())
            {
                return;
            }

            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null && !ore.IsCollected())
            {
                CollectOre(ore);
            }
        }

        /// <summary>
        /// 采集矿石并添加到全局库存
        /// </summary>
        private void CollectOre(SpaceOre ore)
        {
            // 标记矿石为已收集
            ore.Collect();
            
            // 增加矿石资源到全局库存
            if (GameManager.Instance != null)
            {
                bool added = GameManager.Instance.AddResource(ResourceType.SpaceOre, miningAmount);
                
                if (enableDebug && added)
                {
                    int currentAmount = GameManager.Instance.GetResourceAmount(ResourceType.SpaceOre);
                    Debug.Log($"[MiningCollector] {name} 采集矿石：+{miningAmount}, 当前总量：{currentAmount}");
                }
            }
        }

        /// <summary>
        /// 检查是否有电力供应
        /// </summary>
        private bool HasPower()
        {
            if (powerConsumer == null)
            {
                return true;
            }
            
            return powerConsumer.CanWork();
        }

        [Header("调试")]
        [Tooltip("是否启用调试日志")]
        public bool enableDebug = false;
    }
}
