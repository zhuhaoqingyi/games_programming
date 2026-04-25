using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace LogisticsSystem
{
    public class StorageComponent : MonoBehaviour
    {
        [SerializeField] private int capacity = 100;
        [SerializeField] private bool isGlobalStorage = false;
        
        private Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();

        public int Capacity => capacity;
        public int UsedCapacity { get; private set; }
        public int RemainingCapacity => capacity - UsedCapacity;

        protected virtual void Awake()
        {
            if (isGlobalStorage)
            {
                GameManager.Instance?.RegisterStorage(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (isGlobalStorage)
            {
                GameManager.Instance?.UnregisterStorage(this);
            }
        }

        public int AddResource(ResourceType type, int amount)
        {
            if (amount <= 0 || RemainingCapacity <= 0) return 0;

            int actualAmount = Mathf.Min(amount, RemainingCapacity);

            if (!storedResources.ContainsKey(type))
            {
                storedResources[type] = 0;
            }
            storedResources[type] += actualAmount;
            UsedCapacity += actualAmount;

            return actualAmount;
        }

        public int RemoveResource(ResourceType type, int amount)
        {
            if (amount <= 0) return 0;

            if (!storedResources.ContainsKey(type) || storedResources[type] == 0)
            {
                return 0;
            }

            int actualAmount = Mathf.Min(amount, storedResources[type]);
            storedResources[type] -= actualAmount;
            UsedCapacity -= actualAmount;

            if (storedResources[type] == 0)
            {
                storedResources.Remove(type);
            }

            return actualAmount;
        }

        public int GetResourceAmount(ResourceType type)
        {
            return storedResources.TryGetValue(type, out int amount) ? amount : 0;
        }

        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResourceAmount(type) >= amount;
        }

        public Dictionary<ResourceType, int> GetStoredResources()
        {
            return new Dictionary<ResourceType, int>(storedResources);
        }
    }
}