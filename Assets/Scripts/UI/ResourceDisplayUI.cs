using UnityEngine;
using UnityEngine.UI;
using GameCore;
using System.Collections.Generic;
using PowerSystem;
using LogisticsSystem;

namespace UI
{
    public class ResourceDisplayUI : MonoBehaviour
    {
        public static ResourceDisplayUI Instance { get; private set; }

        [Header("UI Components")]
        public RectTransform container;
        public GameObject resourceEntryPrefab;

        [Header("Icons")]
        public Sprite spaceOreIcon;
        public Sprite metalMaterialIcon;
        public Sprite basicPartIcon;
        public Sprite advancedPartIcon;

        [Header("Power Icon")]
        public Sprite powerIcon;

        [Header("Font")]
        public Font resourceFont;

        [Header("Settings")]
        public Color normalTextColor = Color.white;
        public Color fullTextColor = new Color(1f, 0.267f, 0.267f);
        public Color powerNormalColor = Color.white;
        public Color powerInsufficientColor = new Color(1f, 0.267f, 0.267f);
        public int fontSize = 16;
        public float entrySpacing = 10f;
        public float entryWidth = 200f;
        public float entryHeight = 50f;
        public float iconSize = 50f;

        private Dictionary<ResourceType, ResourceEntryUI> resourceEntries = new Dictionary<ResourceType, ResourceEntryUI>();
        private ResourceEntryUI powerEntry;
        private float updateTimer = 0f;
        private float updateInterval = 0.5f;

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
            InitializeResourceDisplay();
            InitializePowerDisplay();
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateResourceDisplay();
                UpdatePowerDisplay();
            }
        }

        private void InitializeResourceDisplay()
        {
            if (container == null)
            {
                Debug.LogError("[ResourceDisplayUI] Container 未设置！");
                return;
            }

            var resourceDefs = DataConfig.GetAllResources();
            foreach (var kvp in resourceDefs)
            {
                var resourceDef = kvp.Value;
                if (resourceDef == null) continue;

                Sprite iconSprite = GetIconForResource(resourceDef.type);
                ResourceEntryUI entry = CreateResourceEntry(resourceDef.type, iconSprite);
                if (entry != null)
                {
                    resourceEntries[resourceDef.type] = entry;
                }
            }

            if (resourceEntries.Count > 0)
            {
                UpdateResourceDisplay();
            }
        }

        private void InitializePowerDisplay()
        {
            if (container == null) return;

            GameObject entryObj = null;
            if (resourceEntryPrefab != null)
            {
                entryObj = Instantiate(resourceEntryPrefab, container);
            }
            else
            {
                entryObj = CreateDefaultEntryObject("Power");
            }

            if (entryObj == null) return;

            entryObj.name = "ResourceEntry_Power";

            ResourceEntryUI entry = entryObj.GetComponent<ResourceEntryUI>();
            if (entry == null)
            {
                entry = entryObj.AddComponent<ResourceEntryUI>();
            }

            Image iconImage = entryObj.transform.Find("Icon")?.GetComponent<Image>();
            Text amountText = entryObj.transform.Find("Text")?.GetComponent<Text>();

            if (iconImage != null)
            {
                entry.resourceIcon = iconImage;
            }

            if (amountText != null)
            {
                entry.resourceText = amountText;
            }

            entry.InitializePower(powerIcon, resourceFont, fontSize, powerNormalColor);

            if (iconImage != null)
            {
                RectTransform iconRect = iconImage.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.sizeDelta = new Vector2(entryWidth, entryHeight);
            }

            powerEntry = entry;
            UpdatePowerDisplay();
        }

        private Sprite GetIconForResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.SpaceOre: return spaceOreIcon;
                case ResourceType.MetalMaterial: return metalMaterialIcon;
                case ResourceType.BasicPart: return basicPartIcon;
                case ResourceType.AdvancedPart: return advancedPartIcon;
                default: return null;
            }
        }

        private ResourceEntryUI CreateResourceEntry(ResourceType type, Sprite iconSprite)
        {
            GameObject entryObj = null;

            if (resourceEntryPrefab != null)
            {
                entryObj = Instantiate(resourceEntryPrefab, container);
            }
            else
            {
                entryObj = CreateDefaultEntryObject(type.ToString());
            }

            if (entryObj == null) return null;

            entryObj.name = $"ResourceEntry_{type.ToString()}";

            ResourceEntryUI entry = entryObj.GetComponent<ResourceEntryUI>();
            if (entry == null)
            {
                entry = entryObj.AddComponent<ResourceEntryUI>();
            }

            Image iconImage = entryObj.transform.Find("Icon")?.GetComponent<Image>();
            Text amountText = entryObj.transform.Find("Text")?.GetComponent<Text>();

            if (iconImage != null) entry.resourceIcon = iconImage;
            if (amountText != null) entry.resourceText = amountText;

            entry.Initialize(type, iconSprite, resourceFont, fontSize, normalTextColor);

            if (iconImage != null)
            {
                RectTransform iconRect = iconImage.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.sizeDelta = new Vector2(entryWidth, entryHeight);
            }

            return entry;
        }

        private GameObject CreateDefaultEntryObject(string nameSuffix)
        {
            GameObject entryObj = new GameObject($"ResourceEntry_{nameSuffix}");
            entryObj.transform.SetParent(container);
            entryObj.transform.localScale = Vector3.one;

            RectTransform entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0, 0.5f);
            entryRect.anchorMax = new Vector2(0, 0.5f);
            entryRect.pivot = new Vector2(0, 0.5f);

            Image backgroundImage = entryObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(entryObj.transform);
            iconObj.transform.localScale = Vector3.one;

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(5, 0);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(entryObj.transform);
            textObj.transform.localScale = Vector3.one;

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.3f, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);

            Text amountText = textObj.AddComponent<Text>();
            amountText.color = normalTextColor;
            amountText.fontSize = fontSize;
            amountText.alignment = TextAnchor.MiddleLeft;
            amountText.horizontalOverflow = HorizontalWrapMode.Overflow;
            amountText.verticalOverflow = VerticalWrapMode.Overflow;

            if (resourceFont != null)
            {
                amountText.font = resourceFont;
            }

            return entryObj;
        }

        private int GetTotalStorageCapacity()
        {
            if (GameManager.Instance == null) return 0;
            return GameManager.Instance.GetTotalStorageCapacity();
        }

        private void UpdateResourceDisplay()
        {
            if (GameManager.Instance == null) return;

            foreach (var kvp in resourceEntries)
            {
                ResourceType type = kvp.Key;
                ResourceEntryUI entry = kvp.Value;

                int amount = GameManager.Instance.GetResourceAmount(type);
                int capacity = GameManager.Instance.GetResourceCapacity(type);
                entry.UpdateAmountWithMax(amount, capacity > 0 ? capacity : 0, normalTextColor, fullTextColor);
            }
        }

        private void UpdatePowerDisplay()
        {
            if (PowerManager.Instance == null) return;
            if (powerEntry == null) return;

            float generated = PowerManager.Instance.TotalGenerated;
            float demand = PowerManager.Instance.TotalDemand;
            bool satisfied = PowerManager.Instance.IsPowerSatisfied;

            Color displayColor = satisfied ? powerNormalColor : powerInsufficientColor;
            powerEntry.UpdatePowerAmount(Mathf.CeilToInt(demand), Mathf.CeilToInt(generated), displayColor);
        }

        public void RefreshDisplay()
        {
            UpdateResourceDisplay();
            UpdatePowerDisplay();
        }
    }
}
