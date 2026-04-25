using System.Collections.Generic;
using GameCore;

namespace LogisticsSystem
{
    public class StorageManager
    {
        private Dictionary<ResourceType, int> globalInventory = new Dictionary<ResourceType, int>();
        private List<StorageComponent> storageComponents = new List<StorageComponent>();

        public int GetResourceAmount(ResourceType type)
        {
            int total = globalInventory.TryGetValue(type, out int amount) ? amount : 0;
            
            foreach (var storage in storageComponents)
            {
                total += storage.GetResourceAmount(type);
            }
            
            return total;
        }

        public bool AddResource(ResourceType type, int amount)
        {
            if (amount <= 0) return false;

            int totalNeeded = amount;
            int added = 0;

            foreach (var storage in storageComponents)
            {
                if (totalNeeded <= 0) break;
                
                int addedToStorage = storage.AddResource(type, totalNeeded);
                added += addedToStorage;
                totalNeeded -= addedToStorage;
            }

            if (totalNeeded > 0)
            {
                if (!globalInventory.ContainsKey(type))
                {
                    globalInventory[type] = 0;
                }
                globalInventory[type] += totalNeeded;
                added += totalNeeded;
            }

            return added > 0;
        }

        public bool RemoveResource(ResourceType type, int amount)
        {
            if (amount <= 0) return false;

            int totalAvailable = GetResourceAmount(type);
            if (totalAvailable < amount) return false;

            int totalRemoved = 0;
            int remaining = amount;

            foreach (var storage in storageComponents)
            {
                if (remaining <= 0) break;
                
                int removedFromStorage = storage.RemoveResource(type, remaining);
                totalRemoved += removedFromStorage;
                remaining -= removedFromStorage;
            }

            if (remaining > 0)
            {
                if (globalInventory.ContainsKey(type))
                {
                    globalInventory[type] -= remaining;
                    totalRemoved += remaining;
                }
            }

            return totalRemoved == amount;
        }

        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResourceAmount(type) >= amount;
        }

        public void RegisterStorage(StorageComponent storage)
        {
            if (!storageComponents.Contains(storage))
            {
                storageComponents.Add(storage);
            }
        }

        public void UnregisterStorage(StorageComponent storage)
        {
            storageComponents.Remove(storage);
        }

        public Dictionary<ResourceType, int> GetAllResources()
        {
            Dictionary<ResourceType, int> allResources = new Dictionary<ResourceType, int>();

            foreach (var storage in storageComponents)
            {
                foreach (var item in storage.GetStoredResources())
                {
                    if (!allResources.ContainsKey(item.Key))
                    {
                        allResources[item.Key] = 0;
                    }
                    allResources[item.Key] += item.Value;
                }
            }

            foreach (var item in globalInventory)
            {
                if (!allResources.ContainsKey(item.Key))
                {
                    allResources[item.Key] = 0;
                }
                allResources[item.Key] += item.Value;
            }

            return allResources;
        }

        public int GetTotalStorageCapacity()
        {
            int total = 0;
            foreach (var storage in storageComponents)
            {
                total += storage.Capacity;
            }
            return total;
        }

        public int GetUsedStorageCapacity()
        {
            int used = 0;
            foreach (var storage in storageComponents)
            {
                used += storage.UsedCapacity;
            }
            return used;
        }
    }
}