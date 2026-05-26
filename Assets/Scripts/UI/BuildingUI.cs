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
        
        // 事件：选择建筑
        public delegate void BuildingSelectedDelegate(BuildingType buildingType);
        public static event BuildingSelectedDelegate OnBuildingSelected;
        
        // 事件：退出建筑模式
        public delegate void BuildingModeExitDelegate();
        public static event BuildingModeExitDelegate OnBuildingModeExit;

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
        public CanvasGroup tooltipCanvasGroup;

        [Header("Tooltip Animation")]
        public float tooltipFadeDuration = 0.2f;

        [Header("Building Placer")]
        public BuildingPlacer buildingPlacer;
        public Camera mainCamera;

        [Header("Delete Mode")]
        public GameObject deleteModeButton;
        public Text deleteModeButtonText;
        public Color deleteModeActiveColor = new Color(1f, 0.3f, 0.3f);
        public Color deleteModeInactiveColor = Color.white;

        [Header("Settings")]
        public KeyCode toggleKey = KeyCode.B;
        public KeyCode deleteModeToggleKey = KeyCode.X;
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
            InitializeDeleteModeButton();
            
            if (buildingPlacer != null)
            {
                buildingPlacer.OnDeleteModeChanged += OnDeleteModeChanged;
            }
        }

        private void OnDestroy()
        {
            if (buildingPlacer != null)
            {
                buildingPlacer.OnDeleteModeChanged -= OnDeleteModeChanged;
            }
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

            if (Input.GetKeyDown(deleteModeToggleKey))
            {
                ToggleDeleteMode();
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
            Debug.Log($"[BuildingUI] 点击建筑按钮: {button.BuildingDef?.name ?? "Unknown"}");

            if (button.BuildingDef == null)
            {
                Debug.LogWarning("[BuildingUI] 无法选中 - BuildingDef为空");
                return;
            }

            if (!button.BuildingDef.CanAfford(GameManager.Instance?.GetAllResources() ?? new Dictionary<ResourceType, int>()))
            {
                Debug.LogWarning($"[BuildingUI] 无法选中 {button.BuildingDef.name} - 资源不足");
                return;
            }

            if (isBuildingMode && selectedButton != null && selectedButton.BuildingDef?.type == button.BuildingDef.type)
            {
                Debug.Log($"[BuildingUI] 重复点击同一建筑 {button.BuildingDef.name}，取消当前放置模式");
                CancelBuildingMode();
                return;
            }

            if (isBuildingMode)
            {
                Debug.Log($"[BuildingUI] 已在建筑模式中，先取消当前模式再选中新建筑");
                CancelBuildingMode();
            }

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

            // 触发选择建筑事件
            OnBuildingSelected?.Invoke(def.type);

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
                    buildingPlacer.TryPlaceBuilding();
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

            // 触发退出建筑模式事件
            OnBuildingModeExit?.Invoke();

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
                    new Vector2(0, Screen.height),
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

        private void InitializeDeleteModeButton()
        {
            if (deleteModeButton != null)
            {
                Button btn = deleteModeButton.GetComponent<Button>();
                if (btn == null)
                {
                    btn = deleteModeButton.AddComponent<Button>();
                }
                
                btn.onClick.AddListener(ToggleDeleteMode);
                
                UpdateDeleteModeButtonText();
            }
        }

        private void ToggleDeleteMode()
        {
            if (buildingPlacer != null)
            {
                buildingPlacer.ToggleDeleteMode();
            }
        }

        private void OnDeleteModeChanged(bool isDeleteMode)
        {
            UpdateDeleteModeButtonText();
            Debug.Log($"[BuildingUI] 删除模式: {(isDeleteMode ? "开启" : "关闭")}");
        }

        private void UpdateDeleteModeButtonText()
        {
            if (deleteModeButtonText != null)
            {
                bool isDeleteMode = buildingPlacer != null && buildingPlacer.IsDeleteMode;
                deleteModeButtonText.text = isDeleteMode ? "退出删除" : "删除模式";
                deleteModeButtonText.color = isDeleteMode ? deleteModeActiveColor : deleteModeInactiveColor;
            }

            if (deleteModeButton != null)
            {
                Image btnImage = deleteModeButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    bool isDeleteMode = buildingPlacer != null && buildingPlacer.IsDeleteMode;
                    btnImage.color = isDeleteMode ? new Color(1f, 0.3f, 0.3f, 1f) : Color.white;
                }
            }
        }
    }
}
