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
        Label _titleLabel;
        Button _newGameButton, _continueButton, _settingsButton, _creditsButton, _quitButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Bind UI elements
            _titleLabel = root.Q<Label>("Title");
            _newGameButton = root.Q<Button>("NewGameButton");
            _continueButton = root.Q<Button>("ContinueButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _creditsButton = root.Q<Button>("CreditsButton");
            _quitButton = root.Q<Button>("QuitButton");

            // Use the title label
            _titleLabel.text = "Game title";

            // Button click callbacks
            _newGameButton.clicked += OnNewGameClicked;
            _continueButton.clicked += OnContinueClicked;
            _settingsButton.clicked += OnSettingsClicked;
            _creditsButton.clicked += OnCreditsClicked;
            _quitButton.clicked += OnQuitClicked;
        }

        void OnNewGameClicked()
        {
            // Load the gameplay scene
            SceneManager.LoadScene("MainScene");
        }

        void OnContinueClicked()
        {
            // TODO: Load save system
        }

        void OnSettingsClicked()
        {
            // TODO: Open settings panel or scene
        }

        void OnCreditsClicked()
        {
            // TODO: Show credits panel or scene
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