using UnityEngine;
using System.Collections.Generic;

namespace ProductionSystem
{
    public class MiningMachine : MonoBehaviour
    {
        [Header("采矿设置")]
        public float miningInterval = 2f;
        public int miningAmount = 1;

        [Header("组件引用")]
        public MiningCollector collector;

        [Header("测试选项")]
        public bool ignorePowerCheck = false;

        [Header("调试")]
        public bool enableDebug = true;

        private float timer;

        protected virtual void Awake()
        {
            if (collector != null)
            {
                collector.Initialize(this);
            }

            LogDebug("MiningMachine 已初始化");
        }

        protected virtual void Update()
        {
            if (!CanWork())
            {
                LogDebug("无法工作: CanWork() = false");
                return;
            }

            timer += Time.deltaTime;

            if (timer >= miningInterval)
            {
                timer = 0;
                TryCollectOres();
            }
        }

        public virtual bool CanWork()
        {
            LogDebug($"CanWork 检查: base=true, power=true");
            return true;
        }

        private bool CanMine()
        {
            return CanWork();
        }

        private void TryCollectOres()
        {
            if (collector == null)
            {
                LogDebug("Collector 为空!");
                return;
            }

            List<SpaceOre> ores = collector.GetOresInRange();
            LogDebug($"检测到 {ores.Count} 个矿石在范围内");

            if (ores.Count == 0) return;

            foreach (SpaceOre ore in ores)
            {
                if (ore != null && !ore.IsCollected())
                {
                    LogDebug($"采集矿石 at {ore.transform.position}");
                    ore.Collect();
                }
            }
        }

        public int GetCollectorOreCount()
        {
            return collector != null ? collector.GetOreCount() : 0;
        }

        private void LogDebug(string message)
        {
            if (enableDebug)
            {
                Debug.Log($"[MiningMachine] {message}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (collector != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawSphere(collector.transform.position, collector.collectionRadius);
            }
        }
    }
}