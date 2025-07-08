using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace GravityGame.UI
{
    /// <summary>
    ///     Manages the pause menu UI, including show/hide logic,
    ///     volume control, Save/Cancel behavior, and navigation back to main menu.
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
            // Only keep in MainScene
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

            // Toggle with P
            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();

            // If paused, ensure cursor is visible & unlocked
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

            // grab our root containers and controls
            _pauseMenu = e.PauseMenu;
            _mainPanel = e.MainPanel;
            _settingsPanel = e.SettingsPanel;
            _resumeButton = e.ResumeButton;
            _settingsButton = e.SettingsButton;
            _mainMenuButton = e.MainMenuButton;
            _backButton = e.BackButton;
            _saveButton = e.SaveButton;

            _sfxSlider = e.SfxVolumeSlider;
            _musicSlider = e.MusicVolumeSlider;

            // find both volume value labels (they share the name "volumelabel")
            var labels = root.Query<Label>(name: "volumelabel").ToList();
            if (labels.Count >= 2) {
                _sfxValueLabel = labels[0];
                _musicValueLabel = labels[1];
            } else {
                Debug.LogError("PauseMenu: Couldn't find both volume‐labels in UXML!");
                return;
            }

            // verify everything exists
            if (_pauseMenu == null ||
                _mainPanel == null ||
                _settingsPanel == null ||
                _resumeButton == null ||
                _settingsButton == null ||
                _mainMenuButton == null ||
                _backButton == null ||
                _saveButton == null ||
                _sfxSlider == null ||
                _musicSlider == null ||
                _sfxValueLabel == null ||
                _musicValueLabel == null) {
                Debug.LogError("PauseMenu: One or more UI elements could not be found!");
                return;
            }

            // hide both overlays at start
            _pauseMenu.style.display = DisplayStyle.None;
            _mainPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.None;

            // wire up button callbacks
            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += ShowSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            _backButton.clicked += ShowMain; // acts as Cancel
            _saveButton.clicked += SaveSettings; // acts as Save

            // slider callbacks — update just the labels
            _sfxSlider.RegisterValueChangedCallback(evt => { _sfxValueLabel.text = $"{Mathf.RoundToInt(evt.newValue)}"; });
            _musicSlider.RegisterValueChangedCallback(evt => { _musicValueLabel.text = $"{Mathf.RoundToInt(evt.newValue)}"; });

            // apply OCRA font if available
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

            if (Time.timeScale == 0f)
                ResumeGame();
            else
                PauseGame();
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
            // reset sliders & labels to last‐saved values
            float sfxPct = PlayerPrefs.GetFloat(PrefSfxVolume, 1f) * 100f;
            float musicPct = PlayerPrefs.GetFloat(PrefMusicVolume, 1f) * 100f;

            _sfxSlider.value = sfxPct;
            _musicSlider.value = musicPct;
            _sfxValueLabel.text = $"{Mathf.RoundToInt(sfxPct)}";
            _musicValueLabel.text = $"{Mathf.RoundToInt(musicPct)}";

            // show only the settings overlay
            _pauseMenu.style.display = DisplayStyle.None;
            _settingsPanel.style.display = DisplayStyle.Flex;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void ShowMain()
        {
            // hide settings, show pause overlay + main panel
            _settingsPanel.style.display = DisplayStyle.None;
            _pauseMenu.style.display = DisplayStyle.Flex;
            _mainPanel.style.display = DisplayStyle.Flex;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        void SaveSettings()
        {
            // persist both volumes (0–100 → 0–1)
            PlayerPrefs.SetFloat(PrefSfxVolume, _sfxSlider.value / 100f);
            PlayerPrefs.SetFloat(PrefMusicVolume, _musicSlider.value / 100f);
            PlayerPrefs.Save();

            // optionally apply immediately
            AudioListener.volume = PlayerPrefs.GetFloat(PrefSfxVolume, 1f);
            // TODO: route music to your mixer group

            // return to the main pause overlay
            ShowMain();
        }
    }
}