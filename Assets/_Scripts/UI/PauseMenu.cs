using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace GravityGame.UI
{
    [RequireComponent(typeof(GameUI))]
    public class PauseMenu : MonoBehaviour
    {
        private VisualElement _pauseMenu, _mainPanel, _settingsPanel;
        private Button        _resumeButton, _settingsButton, _mainMenuButton, _quitButton, _backButton;
        private Slider        _volumeSlider;

        private bool _initialized;
        private const string PrefVolume = "MasterVolume";

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "MainScene")
                Destroy(gameObject);
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
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }
        }

        private void TryInitialize()
        {
            if (_initialized || GameUI.Instance == null) return;

            var e = GameUI.Instance.Elements;

            _pauseMenu     = e.PauseMenu;
            _mainPanel     = e.MainPanel;
            _settingsPanel = e.SettingsPanel;

            _resumeButton   = e.ResumeButton;
            _settingsButton = e.SettingsButton;
            _mainMenuButton = e.MainMenuButton;
            _quitButton     = e.QuitButton;
            _backButton     = e.BackButton;
            _volumeSlider   = e.VolumeSlider;

            // Start hidden
            _pauseMenu.style.display     = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // Button wiring
            _resumeButton.clicked   += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _quitButton.clicked     += QuitToMainMenu;
            _backButton.clicked     += ShowMain;

            // Volume slider
            _volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                PlayerPrefs.SetFloat(PrefVolume, evt.newValue);
            });

            float saved = PlayerPrefs.GetFloat(PrefVolume, 1f);
            AudioListener.volume = saved;
            _volumeSlider.value  = saved;

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
            Time.timeScale           = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void ResumeGame()
        {
            Time.timeScale           = 1f;
            _pauseMenu.style.display = DisplayStyle.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void ShowSettings()
        {
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.Flex;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void QuitToMainMenu()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
