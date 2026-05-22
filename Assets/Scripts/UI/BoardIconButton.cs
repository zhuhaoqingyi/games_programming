using UnityEngine;
using UnityEngine.UI;
using GameCore;
using System.Collections.Generic;

namespace UI
{
    public class BoardIconButton : MonoBehaviour
    {
        [Header("UI Components")]
        public Image boardImage;
        public Text boardNameText;
        public Image selectedIndicator;

        [Header("Colors")]
        public Color normalColor = new Color(1f, 1f, 1f);
        public Color selectedColor = new Color(0f, 1f, 1f);
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f);

        private BoardDefinition boardDef;
        private bool canAfford;
        private bool isSelected;

        public BoardDefinition BoardDef => boardDef;

        public delegate void BoardSelectedDelegate(BoardIconButton button);
        public event BoardSelectedDelegate OnSelected;

        public void Initialize(BoardDefinition def, bool afford)
        {
            boardDef = def;
            canAfford = afford;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (boardNameText != null)
            {
                boardNameText.text = boardDef.name;
            }

            if (boardImage != null)
            {
                boardImage.color = canAfford ? normalColor : lockedColor;
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.gameObject.SetActive(false);
            }
        }

        public void UpdateAffordability()
        {
            canAfford = boardDef.CanAfford(GameManager.Instance?.GetAllResources() ?? new Dictionary<ResourceType, int>());
            UpdateDisplay();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedIndicator != null)
            {
                selectedIndicator.gameObject.SetActive(selected);
            }
        }

        private void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (canAfford)
            {
                OnSelected?.Invoke(this);
            }
        }
    }
}