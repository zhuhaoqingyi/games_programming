using UnityEngine;
using GameCore;
using GridSystem;

namespace PowerSystem
{
    public class PowerProducer : MonoBehaviour
    {
        [SerializeField] protected float powerOutput = 0f;
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected ResourceType fuelResource = ResourceType.None;
        [SerializeField] protected int fuelConsumptionPerSecond = 0;

        private float fuelTimer = 0f;
        private BuildingComponent buildingComponent;

        protected virtual void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
            RegisterWithManager();
        }

        protected virtual void OnDestroy()
        {
            UnregisterFromManager();
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            if (fuelResource != ResourceType.None && fuelConsumptionPerSecond > 0)
            {
                fuelTimer += Time.deltaTime;
                if (fuelTimer >= 1f)
                {
                    fuelTimer -= 1f;
                    if (GameManager.Instance != null && GameManager.Instance.HasEnoughResource(fuelResource, fuelConsumptionPerSecond))
                    {
                        GameManager.Instance.RemoveResource(fuelResource, fuelConsumptionPerSecond);
                    }
                    else
                    {
                        isActive = false;
                    }
                }
            }
        }

        protected void RegisterWithManager()
        {
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.RegisterProducer(this);
            }
        }

        protected void UnregisterFromManager()
        {
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.UnregisterProducer(this);
            }
        }

        public virtual float GetPowerOutput()
        {
            return isActive && IsPowered() ? powerOutput : 0f;
        }

        public virtual bool IsActive()
        {
            return isActive;
        }

        public virtual void SetActive(bool active)
        {
            isActive = active;
        }

        private bool IsPowered()
        {
            if (buildingComponent == null) return true;
            return buildingComponent.Status == BuildingStatus.Active;
        }
    }
}