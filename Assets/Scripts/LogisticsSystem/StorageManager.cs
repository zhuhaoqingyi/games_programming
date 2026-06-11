using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace LogisticsSystem
{
    public class StorageManager
    {
        private Dictionary<ResourceType, int> globalInventory = new Dictionary<ResourceType, int>();
        private Dictionary<ResourceType, int> resourceCapacities = new Dictionary<ResourceType, int>();
        private int totalCapacity = 0;

        public int TotalCapacity => totalCapacity;

        public int GetResourceAmount(ResourceType type)
        {
            return globalInventory.TryGetValue(type, out int amount) ? amount : 0;
        }

        public int GetResourceCapacity(ResourceType type)
        {
            return resourceCapacities.TryGetValue(type, out int cap) ? cap : 0;
        }

        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var item in globalInventory.Values)
            {
                total += item;
            }
            return total;
        }

        public void AddSimpleStorage(int amount)
        {
            totalCapacity += amount;
            EnforceCapacityLimits();
        }

        public void RemoveSimpleStorage(int amount)
        {
            totalCapacity = Mathf.Max(0, totalCapacity - amount);
            EnforceCapacityLimits();
        }

        public void AddContainer(Dictionary<ResourceType, int> capacities, int totalCapacityAdd)
        {
            foreach (var kvp in capacities)
            {
                if (!resourceCapacities.ContainsKey(kvp.Key))
                {
                    resourceCapacities[kvp.Key] = 0;
                }
                resourceCapacities[kvp.Key] += kvp.Value;
            }
            totalCapacity += totalCapacityAdd;
            EnforceCapacityLimits();
        }

        public void RemoveContainer(Dictionary<ResourceType, int> capacities, int totalCapacityRemove)
        {
            foreach (var kvp in capacities)
            {
                if (resourceCapacities.ContainsKey(kvp.Key))
                {
                    resourceCapacities[kvp.Key] = Mathf.Max(0, resourceCapacities[kvp.Key] - kvp.Value);
                    if (resourceCapacities[kvp.Key] <= 0)
                    {
                        resourceCapacities.Remove(kvp.Key);
                    }
                }
            }
            totalCapacity = Mathf.Max(0, totalCapacity - totalCapacityRemove);
            EnforceCapacityLimits();
        }

        public void EnforceCapacityLimits()
        {
            foreach (var kvp in resourceCapacities)
            {
                ResourceType type = kvp.Key;
                int capacity = kvp.Value;
                int amount = GetResourceAmount(type);

                if (amount > capacity)
                {
                    globalInventory[type] = capacity;
                    if (globalInventory[type] <= 0)
                    {
                        globalInventory.Remove(type);
                    }
                }
            }

            int totalItems = GetTotalItemCount();
            if (totalItems > totalCapacity && totalCapacity > 0)
            {
                int excess = totalItems - totalCapacity;
                foreach (var kvp in new List<KeyValuePair<ResourceType, int>>(globalInventory))
                {
                    if (excess <= 0) break;
                    int canRemove = Mathf.Min(kvp.Value, excess);
                    globalInventory[kvp.Key] -= canRemove;
                    excess -= canRemove;
                    if (globalInventory[kvp.Key] <= 0)
                    {
                        globalInventory.Remove(kvp.Key);
                    }
                }
            }
        }

        public bool AddResource(ResourceType type, int amount)
        {
            return AddResource(type, amount, checkCapacity: true);
        }

        public bool AddResource(ResourceType type, int amount, bool checkCapacity)
        {
            if (amount <= 0) return false;

            if (checkCapacity)
            {
                int totalItems = GetTotalItemCount();
                if (totalCapacity > 0 && totalItems >= totalCapacity) return false;

                int capacity = GetResourceCapacity(type);
                int current = GetResourceAmount(type);
                int canAdd = Mathf.Min(amount, capacity - current);
                if (totalCapacity > 0)
                {
                    canAdd = Mathf.Min(canAdd, totalCapacity - totalItems);
                }

                if (canAdd <= 0) return false;

                if (!globalInventory.ContainsKey(type))
                {
                    globalInventory[type] = 0;
                }
                globalInventory[type] += canAdd;
                return true;
            }
            else
            {
                // 不检查容量，直接添加（用于初始资源设置）
                if (!globalInventory.ContainsKey(type))
                {
                    globalInventory[type] = 0;
                }
                globalInventory[type] += amount;
                return true;
            }
        }

        public bool RemoveResource(ResourceType type, int amount)
        {
            if (amount <= 0) return false;

            int available = GetResourceAmount(type);
            if (available < amount) return false;

            globalInventory[type] -= amount;
            return true;
        }

        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResourceAmount(type) >= amount;
        }

        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(globalInventory);
        }

        public bool IsResourceOverCapacity(ResourceType type)
        {
            int cap = GetResourceCapacity(type);
            return cap > 0 && GetResourceAmount(type) > cap;
        }
    }
}
