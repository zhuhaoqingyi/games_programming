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
        }

        protected virtual void OnDestroy()
        {
        }

        public void UpdateBelt(float deltaTime)
        {
            if (currentResource.IsValid())
            {
                progress += speed * deltaTime;
                
                if (progress >= 1f)
                {
                    progress = 0f;
                    currentResource = new ResourceStack();
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
