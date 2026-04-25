using UnityEngine;

namespace PowerSystem
{
    public class PowerConsumer : MonoBehaviour
    {
        [SerializeField] protected float powerInput = 0f;
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected bool isPowered = true;

        protected virtual void Awake()
        {
            RegisterWithManager();
        }

        protected virtual void OnDestroy()
        {
            UnregisterFromManager();
        }

        protected void RegisterWithManager()
        {
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.RegisterConsumer(this);
            }
        }

        protected void UnregisterFromManager()
        {
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.UnregisterConsumer(this);
            }
        }

        public virtual float GetPowerInput()
        {
            return isActive && isPowered ? powerInput : 0f;
        }

        public virtual bool IsActive()
        {
            return isActive;
        }

        public virtual bool IsPowered()
        {
            return isPowered;
        }

        public virtual void SetActive(bool active)
        {
            isActive = active;
        }

        public virtual void SetPowerAvailable(bool available)
        {
            isPowered = available;
        }

        public virtual bool CanWork()
        {
            return isActive && isPowered;
        }
    }
}