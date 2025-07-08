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

        // Buttons only
        Button _newGameButton;
        Button _continueButton;
        Button _settingsButton;
        Button _creditsButton;
        Button _quitButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // grab buttons
            _newGameButton = root.Q<Button>("NewGameButton");
            _continueButton = root.Q<Button>("ContinueButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _creditsButton = root.Q<Button>("CreditsButton");
            _quitButton = root.Q<Button>("QuitButton");

            if (_newGameButton == null || _continueButton == null ||
                _settingsButton == null || _creditsButton == null || _quitButton == null) {
                Debug.LogError("MainMenu: UI element missing—check UXML names!");
                return;
            }

            // apply custom font & size to buttons
            if (OcrFont != null) {
                var styleFont = new StyleFont(OcrFont);
                foreach (var btn in new[] { _newGameButton, _continueButton, _settingsButton, _creditsButton, _quitButton }) {
                    btn.style.unityFont = styleFont;
                    btn.style.fontSize = 40;
                }
            } else {
                Debug.LogWarning("MainMenu: OcrFont not set—using default font.");
            }

            // wire up callbacks
            _newGameButton.clicked += () => SceneManager.LoadScene("MainScene");
            _continueButton.clicked += OnContinueClicked;
            _settingsButton.clicked += OnSettingsClicked;
            _creditsButton.clicked += OnCreditsClicked;
            _quitButton.clicked += OnQuitClicked;
        }

        void OnContinueClicked()
        {
            Debug.Log("Continue clicked – implement your save/load here.");
        }

        void OnSettingsClicked()
        {
            Debug.Log("Settings clicked – open settings panel.");
        }

        void OnCreditsClicked()
        {
            Debug.Log("Credits clicked – show credits screen.");
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