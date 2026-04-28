using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("按钮引用")]
        public Button newGameButton;
        public Button loadButton;
        public Button optionButton;
        public Button exitButton;

        [Header("面板引用")]
        public GameObject mainMenuPanel;
        public GameObject settingsPanel;
        public GameObject loadPanel;

        [Header("场景名称")]
        public string gameSceneName = "GameScene";

        private void Awake()
        {
            InitializeButtons();
            HideAllPanels();
            ShowMainMenu();
        }

        private void InitializeButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);
            
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);
            
            if (optionButton != null)
                optionButton.onClick.AddListener(OnOptionClicked);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            
            if (loadPanel != null)
                loadPanel.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        public void ShowSettings()
        {
            HideAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void ShowLoadMenu()
        {
            HideAllPanels();
            if (loadPanel != null)
                loadPanel.SetActive(true);
        }

        private void OnNewGameClicked()
        {
            Debug.Log("开始新游戏");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnLoadClicked()
        {
            Debug.Log("打开存档菜单");
            ShowLoadMenu();
        }

        private void OnOptionClicked()
        {
            Debug.Log("打开设置菜单");
            ShowSettings();
        }

        private void OnExitClicked()
        {
            Debug.Log("退出游戏");
            Application.Quit();
        }

        public void OnBackButtonClicked()
        {
            ShowMainMenu();
        }

        public void SaveSettings()
        {
            Debug.Log("保存设置");
            ShowMainMenu();
        }

        public void LoadGame(int saveIndex)
        {
            Debug.Log($"加载存档 {saveIndex}");
            SceneManager.LoadScene(gameSceneName);
        }
    }
}