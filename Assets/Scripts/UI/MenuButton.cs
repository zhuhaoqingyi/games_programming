using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuButton : MonoBehaviour
    {
        [Header("按钮状态颜色")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(0, 0.8f, 1f);
        public Color pressedColor = new Color(0, 0.6f, 0.8f);

        [Header("动画效果")]
        public float hoverScale = 1.1f;
        public float pressedScale = 0.95f;

        private Button button;
        private Image buttonImage;
        private Text buttonText;
        private Vector3 originalScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<Text>();
            originalScale = transform.localScale;

            AddButtonListeners();
        }

        private void AddButtonListeners()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
                
                EventTriggerListener listener = gameObject.AddComponent<EventTriggerListener>();
                listener.OnEnter += OnHoverEnter;
                listener.OnExit += OnHoverExit;
                listener.OnDown += OnPressed;
                listener.OnUp += OnReleased;
            }
        }

        private void OnClicked()
        {
            Debug.Log($"按钮点击: {gameObject.name}");
        }

        private void OnHoverEnter()
        {
            if (buttonImage != null)
                buttonImage.color = hoverColor;
            
            if (buttonText != null)
                buttonText.color = hoverColor;
            
            transform.localScale = originalScale * hoverScale;
        }

        private void OnHoverExit()
        {
            if (buttonImage != null)
                buttonImage.color = normalColor;
            
            if (buttonText != null)
                buttonText.color = normalColor;
            
            transform.localScale = originalScale;
        }

        private void OnPressed()
        {
            if (buttonImage != null)
                buttonImage.color = pressedColor;
            
            transform.localScale = originalScale * pressedScale;
        }

        private void OnReleased()
        {
            if (buttonImage != null)
                buttonImage.color = hoverColor;
            
            transform.localScale = originalScale * hoverScale;
        }
    }
}