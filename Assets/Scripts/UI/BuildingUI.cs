using UnityEngine;
using UnityEngine.UI;
using GameCore;
using GridSystem;
using System.Collections.Generic;

namespace UI
{
    public class BuildingUI : MonoBehaviour
    {
        public static BuildingUI Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject mainPanel;
        public BuildingCategoryPanel[] categoryPanels;
        public GameObject tooltipPanel;

        [Header("Tooltip Components")]
        public Text tooltipName;
        public Text tooltipDescription;
        public Text tooltipCost;
        public Text tooltipStats;
        public RectTransform tooltipRect;

        [Header("Building Placer")]
        public BuildingPlacer buildingPlacer;
        public Camera mainCamera;

        [Header("Settings")]
        public KeyCode toggleKey = KeyCode.B;
        public float scrollSensitivity = 1f;

        private BuildingIconButton selectedButton;
        private BuildingCategoryPanel currentCategory;
        private bool isUIVisible = true;
        private bool isBuildingMode = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializePanels();
            HideBuildingTooltip();
        }

        private void Update()
        {
            HandleToggleUI();
            HandleScroll();
            HandleCancelBuilding();

            if (isBuildingMode && Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding();
            }
        }

        private void InitializePanels()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel != null)
                {
                    panel.InitializeCategory();
                }
            }
        }

        private void HandleToggleUI()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleUI();
            }
        }

        private void HandleScroll()
        {
            if (!isUIVisible) return;

            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.1f)
            {
                if (currentCategory != null)
                {
                    currentCategory.HandleScroll(scrollDelta * scrollSensitivity);
                }
                else
                {
                    foreach (var panel in categoryPanels)
                    {
                        if (panel != null && panel.IsExpanded)
                        {
                            panel.HandleScroll(scrollDelta * scrollSensitivity);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleCancelBuilding()
        {
            if (isBuildingMode && Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuildingMode();
            }
            else if (isBuildingMode && Input.GetMouseButtonDown(1))
            {
                CancelBuildingMode();
            }
        }

        public void ToggleUI()
        {
            isUIVisible = !isUIVisible;
            if (mainPanel != null)
            {
                mainPanel.SetActive(isUIVisible);
            }

            if (!isUIVisible && isBuildingMode)
            {
                CancelBuildingMode();
            }
        }

        public void ShowUI()
        {
            isUIVisible = true;
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
            }
        }

        public void HideUI()
        {
            isUIVisible = false;
            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }

            if (isBuildingMode)
            {
                CancelBuildingMode();
            }
        }

        public void SelectBuilding(BuildingIconButton button)
        {
            DeselectAll();

            selectedButton = button;
            selectedButton.SetSelected(true);

            foreach (var panel in categoryPanels)
            {
                if (panel != null && panel.Category == button.BuildingDef.category)
                {
                    currentCategory = panel;
                    break;
                }
            }

            EnterBuildingMode(button.BuildingDef);
        }

        private void DeselectAll()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel != null)
                {
                    panel.DeselectAll();
                }
            }
            selectedButton = null;
        }

        private void EnterBuildingMode(BuildingDefinition def)
        {
            isBuildingMode = true;

            if (buildingPlacer != null)
            {
                buildingPlacer.SelectBuilding(def.type);
            }

            if (GridRenderer.Instance != null)
            {
                GridRenderer.Instance.ShowGrid();
            }
        }

        private void TryPlaceBuilding()
        {
            if (!isBuildingMode || selectedButton == null) return;

            var buildingDef = selectedButton.BuildingDef;
            if (buildingDef == null) return;

            if (GameManager.Instance != null && buildingDef.CanAfford(GameManager.Instance.GetAllResources()))
            {
                if (buildingPlacer != null)
                {
                }
            }
        }

        public void OnBuildingPlacedSuccess(GridPosition position, BuildingType type)
        {
            var buildingDef = DataConfig.GetBuilding(type);
            if (buildingDef != null && GameManager.Instance != null)
            {
                foreach (var cost in buildingDef.costs)
                {
                    GameManager.Instance.RemoveResource(cost.resourceType, cost.amount);
                }

                UpdateAllAffordability();
            }

            CancelBuildingMode();
        }

        public void CancelBuildingMode()
        {
            isBuildingMode = false;

            if (buildingPlacer != null)
            {
                buildingPlacer.CancelPlacement();
            }

            if (GridRenderer.Instance != null)
            {
                GridRenderer.Instance.HideGrid();
            }

            DeselectAll();
        }

        public void UpdateAllAffordability()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel != null)
                {
                    panel.UpdateAffordability();
                }
            }
        }

        public void ShowBuildingTooltip(BuildingDefinition def, Vector3 screenPosition)
        {
            if (tooltipPanel == null || def == null) return;

            tooltipPanel.SetActive(true);

            if (tooltipName != null)
            {
                tooltipName.text = def.name;
            }

            if (tooltipDescription != null)
            {
                tooltipDescription.text = def.description;
            }

            if (tooltipCost != null)
            {
                string costText = "建造消耗:\n";
                if (def.costs.Count > 0)
                {
                    foreach (var cost in def.costs)
                    {
                        var resourceDef = DataConfig.GetResource(cost.resourceType);
                        costText += $"- {resourceDef?.name ?? cost.resourceType.ToString()}: {cost.amount}\n";
                    }
                }
                else
                {
                    costText += "无";
                }
                tooltipCost.text = costText;
            }

            if (tooltipStats != null)
            {
                string statsText = $"尺寸: {def.width}x{def.height}\n";
                if (def.powerConsumption > 0)
                {
                    statsText += $"耗电: {def.powerConsumption}\n";
                }
                if (def.powerProduction > 0)
                {
                    statsText += $"发电: {def.powerProduction}\n";
                }
                if (def.storageCapacity > 0)
                {
                    statsText += $"存储: {def.storageCapacity}\n";
                }
                tooltipStats.text = statsText;
            }

            if (tooltipRect != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    tooltipRect.parent as RectTransform,
                    screenPosition,
                    null,
                    out localPoint
                );
                tooltipRect.localPosition = localPoint + new Vector2(10, -10);
            }
        }

        public void HideBuildingTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public void RefreshAllPanels()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel != null)
                {
                    panel.Refresh();
                }
            }
        }
    }
}
