using UnityEngine;
using GameCore;
using System.Collections.Generic;

namespace GridSystem
{
    public class ContainerComponent : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceCapacity
        {
            public ResourceType resourceType;
            public int capacity;
        }

        [Header("Resource Capacities")]
        public List<ResourceCapacity> resourceCapacities = new List<ResourceCapacity>();

        public int GetCapacity(ResourceType type)
        {
            foreach (var rc in resourceCapacities)
            {
                if (rc.resourceType == type)
                    return rc.capacity;
            }
            return 0;
        }

        public int GetTotalCapacity()
        {
            int total = 0;
            foreach (var rc in resourceCapacities)
            {
                total += rc.capacity;
            }
            return total;
        }
    }
}
