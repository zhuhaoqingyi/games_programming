using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [Header("按钮引用")]
        public Button newGameButton;
        public Button loadButton;
        public Button optionButton;
        public Button tutorialButton;
        public Button exitButton;

        [Header("面板引用")]
        public GameObject mainMenuPanel;
        public GameObject settingsPanel;
        public GameObject loadPanel;
        public GameObject tutorialPanel;

        [Header("场景名称")]
        public string gameSceneName = "GameScene";

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
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
            
            if (tutorialButton != null)
                tutorialButton.onClick.AddListener(OnTutorialClicked);
            
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
            
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
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

        public void ShowTutorial()
        {
            HideAllPanels();
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }

        private void OnNewGameClicked()
        {
            Debug.Log("开始新游戏");
            GameManager.PendingLoad = false;
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnLoadClicked()
        {
            Debug.Log("加载存档");

            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
            if (System.IO.File.Exists(savePath))
            {
                GameManager.PendingLoad = true;
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogWarning("没有找到存档文件");
            }
        }

        private void OnOptionClicked()
        {
            Debug.Log("打开设置菜单");
            ShowSettings();
        }

        private void OnTutorialClicked()
        {
            Debug.Log("打开教程");
            ShowTutorial();
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
            
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
            {
                // 先加载场景
                SceneManager.LoadScene(gameSceneName);
                // 场景加载后会自动加载存档（在 GameManager.Start 中处理）
            }
            else
            {
                Debug.LogWarning("没有找到存档文件");
            }
        }
    }
}