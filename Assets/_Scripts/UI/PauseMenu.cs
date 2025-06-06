using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace GravityGame.UI
{
    /// <summary>
    /// Manages the pause menu UI, including show/hide logic, volume control, and navigation back to main menu.
    /// </summary>
    [RequireComponent(typeof(GameUI))]
    public class PauseMenu : MonoBehaviour
    {
        [FormerlySerializedAs("ocrFont")]
        [Header("Assign your OCRA.ttf Unity Font here")]
        public Font OcrFont;

        private VisualElement _pauseMenu;
        private VisualElement _mainPanel;
        private VisualElement _settingsPanel;

        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _backButton;

        private Slider _volumeSlider;
        private Label  _soundLevelsLabel;

        private bool _initialized;
        private const string PrefVolume = "MasterVolume";

        /// <summary>
        /// Ensures this component only exists in the MainScene.
        /// Destroys itself if loaded in any other scene.
        /// </summary>
        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "MainScene")
                Destroy(gameObject);
        }

        /// <summary>
        /// Attempts to initialize UI references when this component becomes enabled.
        /// </summary>
        private void OnEnable()
        {
            TryInitialize();
        }

        private void Update()
        {
            // If initialization hasn't succeeded yet, keep trying until GameUI.Instance is ready.
            if (!_initialized)
                TryInitialize();

            // Toggle pause when 'P' is pressed.
            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();

            // When paused, ensure the cursor is visible and unlocked.
            if (Time.timeScale == 0f)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }
        }

        /// <summary>
        /// Sets up all UI element references, initial visibility, callbacks, and volume settings.
        /// Skips setup if already initialized or if the GameUI singleton isn't available yet.
        /// </summary>
        private void TryInitialize()
        {
            if (_initialized || GameUI.instance == null)
                return;

            var elements = GameUI.instance.elements;

            _pauseMenu      = elements.PauseMenu;
            _mainPanel      = elements.MainPanel;
            _settingsPanel  = elements.SettingsPanel;

            _resumeButton   = elements.ResumeButton;
            _settingsButton = elements.SettingsButton;
            _mainMenuButton = elements.MainMenuButton;
            _backButton     = elements.BackButton;
            _volumeSlider   = elements.VolumeSlider;

            // Retrieve the "SoundLevelsLabel" from the root VisualElement
            _soundLevelsLabel = GameUI.instance.uiDocument.rootVisualElement
                .Q<Label>("SoundLevelsLabel");

            // Verify all required UI elements exist
            if (_pauseMenu       == null ||
                _mainPanel       == null ||
                _settingsPanel   == null ||
                _resumeButton    == null ||
                _settingsButton  == null ||
                _mainMenuButton  == null ||
                _backButton      == null ||
                _volumeSlider    == null ||
                _soundLevelsLabel == null)
            {
                Debug.LogError("PauseMenu: One or more UI elements could not be found in UXML!");
                return;
            }

            // Hide all panels initially
            _pauseMenu.style.display     = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // Register button click callbacks
            _resumeButton.clicked   += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _backButton.clicked     += ShowMain;

            // Initialize volume slider from saved preference
            float savedVolume       = PlayerPrefs.GetFloat(PrefVolume, 1f);
            AudioListener.volume    = savedVolume;
            _volumeSlider.value     = savedVolume;
            _volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                PlayerPrefs.SetFloat(PrefVolume, evt.newValue);
            });

            // Apply OCRA font and styling if provided
            if (OcrFont != null)
            {
                var sFont = new StyleFont(OcrFont);

                // Buttons: OCRA font at size 20
                _resumeButton.style.unityFont   = sFont;
                _resumeButton.style.fontSize    = 20;
                _settingsButton.style.unityFont = sFont;
                _settingsButton.style.fontSize  = 20;
                _mainMenuButton.style.unityFont = sFont;
                _mainMenuButton.style.fontSize  = 20;
                _backButton.style.unityFont     = sFont;
                _backButton.style.fontSize      = 20;

                // "Sound Levels" label: OCRA font at size 28, white color
                _soundLevelsLabel.style.unityFont = sFont;
                _soundLevelsLabel.style.fontSize  = 28;
                _soundLevelsLabel.style.color     = new StyleColor(Color.white);
            }
            else
            {
                Debug.LogWarning("PauseMenu: OcrFont is null. All text will use the default font.");
            }

            _initialized = true;
        }

        /// <summary>
        /// Toggles between paused and unpaused states if initialization is complete.
        /// </summary>
        private void TogglePause()
        {
            if (!_initialized)
                return;

            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
        }

        /// <summary>
        /// Pauses the game by stopping time, showing the pause menu, and unlocking the cursor.
        /// </summary>
        private void PauseGame()
        {
            Time.timeScale           = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        /// <summary>
        /// Resumes the game by restoring time scale and hiding the pause menu, then locking the cursor.
        /// </summary>
        private void ResumeGame()
        {
            Time.timeScale           = 1f;
            _pauseMenu.style.display = DisplayStyle.None;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        /// <summary>
        /// Shows the settings panel within the pause menu and keeps the cursor unlocked.
        /// </summary>
        private void ShowSettings()
        {
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        /// <summary>
        /// Shows the main pause panel (resume, main menu, etc.) and keeps the cursor unlocked.
        /// </summary>
        private void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.Flex;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        /// <summary>
        /// Unpauses the game and loads the MainMenu scene.
        /// </summary>
        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
