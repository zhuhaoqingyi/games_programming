using UnityEngine;
using UnityEngine.UI;
using GameCore;
using System.Collections.Generic;

namespace UI
{
    public class BuildingCategoryPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public Image categoryIcon;
        public Transform contentContainer;
        public GameObject buildingIconPrefab;
        public GameObject scrollView;
        public ScrollRect scrollRect;

        [Header("Settings")]
        public BuildingCategory category;
        public int iconsPerRow = 4;
        public float iconSpacing = 10f;
        public float iconSize = 80f;

        private List<BuildingIconButton> iconButtons = new List<BuildingIconButton>();
        private bool isExpanded = true;
        private int currentPage = 0;
        private int totalPages = 0;
        private List<BuildingDefinition> buildingsInCategory = new List<BuildingDefinition>();

        public bool IsExpanded => isExpanded;
        public BuildingCategory Category => category;

        private void Start()
        {
            InitializeCategory();
        }

        public void InitializeCategory()
        {
            buildingsInCategory = DataConfig.GetBuildingsByCategory(category);

            GenerateBuildingIcons();
            UpdatePagination();
        }

        private void GenerateBuildingIcons()
        {
            ClearIcons();

            if (buildingIconPrefab == null)
            {
                Debug.LogError("buildingIconPrefab is not assigned in BuildingCategoryPanel");
                return;
            }

            foreach (var building in buildingsInCategory)
            {
                GameObject iconObj = Instantiate(buildingIconPrefab, contentContainer);
                BuildingIconButton iconButton = iconObj.GetComponent<BuildingIconButton>();

                if (iconButton != null)
                {
                    bool canAfford = building.CanAfford(GameManager.Instance?.GetAllResources() ?? new Dictionary<ResourceType, int>());
                    iconButton.Initialize(building, canAfford);
                    iconButton.OnSelected += OnBuildingIconSelected;
                    iconButtons.Add(iconButton);
                }
            }

            ArrangeIcons();
        }

        private void ArrangeIcons()
        {
            if (contentContainer == null) return;

            RectTransform containerRect = contentContainer.GetComponent<RectTransform>();
            if (containerRect == null) return;

            int rowCount = Mathf.CeilToInt((float)iconButtons.Count / iconsPerRow);
            float totalHeight = rowCount * (iconSize + iconSpacing);
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, totalHeight);

            for (int i = 0; i < iconButtons.Count; i++)
            {
                int row = i / iconsPerRow;
                int col = i % iconsPerRow;

                RectTransform iconRect = iconButtons[i].GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    float x = col * (iconSize + iconSpacing) + iconSpacing;
                    float y = -row * (iconSize + iconSpacing) - iconSpacing;
                    iconRect.anchoredPosition = new Vector2(x, y);
                    iconRect.sizeDelta = new Vector2(iconSize, iconSize);
                }
            }
        }

        private void ClearIcons()
        {
            foreach (var button in iconButtons)
            {
                if (button != null)
                {
                    button.OnSelected -= OnBuildingIconSelected;
                    Destroy(button.gameObject);
                }
            }
            iconButtons.Clear();
        }

        private void OnBuildingIconSelected(BuildingIconButton button)
        {
            BuildingUI.Instance?.SelectBuilding(button);
        }

        public void ToggleExpand()
        {
            isExpanded = !isExpanded;
            if (scrollView != null)
            {
                scrollView.SetActive(isExpanded);
            }
        }

        public void SetExpanded(bool expanded)
        {
            isExpanded = expanded;
            if (scrollView != null)
            {
                scrollView.SetActive(isExpanded);
            }
        }

        public void UpdatePagination()
        {
            int iconsPerPage = iconsPerRow * 3;
            totalPages = Mathf.Max(1, Mathf.CeilToInt((float)iconButtons.Count / iconsPerPage));
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
        }

        public void ScrollToPage(int page)
        {
            currentPage = Mathf.Clamp(page, 0, totalPages - 1);
            if (scrollRect != null)
            {
                float normalizedPosition = 1f - (float)currentPage / (totalPages - 1);
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
            }
        }

        public void HandleScroll(float scrollDelta)
        {
            if (scrollDelta > 0)
            {
                ScrollToPage(currentPage - 1);
            }
            else if (scrollDelta < 0)
            {
                ScrollToPage(currentPage + 1);
            }
        }

        public void UpdateAffordability()
        {
            foreach (var button in iconButtons)
            {
                button.UpdateAffordability();
            }
        }

        public void DeselectAll()
        {
            foreach (var button in iconButtons)
            {
                button.SetSelected(false);
            }
        }

        public void Refresh()
        {
            GenerateBuildingIcons();
        }
    }
}
