using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace GravityGame.UI
{
    /// <summary>
    /// Centralized access to all UI elements in the Game UI Document.
    /// Helps manage changes to the UI layout from a single point.
    /// </summary>
    public record GameUIElements(VisualElement Root)
    {
        public readonly GravityDirectionRadialMenu GravityDirectionRadialMenu = Root.Q<GravityDirectionRadialMenu>();
        public readonly VisualElement DebugElement = Root.Q<VisualElement>("Debug");

        public readonly VisualElement PauseMenu     = Root.Q<VisualElement>("PauseMenu");
        public readonly VisualElement MainPanel     = Root.Q<VisualElement>("MainPanel");
        public readonly VisualElement SettingsPanel = Root.Q<VisualElement>("SettingsPanel");

        public readonly Button ResumeButton   = Root.Q<Button>("ResumeButton");
        public readonly Button SettingsButton = Root.Q<Button>("SettingsButton");
        public readonly Button MainMenuButton = Root.Q<Button>("MainMenuButton");
        public readonly Button QuitButton     = Root.Q<Button>("QuitButton");
        public readonly Button BackButton     = Root.Q<Button>("BackButton");

        public readonly Slider VolumeSlider = Root.Q<Slider>("VolumeSlider");
    }

    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

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

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != "MainScene") return;

            UIDocument = GetComponent<UIDocument>();
            Elements = new GameUIElements(UIDocument.rootVisualElement);
        }
    }
}
