using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    /// Provides centralized access to all UI elements in the Game UI Document,
    /// ensuring no field is ever null by falling back to empty stubs when an element is missing.
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

        public readonly Slider VolumeSlider;

        /// <summary>
        /// Queries the root document for all named UI elements. 
        /// If an element isn't found, a new stub is assigned to prevent null references.
        /// </summary>
        /// <param name="root">The root VisualElement of the UI Document.</param>
        public GameUIElements(VisualElement root)
        {
            GravityDirectionRadialMenu = root.Q<GravityDirectionRadialMenu>("RadialMenu")
                                         ?? new GravityDirectionRadialMenu();
            DebugElement              = root.Q<VisualElement>("Debug")
                                         ?? new VisualElement();

            PauseMenu    = root.Q<VisualElement>("PauseMenu")    ?? new VisualElement();
            MainPanel    = root.Q<VisualElement>("MainPanel")    ?? new VisualElement();
            SettingsPanel = root.Q<VisualElement>("SettingsPanel") ?? new VisualElement();

            ResumeButton    = root.Q<Button>("ResumeButton")    ?? new Button();
            SettingsButton  = root.Q<Button>("SettingsButton")  ?? new Button();
            MainMenuButton  = root.Q<Button>("MainMenuButton")  ?? new Button();
            BackButton      = root.Q<Button>("BackButton")      ?? new Button();

            VolumeSlider    = root.Q<Slider>("VolumeSlider")    ?? new Slider();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

        /// <summary>
        /// Ensures that only one instance of GameUI exists in the MainScene.
        /// Any duplicate or out-of-scene instances are destroyed immediately.
        /// </summary>
        private void Awake()
        {
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

        /// <summary>
        /// Initializes the UIDocument and constructs the GameUIElements wrapper.
        /// If no UIDocument is attached, uses an empty root to prevent null issues.
        /// </summary>
        private void OnEnable()
        {
            UIDocument = GetComponent<UIDocument>();
            var root = UIDocument != null
                ? UIDocument.rootVisualElement
                : new VisualElement();

            Elements = new GameUIElements(root);
        }
    }
}
