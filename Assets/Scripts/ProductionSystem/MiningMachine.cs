using UnityEngine;
using System.Collections.Generic;
using GameCore;
using PowerSystem;
using GameResources;

namespace ProductionSystem
{
    public class MiningMachine : GridSystem.BuildingComponent
    {
        [Header("采矿设置")]
        public float miningInterval = 2f;
        public int miningAmount = 1;
        public ResourceType minedResource = ResourceType.SpaceOre;

        [Header("组件引用")]
        public MiningCollector collector;

        [Header("测试选项")]
        public bool ignorePowerCheck = false;

        [Header("调试")]
        public bool enableDebug = true;

        private float timer;
        private PowerConsumer powerConsumer;

        protected override void Awake()
        {
            base.Awake();
            powerConsumer = GetComponent<PowerConsumer>();

            if (collector != null)
            {
                collector.Initialize(this);
            }

            LogDebug("MiningMachine 已初始化");
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!CanWork())
            {
                LogDebug("无法工作: CanWork() = false");
                return;
            }

            timer += deltaTime;

            if (timer >= miningInterval)
            {
                timer = 0;
                TryCollectOres();
            }
        }

        public override bool CanWork()
        {
            bool baseCanWork = base.CanWork();
            bool hasPower = powerConsumer != null && powerConsumer.CanWork();

            if (ignorePowerCheck)
            {
                hasPower = true;
            }

            LogDebug($"CanWork 检查: base={baseCanWork}, power={hasPower}");

            return baseCanWork && hasPower;
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