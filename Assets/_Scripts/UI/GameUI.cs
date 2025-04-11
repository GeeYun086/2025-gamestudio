using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    ///     This hosts the sub-elements that are queried from the root GameUI element centrally,
    ///     so we can adjust whenever this whenever the layout of the GameUI document changes,
    ///     and we need to query the elements differently.
    /// </summary>
    public record GameUIElements(VisualElement Root)
    {
        public readonly GravityDirectionRadialMenu GravityDirectionRadialMenu = Root.Q<GravityDirectionRadialMenu>();
        public readonly VisualElement DebugElement = Root.Q("Debug");
    }

    /// <summary>
    ///     Contains the UI document for the in-game player UI
    ///     If you want to add any other in-game UI, add it to the attached UIDocument
    ///     <remarks>
    ///         Note TG: If that is possible, I think we should make the pause or main menu UI their own UIDocuments
    ///     </remarks>
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        public UIDocument UIDocument { get; private set; }
        public GameUIElements Elements { get; private set; }

        void OnEnable()
        {
            if (Instance != null && Instance != this) {
                Destroy(Instance);
                Debug.LogError("More than one singleton instance in the scene!");
            }
            Instance = this;
            UIDocument = gameObject.GetComponent<UIDocument>();
            Elements = new GameUIElements(UIDocument.rootVisualElement);
        }
    }
}