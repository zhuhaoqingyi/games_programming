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
            GameManager.Instance?.RegisterSorter(this);
        }

        protected virtual void OnDestroy()
        {
            GameManager.Instance?.UnregisterSorter(this);
        }

        public void UpdateSorter(float deltaTime)
        {
            if (currentResource.IsValid())
            {
                SortResource();
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

        private void SortResource()
        {
            GridPosition currentPos = GridSystem.GridManager.Instance.WorldToGrid(transform.position);
            GridPosition targetPos;

            if (currentResource.type == targetResource)
            {
                targetPos = currentPos.Offset(outputDirection.x, outputDirection.y);
            }
            else
            {
                targetPos = currentPos.Offset(rejectDirection.x, rejectDirection.y);
            }

            bool transferred = GameManager.Instance?.TryTransferResource(currentResource.type, currentResource.amount, currentPos, targetPos) ?? false;
            
            if (transferred)
            {
                currentResource = new ResourceStack();
            }
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