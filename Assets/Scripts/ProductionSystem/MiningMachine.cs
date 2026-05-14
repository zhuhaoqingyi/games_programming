using UnityEngine;
using System.Collections.Generic;
using GameResources;

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
                LogDebug("MiningMachine 运行中...");
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
                Collider2D col = collector.GetComponent<Collider2D>();
                if (col != null)
                {
                    Gizmos.DrawCube(collector.transform.position, Vector3.one * 2f);
                }
            }
        }
    }
}