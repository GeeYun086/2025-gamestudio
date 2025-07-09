using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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

        [FormerlySerializedAs("newGameSceneAsset")]
        [Header("Drag in your Scene asset here (Editor only)")]
    #if UNITY_EDITOR
        [SerializeField] private SceneAsset _newGameSceneAsset;
    #endif
        [FormerlySerializedAs("newGameSceneName")]
        [Tooltip("Scene name to load (auto-filled from SceneAsset in Editor)")]
        [SerializeField] private string _newGameSceneName = "";

        [Header("UI Button References (auto-wired)")]
        private Button _newGameButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _creditsButton;
        private Button _quitButton;

        void Awake()
        {
    #if UNITY_EDITOR
            // In the Editor, copy the asset name into our string
            if (_newGameSceneAsset != null)
            {
                _newGameSceneName = _newGameSceneAsset.name;
            }
    #endif
        }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Grab buttons by UXML name
            _newGameButton    = root.Q<Button>("NewGameButton");
            _continueButton   = root.Q<Button>("ContinueButton");
            _settingsButton   = root.Q<Button>("SettingsButton");
            _creditsButton    = root.Q<Button>("CreditsButton");
            _quitButton       = root.Q<Button>("QuitButton");

            if (_newGameButton == null || _continueButton == null ||
                _settingsButton == null || _creditsButton == null || _quitButton == null)
            {
                Debug.LogError("MainMenu: UI element missing—check UXML names!");
                return;
            }

            // Apply custom font & size
            if (OcrFont != null)
            {
                var styleFont = new StyleFont(OcrFont);
                foreach (var btn in new[] { _newGameButton, _continueButton, _settingsButton, _creditsButton, _quitButton })
                {
                    btn.style.unityFont = styleFont;
                    btn.style.fontSize = 40;
                }
            }
            else
            {
                Debug.LogWarning("MainMenu: OcrFont not set—using default font.");
            }

            // Wire up callbacks
            _newGameButton.clicked   += OnNewGameClicked;
            _continueButton.clicked  += OnContinueClicked;
            _settingsButton.clicked  += OnSettingsClicked;
            _creditsButton.clicked   += OnCreditsClicked;
            _quitButton.clicked      += OnQuitClicked;
        }

        private void OnNewGameClicked()
        {
            if (string.IsNullOrEmpty(_newGameSceneName))
            {
                Debug.LogError("MainMenu: No scene name set for New Game!");
                return;
            }
            SceneManager.LoadScene(_newGameSceneName);
        }

        private void OnContinueClicked()
        {
            Debug.Log("Continue clicked – implement your save/load here.");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings clicked – open settings panel.");
        }

        private void OnCreditsClicked()
        {
            Debug.Log("Credits clicked – show credits screen.");
        }

        private void OnQuitClicked()
        {
    #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }
}
