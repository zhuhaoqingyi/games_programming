using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace LogisticsSystem
{
    public class ConveyorSystem : MonoBehaviour
    {
        private List<ConveyorBelt> belts = new List<ConveyorBelt>();
        private List<SorterComponent> sorters = new List<SorterComponent>();

        public void RegisterBelt(ConveyorBelt belt)
        {
            if (!belts.Contains(belt))
            {
                belts.Add(belt);
            }
        }

        public void UnregisterBelt(ConveyorBelt belt)
        {
            belts.Remove(belt);
        }

        public void RegisterSorter(SorterComponent sorter)
        {
            if (!sorters.Contains(sorter))
            {
                sorters.Add(sorter);
            }
        }

        public void UnregisterSorter(SorterComponent sorter)
        {
            sorters.Remove(sorter);
        }

        public void UpdateSystem(float deltaTime)
        {
            foreach (var belt in belts)
            {
                belt.UpdateBelt(deltaTime);
            }

            foreach (var sorter in sorters)
            {
                sorter.UpdateSorter(deltaTime);
            }
        }

        public bool TryTransferResource(ResourceType type, int amount, GridPosition from, GridPosition to)
        {
            StorageComponent sourceStorage = FindStorageAt(from);
            StorageComponent targetStorage = FindStorageAt(to);

            if (sourceStorage == null || targetStorage == null)
            {
                return false;
            }

            if (!sourceStorage.HasEnoughResource(type, amount))
            {
                return false;
            }

            int removed = sourceStorage.RemoveResource(type, amount);
            if (removed > 0)
            {
                targetStorage.AddResource(type, removed);
                return true;
            }

            return false;
        }

        private StorageComponent FindStorageAt(GridPosition position)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(GridSystem.GridManager.Instance.GridToWorld(position));
            
            foreach (var collider in colliders)
            {
                StorageComponent storage = collider.GetComponent<StorageComponent>();
                if (storage != null)
                {
                    return storage;
                }
            }

            return null;
        }
    }
}