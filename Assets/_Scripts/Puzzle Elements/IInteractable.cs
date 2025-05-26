namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     interface for interactable objects
    /// </summary>
    public interface IInteractable
    {
        void Interact();
        bool IsInteractable { get; }
    }
}