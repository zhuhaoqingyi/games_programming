using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace LogisticsSystem
{
    public class SorterComponent : MonoBehaviour
    {
        [SerializeField] private ResourceType targetResource;
        [SerializeField] private GridPosition outputDirection = new GridPosition(1, 0);
        [SerializeField] private GridPosition rejectDirection = new GridPosition(0, 1);
        
        private ResourceStack currentResource;

        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        public void UpdateSorter(float deltaTime)
        {
            if (currentResource.IsValid())
            {
                currentResource = new ResourceStack();
            }
        }

        public bool AcceptResource(ResourceStack resource)
        {
            if (!currentResource.IsValid())
            {
                currentResource = resource;
                return true;
            }
            return false;
        }

        public ResourceType GetTargetResource()
        {
            return targetResource;
        }

        public void SetTargetResource(ResourceType type)
        {
            targetResource = type;
        }
    }
}
