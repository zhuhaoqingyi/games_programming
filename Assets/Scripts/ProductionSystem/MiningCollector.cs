using UnityEngine;
using System.Collections.Generic;
using GameCore;
using PowerSystem;
using GameResources;

namespace ProductionSystem
{
    public class MiningCollector : MonoBehaviour
    {
        [Header("采集设置")]
        public float collectionRadius = 2f;

        private List<SpaceOre> oresInRange = new List<SpaceOre>();
        private MiningMachine miningMachine;

        public void Initialize(MiningMachine machine)
        {
            miningMachine = machine;
            SetupCollider();
        }

        private void SetupCollider()
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = collectionRadius;
            collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null && !ore.IsCollected() && !oresInRange.Contains(ore))
            {
                oresInRange.Add(ore);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            SpaceOre ore = other.GetComponent<SpaceOre>();
            if (ore != null)
            {
                oresInRange.Remove(ore);
            }
        }

        public List<SpaceOre> GetOresInRange()
        {
            oresInRange.RemoveAll(ore => ore == null || ore.IsCollected());
            return oresInRange;
        }

        public int GetOreCount()
        {
            return oresInRange.Count;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, collectionRadius);
        }
    }
}