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
        public GameObject selectedIndicator;
        public GameObject lockedOverlay;
        public Color normalColor = Color.white;
        public Color selectedColor = Color.yellow;
        public Color lockedColor = Color.gray;

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

            if (iconImage != null)
            {
                if (!string.IsNullOrEmpty(buildingDef.iconPath))
                {
                    Sprite sprite = Resources.Load<Sprite>(buildingDef.iconPath);
                    if (sprite != null)
                    {
                        iconImage.sprite = sprite;
                        iconImage.enabled = true;
                    }
                    else
                    {
                        iconImage.enabled = false;
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            UpdateAffordability();
        }

        public void UpdateAffordability()
        {
            if (GameManager.Instance != null && buildingDef != null)
            {
                canAfford = buildingDef.CanAfford(GameManager.Instance.GetAllResources());
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!canAfford);
            }

            if (iconImage != null)
            {
                iconImage.color = canAfford ? normalColor : lockedColor;
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(selected);
            }
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
