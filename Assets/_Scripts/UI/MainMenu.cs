using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GravityGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private Label _titleLabel;
        private Button _newGameButton, _continueButton, _settingsButton, _creditsButton, _quitButton;

        private void OnEnable()
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

        private void OnNewGameClicked()
        {
            // Load the gameplay scene
            SceneManager.LoadScene("MainScene");
        }

        private void OnContinueClicked()
        {
            // TODO: Load save system
        }

        private void OnSettingsClicked()
        {
            // TODO: Open settings panel or scene
        }

        private void OnCreditsClicked()
        {
            // TODO: Show credits panel or scene
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