using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameCore;
using System.Collections.Generic;

namespace UI
{
    public class BuildingIconButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        public Image iconImage;
        public Text buildingNameText;
        public Image selectedIndicator;
        public Image lockedOverlay;

        [Header("Colors")]
        public Color normalColor = new Color(1f, 1f, 1f);        // 白色
        public Color selectedColor = new Color(0f, 1f, 1f);       // 青色
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f);   // 灰色

        [Header("Data")]
        private BuildingDefinition buildingDef;
        private bool isSelected;
        private bool canAfford;
        private bool isTooltipVisible;

        [Header("Tooltip Settings")]
        public float tooltipShowDelay = 2.0f;  // 2 秒后显示 tooltip
        public float tooltipHideDelay = 0.3f;  // 延迟隐藏 tooltip
        private float mouseEnterTime;
        private bool isMouseOver;
        private UnityEngine.Coroutine hideTooltipCoroutine;

        public BuildingDefinition BuildingDef => buildingDef;
        public bool IsSelected => isSelected;
        public bool CanAfford => canAfford;

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

        public void Update()
        {
            // 只有在 UI 可见且鼠标悬停时才显示 tooltip
            if (BuildingUI.Instance != null && !BuildingUI.Instance.IsUIVisible)
            {
                // 如果 UI 被隐藏了，重置状态
                if (isMouseOver)
                {
                    isMouseOver = false;
                    isTooltipVisible = false;
                    // 停止隐藏协程
                    if (hideTooltipCoroutine != null)
                        StopCoroutine(hideTooltipCoroutine);
                }
                return;
            }
            
            if (isMouseOver && !isTooltipVisible && Time.time - mouseEnterTime >= tooltipShowDelay)
            {
                isTooltipVisible = true;
                Debug.Log($"[BuildingIconButton] Showing tooltip for {buildingDef?.name}, after {Time.time - mouseEnterTime:F2}s");
                BuildingUI.Instance?.ShowBuildingTooltip(buildingDef, transform.position, this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[BuildingIconButton] OnPointerClick called for {buildingDef?.name ?? "Unknown"}");
            
            bool currentlyCanAfford = buildingDef != null &&
                buildingDef.CanAfford(GameManager.Instance?.GetAllResources() ?? new Dictionary<ResourceType, int>());

            if (currentlyCanAfford)
            {
                canAfford = true;
                Debug.Log($"[BuildingIconButton] Building can be afforded, invoking OnSelected");
                OnSelected?.Invoke(this);
            }
            else
            {
                Debug.LogWarning($"[BuildingIconButton] Building cannot be afforded");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 只有在 UI 可见时才响应鼠标悬停
            if (BuildingUI.Instance != null && !BuildingUI.Instance.IsUIVisible)
            {
                Debug.Log($"[BuildingIconButton] OnPointerEnter ignored - UI is hidden");
                return;
            }
            
            isMouseOver = true;
            mouseEnterTime = Time.time;
            isTooltipVisible = false;  // 重置 tooltip 可见状态
            
            // 停止隐藏协程，因为鼠标又回来了
            if (hideTooltipCoroutine != null)
                StopCoroutine(hideTooltipCoroutine);
            
            Debug.Log($"[BuildingIconButton] OnPointerEnter for {buildingDef?.name}, mouseEnterTime={mouseEnterTime:F2}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
            
            // 停止之前的隐藏协程，避免重复隐藏
            if (hideTooltipCoroutine != null)
                StopCoroutine(hideTooltipCoroutine);
            
            // 延迟一段时间再隐藏 tooltip，给鼠标移动到 tooltip 上的机会
            hideTooltipCoroutine = StartCoroutine(HideTooltipDelayed());
        }

        private System.Collections.IEnumerator HideTooltipDelayed()
        {
            // 增加延迟时间，避免闪烁
            yield return new WaitForSeconds(0.3f);
            
            // 如果仍然没有鼠标悬停，并且 tooltip 仍属于此按钮，才隐藏
            if (!isMouseOver)
            {
                isTooltipVisible = false;
                BuildingUI.Instance?.HideBuildingTooltipIfOwner(this);
            }
            
            hideTooltipCoroutine = null;
        }
    }
}
