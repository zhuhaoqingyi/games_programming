using UnityEngine;
using UnityEngine.UI;
using GameCore;
using System.Collections.Generic;

namespace UI
{
    public class ResourceDisplayUI : MonoBehaviour
    {
        public static ResourceDisplayUI Instance { get; private set; }

        [Header("UI Components")]
        public RectTransform container;
        public Text resourceText;
        public GameObject resourceEntryPrefab;
        public VerticalLayoutGroup layoutGroup;

        [Header("Settings")]
        public string resourcePrefix = "";
        public string resourceSuffix = "";
        public Color textColor = Color.white;
        public int fontSize = 14;
        public int spacing = 5;

        private Dictionary<ResourceType, Text> resourceTexts = new Dictionary<ResourceType, Text>();
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
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateResourceDisplay();
            }
        }

        private void InitializeResourceDisplay()
        {
            if (resourceText == null) return;

            var resourceDefs = DataConfig.GetAllResources();
            foreach (var kvp in resourceDefs)
            {
                var resourceDef = kvp.Value;
                if (resourceDef == null) continue;

                Text entryText = CreateResourceEntry(resourceDef.name, resourceDef.type);
                if (entryText != null)
                {
                    resourceTexts[resourceDef.type] = entryText;
                }
            }

            if (resourceTexts.Count > 0)
            {
                UpdateResourceDisplay();
            }
        }

        private Text CreateResourceEntry(string name, ResourceType type)
        {
            GameObject entryObj = null;

            if (resourceEntryPrefab != null)
            {
                entryObj = Instantiate(resourceEntryPrefab, container);
            }
            else
            {
                entryObj = new GameObject($"ResourceEntry_{name}");
                entryObj.transform.SetParent(container);
            }

            entryObj.name = $"ResourceEntry_{name}";

            Text entryText = entryObj.GetComponent<Text>();
            if (entryText == null)
            {
                entryText = entryObj.AddComponent<Text>();
            }

            entryText.font = resourceText.font;
            entryText.fontSize = fontSize;
            entryText.color = textColor;
            entryText.alignment = TextAnchor.MiddleLeft;
            entryText.horizontalOverflow = HorizontalWrapMode.Overflow;
            entryText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rectTransform = entryObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(200, 25);
            }

            return entryText;
        }

        private void UpdateResourceDisplay()
        {
            if (GameManager.Instance == null) return;

            foreach (var kvp in resourceTexts)
            {
                ResourceType type = kvp.Key;
                Text text = kvp.Value;

                int amount = GameManager.Instance.GetResourceAmount(type);
                var resourceDef = DataConfig.GetResource(type);
                string displayName = resourceDef?.name ?? type.ToString();

                text.text = $"{resourcePrefix}{displayName}: {amount}{resourceSuffix}";
            }
        }

        public void RefreshDisplay()
        {
            UpdateResourceDisplay();
        }

        public void SetTextColor(Color color)
        {
            textColor = color;
            foreach (var text in resourceTexts.Values)
            {
                if (text != null)
                {
                    text.color = color;
                }
            }
        }
    }
}
