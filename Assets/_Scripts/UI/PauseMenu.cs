using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GravityGame.UI
{
    /// <summary>
    /// Controls the Pause Menu functionality: toggling pause state,
    /// switching between menu panels, and handling settings like volume.
    /// </summary>
    [RequireComponent(typeof(GameUI))]
    public class PauseMenu : MonoBehaviour
    {
        // UI elements
        private VisualElement _pauseMenu, _mainPanel, _settingsPanel;
        private Button _resumeButton, _settingsButton, _mainMenuButton, _quitButton, _backButton;
        private Slider _volumeSlider;

        private bool _initialized;
        private const string PREF_VOLUME = "MasterVolume";

        /// <summary>
        /// Unity method called when the GameObject is enabled.
        /// Tries to initialize UI references and bindings.
        /// </summary>
        void OnEnable()
        {
            TryInitialize();
        }

        /// <summary>
        /// Monitors pause toggle input and manages cursor behavior when paused.
        /// </summary>
        void Update()
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

        /// <summary>
        /// Initializes all UI elements and sets up event callbacks.
        /// Called once when the GameUI is ready.
        /// </summary>
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

            // Hide all pause menu panels initially
            _pauseMenu.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // Button click handlers
            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _quitButton.clicked += QuitGame;
            _backButton.clicked += ShowMain;

            // Volume slider handling
            _volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                PlayerPrefs.SetFloat(PREF_VOLUME, evt.newValue);
            });

            // Load and apply saved volume
            var saved = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
            AudioListener.volume = saved;
            _volumeSlider.value = saved;

            _initialized = true;
        }

        /// <summary>
        /// Toggles pause/resume state based on current time scale.
        /// </summary>
        public void TogglePause()
        {
            if (!_initialized) return;

            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
        }

        /// <summary>
        /// Pauses the game, shows pause menu, and enables cursor.
        /// </summary>
        private void PauseGame()
        {
            Time.timeScale = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        /// <summary>
        /// Resumes gameplay and hides pause menu, cursor locked again.
        /// </summary>
        private void ResumeGame()
        {
            Time.timeScale = 1f;
            _pauseMenu.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        /// <summary>
        /// Displays the settings panel inside the pause menu.
        /// </summary>
        private void ShowSettings()
        {
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        /// <summary>
        /// Displays the main panel of the pause menu.
        /// </summary>
        private void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.Flex;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        /// <summary>
        /// Loads the Main Menu scene.
        /// </summary>
        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Quits the game or stops play mode in editor.
        /// </summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
