using UnityEngine;
using UnityEngine.UI;
using GameCore;

namespace UI
{
    public class ResourceEntryUI : MonoBehaviour
    {
        [Header("UI Components")]
        public Image resourceIcon;
        public Text resourceText;

        private ResourceType resourceType;
        private bool isPowerEntry = false;

        public void Initialize(ResourceType type, Sprite iconSprite, Font textFont, int fontSize, Color textColor)
        {
            resourceType = type;
            isPowerEntry = false;

            var resourceDef = DataConfig.GetResource(type);
            string displayName = resourceDef?.name ?? type.ToString();

            if (resourceIcon != null)
            {
                if (iconSprite != null)
                {
                    resourceIcon.sprite = iconSprite;
                    resourceIcon.enabled = true;
                }
                else
                {
                    resourceIcon.enabled = false;
                }
            }

            if (resourceText != null)
            {
                resourceText.text = $"{displayName}: 0 / 0";

                if (textFont != null)
                {
                    resourceText.font = textFont;
                }

                resourceText.fontSize = fontSize;
                resourceText.color = textColor;
            }
        }

        public void InitializePower(Sprite iconSprite, Font textFont, int fontSize, Color textColor)
        {
            isPowerEntry = true;

            if (resourceIcon != null)
            {
                if (iconSprite != null)
                {
                    resourceIcon.sprite = iconSprite;
                    resourceIcon.enabled = true;
                }
                else
                {
                    resourceIcon.enabled = false;
                }
            }

            if (resourceText != null)
            {
                resourceText.text = "Power: 0 / 0";

                if (textFont != null)
                {
                    resourceText.font = textFont;
                }

                resourceText.fontSize = fontSize;
                resourceText.color = textColor;
            }
        }

        public void UpdateAmount(int amount)
        {
            if (resourceText != null)
            {
                var resourceDef = DataConfig.GetResource(resourceType);
                string displayName = resourceDef?.name ?? resourceType.ToString();
                resourceText.text = $"{displayName}: {amount}";
            }
        }

        public void UpdateAmountWithMax(int amount, int maxAmount, Color normalColor, Color fullColor)
        {
            if (resourceText != null)
            {
                var resourceDef = DataConfig.GetResource(resourceType);
                string displayName = resourceDef?.name ?? resourceType.ToString();
                resourceText.text = $"{displayName}: {amount} / {maxAmount}";

                if (maxAmount > 0 && amount >= maxAmount)
                {
                    resourceText.color = fullColor;
                }
                else
                {
                    resourceText.color = normalColor;
                }
            }
        }

        public void UpdatePowerAmount(int consumed, int generated, Color displayColor)
        {
            if (resourceText != null)
            {
                resourceText.text = $"Power: {consumed} / {generated}";
                resourceText.color = displayColor;
            }
        }

        public ResourceType GetResourceType()
        {
            return resourceType;
        }

        public bool IsPowerEntry()
        {
            return isPowerEntry;
        }
    }
}
