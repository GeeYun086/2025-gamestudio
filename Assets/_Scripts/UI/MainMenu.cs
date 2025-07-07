using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GravityGame.UI
{
    /// <summary>
    ///     Handles the main menu UI: styling, element binding, and button callbacks.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        [Header("Assign your OCRA.ttf Unity Font here")]
        public Font OcrFont;

        Label _titleLabel;
        Button _newGameButton;
        Button _continueButton;
        Button _settingsButton;
        Button _creditsButton;
        Button _quitButton;

        void OnEnable()
        {
            // Retrieve the root element from the attached UIDocument.
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Find UI elements by their names in the UXML.
            _titleLabel = root.Q<Label>("Title");
            _newGameButton = root.Q<Button>("NewGameButton");
            _continueButton = root.Q<Button>("ContinueButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _creditsButton = root.Q<Button>("CreditsButton");
            _quitButton = root.Q<Button>("QuitButton");

            // If any element is missing, log an error and abort initialization.
            if (_titleLabel == null ||
                _newGameButton == null ||
                _continueButton == null ||
                _settingsButton == null ||
                _creditsButton == null ||
                _quitButton == null) {
                Debug.LogError("MainMenu: One or more UI elements could not be found in UXML!");
                return;
            }

            // Apply OCRA font and custom styling if a font asset is assigned.
            if (OcrFont != null) {
                var sFont = new StyleFont(OcrFont);

                // Title label: set font, font size, and color.
                _titleLabel.style.unityFont = sFont;
                _titleLabel.style.fontSize = 46;
                _titleLabel.style.color = new StyleColor(Color.red);

                // Buttons: set font and font size.
                _newGameButton.style.unityFont = sFont;
                _newGameButton.style.fontSize = 20;
                _continueButton.style.unityFont = sFont;
                _continueButton.style.fontSize = 20;
                _settingsButton.style.unityFont = sFont;
                _settingsButton.style.fontSize = 20;
                _creditsButton.style.unityFont = sFont;
                _creditsButton.style.fontSize = 20;
                _quitButton.style.unityFont = sFont;
                _quitButton.style.fontSize = 20;
            } else {
                Debug.LogWarning("MainMenu: OcrFont is null. Text will use the default font.");
            }

            // Set the title text explicitly.
            _titleLabel.text = "Game title";

            // Register click event handlers for each button.
            _newGameButton.clicked += OnNewGameClicked;
            _continueButton.clicked += OnContinueClicked;
            _settingsButton.clicked += OnSettingsClicked;
            _creditsButton.clicked += OnCreditsClicked;
            _quitButton.clicked += OnQuitClicked;
        }

        /// <summary>
        ///     Loads the main game scene when starting a new game.
        /// </summary>
        void OnNewGameClicked()
        {
            SceneManager.LoadScene("MainScene");
        }

        /// <summary>
        ///     Placeholder for loading saved progress. To be implemented.
        /// </summary>
        void OnContinueClicked()
        {
            // TODO: Integrate save system to load existing game progress.
        }

        /// <summary>
        ///     Placeholder for opening the settings interface. To be implemented.
        /// </summary>
        void OnSettingsClicked()
        {
            // TODO: Open settings panel or scene.
        }

        /// <summary>
        ///     Placeholder for showing the credits screen. To be implemented.
        /// </summary>
        void OnCreditsClicked()
        {
            // TODO: Show credits panel or scene.
        }

        /// <summary>
        ///     Quits the application; if running in the editor, stops play mode.
        /// </summary>
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