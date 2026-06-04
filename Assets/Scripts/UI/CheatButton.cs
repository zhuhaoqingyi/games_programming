using UnityEngine;
using UnityEngine.UI;
using GameCore;

namespace UI
{
    public class CheatButton : MonoBehaviour
    {
        [Header("按钮引用")]
        public Button addResourcesButton;

        private void Start()
        {
            if (addResourcesButton != null)
            {
                addResourcesButton.onClick.AddListener(OnAddResourcesClicked);
            }
        }

        private void OnAddResourcesClicked()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager 不存在!");
                return;
            }

            GameManager.Instance.AddResource(ResourceType.SpaceOre, 9999);
            GameManager.Instance.AddResource(ResourceType.MetalMaterial, 9999);
            GameManager.Instance.AddResource(ResourceType.BasicPart, 9999);
            GameManager.Instance.AddResource(ResourceType.AdvancedPart, 9999);

            Debug.Log("[作弊] 已添加所有资源 x9999");
        }
    }
}
