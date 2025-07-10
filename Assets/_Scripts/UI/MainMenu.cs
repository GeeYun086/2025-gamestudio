using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GravityGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        [Header("Assign your OCRA.ttf Unity Font here")]
        public Font OcrFont;

#if UNITY_EDITOR
        [Header("Drag in your Scene asset here (Editor only)")]
        [SerializeField]
        SceneAsset _newGameSceneAsset;
#endif
        [Tooltip("Scene name to load (auto-filled from SceneAsset in Editor)")]
        [SerializeField]
        string _newGameSceneName = "";

        // Main‐menu buttons
        Button _newGameButton;
        Button _continueButton;
        Button _settingsButton;
        Button _creditsButton;
        Button _quitButton;

        // Settings panel & its controls
        VisualElement _settingsPanel;
        Button _backButton;
        Button _saveButton;
        Slider _sfxSlider;
        Slider _musicSlider;
        Label _sfxValueLabel;
        Label _musicValueLabel;

        void Awake()
        {
#if UNITY_EDITOR
            if (_newGameSceneAsset != null)
                _newGameSceneName = _newGameSceneAsset.name;
#endif
        }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Main menu buttons
            _newGameButton = root.Q<Button>("NewGameButton");
            _continueButton = root.Q<Button>("ContinueButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _creditsButton = root.Q<Button>("CreditsButton");
            _quitButton = root.Q<Button>("QuitButton");

            // Settings panel (embedded)
            _settingsPanel = root.Q<VisualElement>("SettingsPanel");
            _backButton = root.Q<Button>("BackButton");
            _saveButton = root.Q<Button>("SaveButton");
            _sfxSlider = root.Q<Slider>("SfxVolumeSlider");
            _musicSlider = root.Q<Slider>("MusicVolumeSlider");
            var labels = root.Query<Label>(name: "volumelabel").ToList();
            if (labels.Count >= 2) {
                _sfxValueLabel = labels[0];
                _musicValueLabel = labels[1];
            }

            // Validate
            if (_newGameButton == null || _settingsButton == null ||
                _settingsPanel == null || _backButton == null || _saveButton == null) {
                Debug.LogError("MainMenu: Missing UI element—check UXML names!");
                return;
            }

            // Hide settings panel at start
            _settingsPanel.style.display = DisplayStyle.None;

            // Apply OCR font styling
            if (OcrFont != null) {
                var styleFont = new StyleFont(OcrFont);
                foreach (var btn in new[] {
                             _newGameButton, _continueButton,
                             _settingsButton, _creditsButton, _quitButton,
                             _backButton, _saveButton
                         }) {
                    btn.style.unityFont = styleFont;
                    btn.style.fontSize = 40;
                }
            }

            // Main‐menu callbacks
            _newGameButton.clicked += OnNewGameClicked;
            _continueButton.clicked += OnContinueClicked;
            _settingsButton.clicked += ShowSettings;
            _creditsButton.clicked += OnCreditsClicked;
            _quitButton.clicked += OnQuitClicked;

            // Settings panel callbacks
            _backButton.clicked += HideSettings;
            _saveButton.clicked += SaveSettings;

            // Slider → Label binding
            _sfxSlider.RegisterValueChangedCallback(evt =>
                _sfxValueLabel.text = $"{Mathf.RoundToInt(evt.newValue)}"
            );
            _musicSlider.RegisterValueChangedCallback(evt =>
                _musicValueLabel.text = $"{Mathf.RoundToInt(evt.newValue)}"
            );
        }

        void OnNewGameClicked()
        {
            if (string.IsNullOrEmpty(_newGameSceneName)) {
                Debug.LogError("MainMenu: No scene name set for New Game!");
                return;
            }
            SceneManager.LoadScene(_newGameSceneName);
        }

        void OnContinueClicked()
        {
            Debug.Log("Continue clicked – implement save/load here.");
        }

        void ShowSettings()
        {
            // Simply show the overlay—main buttons remain visible underneath
            _settingsPanel.style.display = DisplayStyle.Flex;

            // Initialize sliders & labels
            float sfxPct = PlayerPrefs.GetFloat("SfxVolume", 1f) * 100f;
            float musicPct = PlayerPrefs.GetFloat("MusicVolume", 1f) * 100f;
            _sfxSlider.value = sfxPct;
            _musicSlider.value = musicPct;
            _sfxValueLabel.text = $"{Mathf.RoundToInt(sfxPct)}";
            _musicValueLabel.text = $"{Mathf.RoundToInt(musicPct)}";
        }

        void HideSettings()
        {
            // Just hide the overlay
            _settingsPanel.style.display = DisplayStyle.None;
        }

        void SaveSettings()
        {
            // Persist volumes
            PlayerPrefs.SetFloat("SfxVolume", _sfxSlider.value / 100f);
            PlayerPrefs.SetFloat("MusicVolume", _musicSlider.value / 100f);
            PlayerPrefs.Save();

            // Optionally apply immediately
            AudioListener.volume = PlayerPrefs.GetFloat("SfxVolume", 1f);

            HideSettings();
        }

        void OnCreditsClicked()
        {
            Debug.Log("Credits clicked – show credits here.");
        }

        void OnQuitClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}