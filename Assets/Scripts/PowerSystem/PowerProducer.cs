using UnityEngine;

namespace PowerSystem
{
    public class PowerProducer : MonoBehaviour
    {
        [SerializeField] protected float powerOutput = 0f;
        [SerializeField] protected bool isActive = true;

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
            return isActive ? powerOutput : 0f;
        }

        public virtual bool IsActive()
        {
            return isActive;
        }

        public virtual void SetActive(bool active)
        {
            isActive = active;
        }
    }
}