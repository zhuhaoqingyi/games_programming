using UnityEngine;
using UnityEngine.UI;
using GridSystem;

namespace PowerSystem
{
    public class PowerConsumer : MonoBehaviour
    {
        [SerializeField] protected float powerInput = 0f;
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected bool isPowered = true;
        [SerializeField] protected int priority = 5;

        [Header("Power Shortage Marker")]
        [Tooltip("Icon displayed when building has no power")]
        public Sprite powerShortageIcon;
        public Vector3 markerOffset = new Vector3(0, 0, -1f);

        private GameObject powerShortageMarker;
        private Image markerImage;

        public int Priority => priority;

        protected virtual void Awake()
        {
            RegisterWithManager();
            CreatePowerShortageMarker();
        }

        protected virtual void OnDestroy()
        {
            UnregisterFromManager();
        }

        private void CreatePowerShortageMarker()
        {
            if (powerShortageIcon == null) return;

            powerShortageMarker = new GameObject("PowerShortageMarker");
            powerShortageMarker.transform.SetParent(transform);
            powerShortageMarker.transform.localPosition = markerOffset;
            powerShortageMarker.transform.localScale = Vector3.one;

            float cellSize = GridManager.Instance != null ? GridManager.Instance.cellSize : 1f;
            RectTransform rectTransform = powerShortageMarker.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            Canvas canvas = powerShortageMarker.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = powerShortageMarker.AddComponent<CanvasScaler>();
            scaler.scaleFactor = 1f;

            GraphicRaycaster raycaster = powerShortageMarker.AddComponent<GraphicRaycaster>();

            markerImage = powerShortageMarker.AddComponent<Image>();
            markerImage.sprite = powerShortageIcon;
            markerImage.color = Color.white;

            powerShortageMarker.SetActive(false);
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
            return powerInput;
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
            UpdatePowerShortageMarker();
        }

        private void UpdatePowerShortageMarker()
        {
            if (powerShortageMarker != null)
            {
                powerShortageMarker.SetActive(!isPowered && isActive);
            }
        }

        public virtual bool CanWork()
        {
            return isActive && isPowered;
        }
    }
}
