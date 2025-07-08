using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    ///     Centralized access to all UI elements in the Game UI Document.
    ///     Ensures no field is ever null by falling back to empty stubs when an element is missing.
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
        public readonly Button BackButton;
        public readonly Button SaveButton;
        public readonly Slider SfxVolumeSlider;
        public readonly Slider MusicVolumeSlider;

        public GameUIElements(VisualElement root)
        {
            GravityDirectionRadialMenu = root.Q<GravityDirectionRadialMenu>("RadialMenu") ?? new GravityDirectionRadialMenu();
            DebugElement = root.Q<VisualElement>("Debug") ?? new VisualElement();
            PauseMenu = root.Q<VisualElement>("PauseMenu") ?? new VisualElement();
            MainPanel = root.Q<VisualElement>("MainPanel") ?? new VisualElement();
            SettingsPanel = root.Q<VisualElement>("SettingsPanel") ?? new VisualElement();
            ResumeButton = root.Q<Button>("ResumeButton") ?? new Button();
            SettingsButton = root.Q<Button>("SettingsButton") ?? new Button();
            MainMenuButton = root.Q<Button>("MainMenuButton") ?? new Button();
            BackButton = root.Q<Button>("BackButton") ?? new Button();
            SaveButton = root.Q<Button>("SaveButton") ?? new Button();
            SfxVolumeSlider = root.Q<Slider>("SfxVolumeSlider") ?? new Slider();
            MusicVolumeSlider = root.Q<Slider>("MusicVolumeSlider") ?? new Slider();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

        void Awake()
        {
            // Optional: Only allow this GameUI in a specific scene
            // if (SceneManager.GetActiveScene().name != "MainScene") { Destroy(gameObject); return; }

            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnEnable()
        {
            UIDocument = GetComponent<UIDocument>();
            var root = UIDocument != null
                ? UIDocument.rootVisualElement
                : new VisualElement();

            Elements = new GameUIElements(root);
        }
    }
}