using UnityEngine;
using UnityEngine.SceneManagement;
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
        [Header("Assign your OCRA.ttf Unity Font here")]
        public Font OcrFont;

        private VisualElement _pauseMenu;
        private VisualElement _mainPanel;
        private VisualElement _settingsPanel;

        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _backButton;

        private Slider _sfxSlider;
        private Slider _musicSlider;

        private bool _initialized;
        private const string PrefSfxVolume   = "SfxVolume";
        private const string PrefMusicVolume = "MusicVolume";

        void Awake()
        {
            if (SceneManager.GetActiveScene().name != "MainScene")
                Destroy(gameObject);
        }

        void OnEnable()
        {
            TryInitialize();
        }

        void Update()
        {
            if (!_initialized)
                TryInitialize();

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
            if (_initialized || GameUI.Instance == null)
                return;

            var e = GameUI.Instance.Elements;
            _pauseMenu     = e.PauseMenu;
            _mainPanel     = e.MainPanel;
            _settingsPanel = e.SettingsPanel;

            _resumeButton  = e.ResumeButton;
            _settingsButton= e.SettingsButton;
            _mainMenuButton= e.MainMenuButton;
            _backButton    = e.BackButton;

            _sfxSlider     = e.SfxVolumeSlider;
            _musicSlider   = e.MusicVolumeSlider;

            // Bail out if any element is missing
            if (_pauseMenu == null || _mainPanel == null || _settingsPanel == null ||
                _resumeButton == null || _settingsButton == null ||
                _mainMenuButton == null || _backButton == null ||
                _sfxSlider == null || _musicSlider == null)
            {
                Debug.LogError("PauseMenu: One or more UI elements could not be found!");
                return;
            }

            // Hide everything initially
            _pauseMenu.style.display     = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // Hook up buttons
            _resumeButton.clicked   += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _backButton.clicked     += ShowMain;

            // Load saved prefs & set slider values
            float savedSfx   = PlayerPrefs.GetFloat(PrefSfxVolume,   1f);
            float savedMusic = PlayerPrefs.GetFloat(PrefMusicVolume, 1f);
            _sfxSlider.value   = savedSfx;
            _musicSlider.value = savedMusic;

            _sfxSlider.RegisterValueChangedCallback(evt =>
            {
                PlayerPrefs.SetFloat(PrefSfxVolume, evt.newValue);
                // TODO: Apply to your SFX mixer group here
            });
            _musicSlider.RegisterValueChangedCallback(evt =>
            {
                PlayerPrefs.SetFloat(PrefMusicVolume, evt.newValue);
                // TODO: Apply to your Music mixer group here
            });

            // Apply OCRA font to buttons
            if (OcrFont != null)
            {
                var sFont = new StyleFont(OcrFont);
                foreach (var btn in new[]{ _resumeButton, _settingsButton, _mainMenuButton, _backButton })
                {
                    btn.style.unityFont = sFont;
                    btn.style.fontSize  = 20;
                }
            }
            else
            {
                Debug.LogWarning("PauseMenu: OcrFont is null. Default font will be used.");
            }

            _initialized = true;
        }

        private void TogglePause()
        {
            if (!_initialized) return;
            if (Time.timeScale == 0f) ResumeGame();
            else                        PauseGame();
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            _pauseMenu.style.display = DisplayStyle.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void ShowSettings()
        {
            _mainPanel.style.display     = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;
        }

        private void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display     = DisplayStyle.Flex;
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
