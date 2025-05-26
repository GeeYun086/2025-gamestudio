using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace GravityGame.UI
{
    /// <summary>
    /// Centralized access to all UI elements in the Game UI Document.
    /// Guarantees every field is non-null (falls back to empty stubs if not found).
    /// </summary>
    public class GameUIElements
    {
        public readonly GravityDirectionRadialMenu GravityDirectionRadialMenu;
        public readonly VisualElement DebugElement;

        public readonly VisualElement PauseMenu;
        public readonly VisualElement MainPanel;
        public readonly VisualElement SettingsPanel;

        public readonly Button ResumeButton;
        public readonly Button SettingsButton;
        public readonly Button MainMenuButton;
        public readonly Button QuitButton;
        public readonly Button BackButton;

        public readonly Slider VolumeSlider;

        public GameUIElements(VisualElement root)
        {
            // If Q<>() returns null, we substitute a brand‐new stub so nobody ever sees a null.
            GravityDirectionRadialMenu = root.Q<GravityDirectionRadialMenu>("RadialMenu")
                                        ?? new GravityDirectionRadialMenu();
            DebugElement               = root.Q<VisualElement>("Debug")
                                        ?? new VisualElement();

            PauseMenu                  = root.Q<VisualElement>("PauseMenu")
                                        ?? new VisualElement();
            MainPanel                  = root.Q<VisualElement>("MainPanel")
                                        ?? new VisualElement();
            SettingsPanel              = root.Q<VisualElement>("SettingsPanel")
                                        ?? new VisualElement();

            ResumeButton               = root.Q<Button>("ResumeButton")
                                        ?? new Button();
            SettingsButton             = root.Q<Button>("SettingsButton")
                                        ?? new Button();
            MainMenuButton             = root.Q<Button>("MainMenuButton")
                                        ?? new Button();
            QuitButton                 = root.Q<Button>("QuitButton")
                                        ?? new Button();
            BackButton                 = root.Q<Button>("BackButton")
                                        ?? new Button();

            VolumeSlider               = root.Q<Slider>("VolumeSlider")
                                        ?? new Slider();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

        private void Awake()
        {
            // Only one GameUI in MainScene
            if (SceneManager.GetActiveScene().name != "MainScene")
            {
                Destroy(gameObject);
                return;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            // Grab the root if we have a UIDocument; otherwise use an empty root.
            UIDocument = GetComponent<UIDocument>();
            var root = (UIDocument != null)
                ? UIDocument.rootVisualElement
                : new VisualElement();

            Elements = new GameUIElements(root);
        }
    }
}
