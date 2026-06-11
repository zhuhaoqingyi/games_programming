using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI引用")]
        public TextMeshProUGUI contentText;
        public Button confirmButton;

        private void Awake()
        {
            InitializeButtons();
            ShowContent();
        }

        private void InitializeButtons()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void ShowContent()
        {
            if (contentText != null)
            {
                contentText.text =
                    "In the distant future, you were once the commander of a powerful interstellar spaceship.\n" +
                    "However, during a sudden meteor storm, your ship was completely destroyed,\n" +
                    "leaving only an emergency shelter and a few surviving crew members drifting in space.\n" +
                    "Now, you must start from scratch, gather resources, build facilities,\n" +
                    "and rebuild a self-sufficient powerful spaceship!\n\n" +
                    "[BUILD MODE] Press B to enter/exit\n" +
                    "• Click buildings on the left panel to select\n" +
                    "• Move mouse over grid to preview placement\n" +
                    "• Left-click to place, press R to rotate\n" +
                    "• Green = valid placement, Red = invalid\n\n" +
                    "[DELETE MODE] Click 'Delete Mode' button in building UI\n" +
                    "• Click placed buildings to demolish them\n" +
                    "• Demolishing refunds 50% of building resources\n" +
                    "• Core buildings (Emergency Shelter) cannot be deleted\n\n" +
                    "[FLIGHT MODE] Press Tab to toggle\n" +
                    "• W to accelerate forward, S to decelerate/reverse\n" +
                    "• A/D to steer left/right\n" +
                    "• Ship gradually slows down due to drag when keys released\n\n" +
                    "[RESOURCE TYPES]\n" +
                    "• Space Ore - Basic resource, automatically mined by Mining Platforms\n" +
                    "• Metal Material - Smelted from Space Ore\n" +
                    "• Basic Parts - Assembled from Metal Materials\n" +
                    "• Advanced Parts - Processed from Basic Parts and Metal Materials\n\n" +
                    "[POWER SYSTEM]\n" +
                    "• Solar Array - Provides basic power\n" +
                    "• Nuclear Reactor - Generates massive power without consumption\n" +
                    "• Buildings stop working and show warning icon when power is low\n\n" +
                    "[ORE COLLISION]\n" +
                    "• Space ores drift and collide with buildings, causing damage\n" +
                    "• Buildings are destroyed when health reaches zero\n" +
                    "• Mining Platforms automatically collect nearby ores\n\n" +
                    "Plan your base layout carefully, balance resource production and consumption,\n" +
                    "survive in space and rebuild your fleet.\n\n" +
                    "Good luck, Commander!";
            }
        }

        private void OnConfirmClicked()
        {
            if (MainMenuManager.Instance != null)
                MainMenuManager.Instance.ShowMainMenu();
        }
    }
}
