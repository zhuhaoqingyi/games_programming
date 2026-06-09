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

        // Tooltip 当前所属的按钮（防止延迟协程错误隐藏后续按钮的tooltip）
        private BuildingIconButton currentTooltipOwner;

        public bool IsUIVisible => isUIVisible;

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
                // 按B键时强制隐藏tooltip
                HideBuildingTooltip();
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

            if (deleteModeButton != null)
            {
                deleteModeButton.SetActive(isUIVisible);
            }

            if (!isUIVisible && isBuildingMode)
            {
                CancelBuildingMode();
            }

            // 在飞船驾驶模式下打开建筑系统时，切换到建造模式视角
            if (isUIVisible && ThrustManager.Instance != null && ThrustManager.Instance.CurrentPhase == GamePhase.ShipMode)
            {
                ThrustManager.Instance.EnterBuildMode();
                Debug.Log("[BuildingUI] 从飞船模式切换到建造模式视角");
            }
        }

        public void ShowUI()
        {
            Debug.Log("[BuildingUI] ShowUI called");
            
            isUIVisible = true;
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
                Debug.Log("[BuildingUI] mainPanel activated");
            }

            if (deleteModeButton != null)
            {
                deleteModeButton.SetActive(true);
                Debug.Log("[BuildingUI] deleteModeButton activated");
            }

            // 恢复所有分类面板的展开状态
            foreach (var panel in categoryPanels)
            {
                if (panel != null)
                {
                    panel.SetExpanded(true);
                    Debug.Log("[BuildingUI] Category panel " + panel.Category + " expanded");
                }
            }
            
            Debug.Log("[BuildingUI] ShowUI completed");
        }

        public void HideUI()
        {
            isUIVisible = false;
            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }

            if (deleteModeButton != null)
            {
                deleteModeButton.SetActive(false);
            }

            // 隐藏 UI 时也隐藏 tooltip
            HideBuildingTooltip();
        }

        public void SelectBuilding(BuildingIconButton button)
        {
            Debug.Log($"[BuildingUI] Building button clicked: {button.BuildingDef?.name ?? "Unknown"}");

            if (button.BuildingDef == null)
            {
                Debug.LogWarning("[BuildingUI] Cannot select - BuildingDef is null");
                return;
            }

            if (!button.BuildingDef.CanAfford(GameManager.Instance?.GetAllResources() ?? new Dictionary<ResourceType, int>()))
            {
                Debug.LogWarning($"[BuildingUI] Cannot select {button.BuildingDef.name} - Insufficient resources");
                return;
            }

            if (isBuildingMode && selectedButton != null && selectedButton.BuildingDef?.type == button.BuildingDef.type)
            {
                Debug.Log($"[BuildingUI] Same building clicked again {button.BuildingDef.name}, canceling placement mode");
                CancelBuildingMode();
                return;
            }

            if (isBuildingMode)
            {
                Debug.Log($"[BuildingUI] Already in building mode, canceling current mode before selecting new building");
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

            // 隐藏建筑 UI
            HideUI();

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
            Debug.Log("[BuildingUI] CancelBuildingMode called, restoring UI");
            
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
            
            // 隐藏 tooltip
            HideBuildingTooltip();

            // 恢复建筑 UI 显示
            ShowUI();
            
            Debug.Log("[BuildingUI] CancelBuildingMode completed, isUIVisible=" + isUIVisible);
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

        public void ShowBuildingTooltip(BuildingDefinition def, Vector3 screenPosition, BuildingIconButton owner)
        {
            if (tooltipPanel == null)
            {
                Debug.LogError("[BuildingUI] tooltipPanel is null!");
                return;
            }
            
            if (def == null)
            {
                Debug.LogError("[BuildingUI] BuildingDefinition is null!");
                return;
            }

            // 记录当前 tooltip 的所属按钮
            currentTooltipOwner = owner;

            Debug.Log($"[BuildingUI] Showing tooltip for {def.name}");

            // 确保 tooltipPanel 显示
            tooltipPanel.SetActive(true);
            
            // 确保 CanvasGroup 的透明度正确
            CanvasGroup tooltipCanvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (tooltipCanvasGroup != null)
            {
                tooltipCanvasGroup.alpha = 1f;
                tooltipCanvasGroup.interactable = false;
                tooltipCanvasGroup.blocksRaycasts = false;
            }
            
            // 确保 Image 组件的颜色透明度正确，并且不阻挡鼠标
            Image tooltipImage = tooltipPanel.GetComponent<Image>();
            if (tooltipImage != null)
            {
                Color c = tooltipImage.color;
                c.a = Mathf.Clamp01(c.a);
                tooltipImage.color = c;
                tooltipImage.raycastTarget = false;
            }
            
            // 确保所有子物体的 Image 也不阻挡鼠标
            Image[] childImages = tooltipPanel.GetComponentsInChildren<Image>();
            foreach (Image childImage in childImages)
            {
                childImage.raycastTarget = false;
            }
            
            // 确保所有子物体的 Text 也不阻挡鼠标
            Text[] childTexts = tooltipPanel.GetComponentsInChildren<Text>();
            foreach (Text childText in childTexts)
            {
                childText.raycastTarget = false;
            }

            // 建筑名称
            if (tooltipName != null)
            {
                tooltipName.text = def.name;
                // 确保名称文本正确设置
                tooltipName.horizontalOverflow = HorizontalWrapMode.Wrap;
                tooltipName.verticalOverflow = VerticalWrapMode.Overflow;
                tooltipName.alignment = TextAnchor.UpperLeft;
            }

            // 建筑描述
            if (tooltipDescription != null)
            {
                tooltipDescription.text = def.description;
                tooltipDescription.horizontalOverflow = HorizontalWrapMode.Wrap;
                tooltipDescription.verticalOverflow = VerticalWrapMode.Overflow;
                tooltipDescription.alignment = TextAnchor.UpperLeft;
            }

            // 建造成本
            if (tooltipCost != null)
            {
                string costText = "<color=#FFD700>Construction Cost:</color>\n";
                if (def.costs.Count > 0)
                {
                    foreach (var cost in def.costs)
                    {
                        var resourceDef = DataConfig.GetResource(cost.resourceType);
                        string resourceName = resourceDef?.name ?? cost.resourceType.ToString();
                        costText += $"  • {resourceName}: {cost.amount}\n";
                    }
                }
                else
                {
                    costText += "  None";
                }
                tooltipCost.text = costText;
                tooltipCost.horizontalOverflow = HorizontalWrapMode.Wrap;
                tooltipCost.verticalOverflow = VerticalWrapMode.Overflow;
                tooltipCost.alignment = TextAnchor.UpperLeft;
            }

            // 建筑属性
            if (tooltipStats != null)
            {
                System.Text.StringBuilder statsText = new System.Text.StringBuilder();
                statsText.AppendLine("<color=#FFD700>Building Stats:</color>");
                statsText.AppendLine($"  • Size: {def.width} x {def.height}");
                
                if (def.functionalAreaWidth > 0 || def.functionalAreaHeight > 0)
                {
                    statsText.AppendLine($"  • Functional Area: {def.functionalAreaWidth} x {def.functionalAreaHeight}");
                }

                if (def.powerConsumption > 0)
                {
                    statsText.AppendLine($"  • <color=#FF6B6B>Power Consumption:</color> {def.powerConsumption} MW");
                }
                
                if (def.powerProduction > 0)
                {
                    statsText.AppendLine($"  • <color=#6BFF6B>Power Production:</color> {def.powerProduction} MW");
                }
                
                if (def.storageCapacity > 0)
                {
                    statsText.AppendLine($"  • <color=#6BB3FF>Storage Capacity:</color> {def.storageCapacity}");
                }

                if (def.isProductionBuilding)
                {
                    statsText.AppendLine($"  • <color=#FFB36B>Production Building</color>");
                }

                if (def.isBoard)
                {
                    statsText.AppendLine($"  • <color=#B36BFF>Base Board</color>");
                }

                if (def.isCoreBuilding)
                {
                    statsText.AppendLine($"  • <color=#FF6BFF>Core Building</color>");
                }

                if (def.canRotate)
                {
                    statsText.AppendLine($"  • <color=#6BFFFF>Rotatable</color>");
                }

                tooltipStats.text = statsText.ToString();
                tooltipStats.horizontalOverflow = HorizontalWrapMode.Wrap;
                tooltipStats.verticalOverflow = VerticalWrapMode.Overflow;
                tooltipStats.alignment = TextAnchor.UpperLeft;
            }

            // 更新 tooltip 位置
            if (tooltipRect != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    tooltipRect.parent as RectTransform,
                    screenPosition,
                    null,
                    out localPoint
                );
                tooltipRect.localPosition = localPoint + new Vector2(15, -15);
            }
        }

        public void HideBuildingTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
            currentTooltipOwner = null;
        }

        /// <summary>
        /// 仅当 owner 与当前 tooltip 的所属按钮一致时才隐藏
        /// 用于 BuildingIconButton 的延迟隐藏协程，防止误隐藏其他按钮的 tooltip
        /// </summary>
        public void HideBuildingTooltipIfOwner(BuildingIconButton owner)
        {
            if (currentTooltipOwner == owner)
            {
                HideBuildingTooltip();
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
            Debug.Log($"[BuildingUI] Delete mode: {(isDeleteMode ? "ON" : "OFF")}");
        }

        private void UpdateDeleteModeButtonText()
        {
            if (deleteModeButtonText != null)
            {
                bool isDeleteMode = buildingPlacer != null && buildingPlacer.IsDeleteMode;
                deleteModeButtonText.text = isDeleteMode ? "Exit Delete" : "Delete Mode";
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
