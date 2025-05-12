using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    /// Centralized access to all UI elements in the Game UI Document.
    /// Helps manage changes to the UI layout from a single point.
    /// </summary>
    public record GameUIElements(VisualElement Root)
    {
        // Existing HUD or debug tools
        public readonly GravityDirectionRadialMenu GravityDirectionRadialMenu = Root.Q<GravityDirectionRadialMenu>();
        public readonly VisualElement DebugElement = Root.Q<VisualElement>("Debug");

        // Pause menu panels
        public readonly VisualElement PauseMenu     = Root.Q<VisualElement>("PauseMenu");
        public readonly VisualElement MainPanel     = Root.Q<VisualElement>("MainPanel");
        public readonly VisualElement SettingsPanel = Root.Q<VisualElement>("SettingsPanel");

        // Buttons
        public readonly Button ResumeButton   = Root.Q<Button>("ResumeButton");
        public readonly Button SettingsButton = Root.Q<Button>("SettingsButton");
        public readonly Button MainMenuButton = Root.Q<Button>("MainMenuButton");
        public readonly Button QuitButton     = Root.Q<Button>("QuitButton");
        public readonly Button BackButton     = Root.Q<Button>("BackButton");

        // Volume slider
        public readonly Slider VolumeSlider = Root.Q<Slider>("VolumeSlider");
    }

    /// <summary>
    /// Singleton MonoBehaviour that manages the UI Document and element references.
    /// Ensures that all UI logic is accessible from a central instance.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

        /// <summary>
        /// Unity lifecycle method called when the GameObject is enabled.
        /// Sets up the singleton instance and initializes UI element bindings.
        /// </summary>
        void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance); // Prevent multiple instances
                Debug.LogError("Multiple GameUI instances detected. Destroying the old one.");
            }

            Instance = this;
            UIDocument = GetComponent<UIDocument>();
            Elements = new GameUIElements(UIDocument.rootVisualElement);
        }
    }
}
