using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace GravityGame.UI
{
    /// <summary>
    ///     Manages the pause menu UI, including show/hide logic, volume control, Save button,
    ///     and navigation back to main menu.
    /// </summary>
    [RequireComponent(typeof(GameUI))]
    public class PauseMenu : MonoBehaviour
    {
        [Header("Assign your OCRA.ttf Unity Font here")]
        public Font OcrFont;

        // panels & buttons
        VisualElement _pauseMenu;
        VisualElement _mainPanel;
        VisualElement _settingsPanel;
        Button _resumeButton;
        Button _settingsButton;
        Button _mainMenuButton;
        Button _backButton;
        Button _saveButton;

        // sliders & their numeric-readout labels
        Slider _sfxSlider;
        Slider _musicSlider;
        Label _sfxValueLabel;
        Label _musicValueLabel;

        bool _initialized;
        const string PrefSfxVolume = "SfxVolume";
        const string PrefMusicVolume = "MusicVolume";

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

            if (Time.timeScale == 0f) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void TryInitialize()
        {
            if (_initialized || GameUI.Instance == null)
                return;

            var root = GameUI.Instance.UIDocument.rootVisualElement;
            var e = GameUI.Instance.Elements;

            // grab panels & buttons
            _pauseMenu = e.PauseMenu;
            _mainPanel = e.MainPanel;
            _settingsPanel = e.SettingsPanel;
            _resumeButton = e.ResumeButton;
            _settingsButton = e.SettingsButton;
            _mainMenuButton = e.MainMenuButton;
            _backButton = e.BackButton;
            _saveButton = e.SaveButton;

            // grab sliders
            _sfxSlider = e.SfxVolumeSlider;
            _musicSlider = e.MusicVolumeSlider;

            // grab both volume labels by their UXML name ("volumelabel")
            var volumeLabels = root
                .Query<Label>(name: "volumelabel")
                .ToList();
            if (volumeLabels.Count >= 2) {
                _sfxValueLabel = volumeLabels[0];
                _musicValueLabel = volumeLabels[1];
            } else {
                Debug.LogError("PauseMenu: Couldn't find both volume-labels in UXML!");
                return;
            }

            // bail-out if anything is null
            if (_pauseMenu == null || _mainPanel == null || _settingsPanel == null ||
                _resumeButton == null || _settingsButton == null ||
                _mainMenuButton == null || _backButton == null || _saveButton == null ||
                _sfxSlider == null || _musicSlider == null ||
                _sfxValueLabel == null || _musicValueLabel == null) {
                Debug.LogError("PauseMenu: One or more UI elements could not be found!");
                return;
            }

            // hide panels at start
            _pauseMenu.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // hook up buttons
            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _backButton.clicked += ShowMain;
            _saveButton.clicked += SaveSettings;

            // load saved prefs (0–1), convert to 0–100, set sliders & labels
            float savedSfxNorm = PlayerPrefs.GetFloat(PrefSfxVolume, 1f);
            float savedMusicNorm = PlayerPrefs.GetFloat(PrefMusicVolume, 1f);
            float savedSfxPct = savedSfxNorm * 100f;
            float savedMusicPct = savedMusicNorm * 100f;

            _sfxSlider.value = savedSfxPct;
            _musicSlider.value = savedMusicPct;
            _sfxValueLabel.text = $"{Mathf.RoundToInt(savedSfxPct)}";
            _musicValueLabel.text = $"{Mathf.RoundToInt(savedMusicPct)}";

            // slider callbacks keep labels & prefs in sync
            _sfxSlider.RegisterValueChangedCallback(evt => {
                    float pct = evt.newValue;
                    float norm = pct / 100f;
                    PlayerPrefs.SetFloat(PrefSfxVolume, norm);
                    _sfxValueLabel.text = $"{Mathf.RoundToInt(pct)}";
                }
            );

            _musicSlider.RegisterValueChangedCallback(evt => {
                    float pct = evt.newValue;
                    float norm = pct / 100f;
                    PlayerPrefs.SetFloat(PrefMusicVolume, norm);
                    _musicValueLabel.text = $"{Mathf.RoundToInt(pct)}";
                }
            );

            // font styling
            if (OcrFont != null) {
                var sFont = new StyleFont(OcrFont);
                foreach (var btn in new[] { _resumeButton, _settingsButton, _mainMenuButton, _backButton, _saveButton }) {
                    btn.style.unityFont = sFont;
                    btn.style.fontSize = 20;
                }
            } else {
                Debug.LogWarning("PauseMenu: OcrFont is null. Default font will be used.");
            }

            _initialized = true;
        }

        void TogglePause()
        {
            if (!_initialized) return;
            if (Time.timeScale == 0f) ResumeGame();
            else PauseGame();
        }

        void PauseGame()
        {
            Time.timeScale = 0f;
            _pauseMenu.style.display = DisplayStyle.Flex;
            ShowMain();
        }

        void ResumeGame()
        {
            Time.timeScale = 1f;
            _pauseMenu.style.display = DisplayStyle.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void ShowSettings()
        {
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;
        }

        void ShowMain()
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.Flex;
        }

        void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        void SaveSettings()
        {
            // Persist both volumes
            PlayerPrefs.SetFloat(PrefSfxVolume, _sfxSlider.value / 100f);
            PlayerPrefs.SetFloat(PrefMusicVolume, _musicSlider.value / 100f);
            PlayerPrefs.Save();

            // Optionally apply immediately:
            AudioListener.volume = PlayerPrefs.GetFloat(PrefSfxVolume, 1f);
            // TODO: route music volume to your mixer group as well

            // Return to the main pause panel
            ShowMain();
        }
    }
}