using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GravityGame.UI
{
    [RequireComponent(typeof(GameUI))]
    public class PauseMenu : MonoBehaviour
    {
        private VisualElement _pauseMenu, _mainPanel, _settingsPanel;
        private Button _resumeButton, _settingsButton, _mainMenuButton, _quitButton, _backButton;
        private Slider _volumeSlider;

        private bool _initialized;
        private const string PrefVolume = "MasterVolume";

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "MainScene")
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!_initialized) TryInitialize();

            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();

            if (Time.timeScale == 0f)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }

        private void TryInitialize()
        {
            if (_initialized || GameUI.Instance == null) return;

            var e = GameUI.Instance.Elements;
            _pauseMenu = e.PauseMenu;
            _mainPanel = e.MainPanel;
            _settingsPanel = e.SettingsPanel;

            _resumeButton = e.ResumeButton;
            _settingsButton = e.SettingsButton;
            _mainMenuButton = e.MainMenuButton;
            _quitButton = e.QuitButton;
            _backButton = e.BackButton;
            _volumeSlider = e.VolumeSlider;

            _pauseMenu.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _quitButton.clicked += QuitToMainMenu;
            _backButton.clicked += ShowMain;

            _volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                PlayerPrefs.SetFloat(PrefVolume, evt.newValue);
            });

            var saved = PlayerPrefs.GetFloat(PrefVolume, 1f);
            AudioListener.volume = saved;
            _volumeSlider.value = saved;

            _initialized = true;
        }

        private void TogglePause()
        {
            if (!_initialized) return;

            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            _pauseMenu.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        private void ShowSettings()
        {
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.Flex;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
