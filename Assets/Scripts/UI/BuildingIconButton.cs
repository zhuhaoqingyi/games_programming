using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameCore;

namespace UI
{
    public class BuildingIconButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        public Image iconImage;
        public Text buildingNameText;
        public Image selectedIndicator;
        public Image lockedOverlay;
        public Color normalColor = new Color(1f, 1f, 1f);        // 白色
        public Color selectedColor = new Color(0f, 1f, 1f);       // 青色
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f);   // 灰色

        [Header("Data")]
        private BuildingDefinition buildingDef;
        private bool isSelected;
        private bool canAfford;

        public BuildingDefinition BuildingDef => buildingDef;
        public bool IsSelected => isSelected;

        public delegate void OnBuildingSelected(BuildingIconButton button);
        public event OnBuildingSelected OnSelected;

        public void Initialize(BuildingDefinition def, bool canAfford)
        {
            buildingDef = def;
            this.canAfford = canAfford;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (buildingDef == null) return;

            if (buildingNameText != null)
            {
                buildingNameText.text = buildingDef.name;
            }

            if (!string.IsNullOrEmpty(buildingDef.iconPath))
            {
                Sprite sprite = Resources.Load<Sprite>(buildingDef.iconPath);
                if (sprite != null)
                {
                    if (iconImage != null)
                    {
                        iconImage.sprite = sprite;
                        iconImage.enabled = true;
                    }
                    if (selectedIndicator != null)
                    {
                        selectedIndicator.sprite = sprite;
                        selectedIndicator.enabled = true;
                    }
                    if (lockedOverlay != null)
                    {
                        lockedOverlay.sprite = sprite;
                        lockedOverlay.enabled = true;
                    }
                }
            }
            UpdateIconColor();
        }

        public void UpdateAffordability()
        {
            if (GameManager.Instance != null && buildingDef != null)
            {
                canAfford = buildingDef.CanAfford(GameManager.Instance.GetAllResources());
            }
            UpdateIconColor();
        }

        private void UpdateIconColor()
        {
            if (iconImage != null)
            {
                iconImage.color = normalColor;
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.color = selectedColor;
                selectedIndicator.gameObject.SetActive(isSelected);
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.color = lockedColor;
                lockedOverlay.gameObject.SetActive(!canAfford);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateIconColor();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (canAfford)
            {
                OnSelected?.Invoke(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            BuildingUI.Instance?.ShowBuildingTooltip(buildingDef, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            BuildingUI.Instance?.HideBuildingTooltip();
        }
    }
}
