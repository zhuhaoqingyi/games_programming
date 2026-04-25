using UnityEngine;
using GameCore;

namespace LogisticsSystem
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private GridPosition direction = new GridPosition(1, 0);
        
        private ResourceStack currentResource;
        private float progress = 0f;

        protected virtual void Awake()
        {
            GameManager.Instance?.RegisterConveyor(this);
        }

        protected virtual void OnDestroy()
        {
            GameManager.Instance?.UnregisterConveyor(this);
        }

        public void UpdateBelt(float deltaTime)
        {
            if (currentResource.IsValid())
            {
                progress += speed * deltaTime;
                
                if (progress >= 1f)
                {
                    progress = 0f;
                    MoveResource();
                }
            }
        }

        public bool AcceptResource(ResourceStack resource)
        {
            if (!currentResource.IsValid())
            {
                currentResource = resource;
                progress = 0f;
                return true;
            }
            return false;
        }

        private void MoveResource()
        {
            GridPosition currentPos = GridSystem.GridManager.Instance.WorldToGrid(transform.position);
            GridPosition nextPos = currentPos.Offset(direction.x, direction.y);

            bool transferred = GameManager.Instance?.TryTransferResource(currentResource.type, currentResource.amount, currentPos, nextPos) ?? false;
            
            if (transferred)
            {
                currentResource = new ResourceStack();
            }
        }

        public ResourceStack GetCurrentResource()
        {
            return currentResource;
        }

        public GridPosition GetDirection()
        {
            return direction;
        }

        public void SetDirection(GridPosition dir)
        {
            direction = dir;
        }
    }
}