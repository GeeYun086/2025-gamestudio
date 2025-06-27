using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    [RequireComponent(typeof(Checkpoint))]
    [RequireComponent(typeof(Renderer))]
    public class CheckpointExampleVisualizer : MonoBehaviour
    {
        Checkpoint _checkpoint;
        Renderer _renderer;

        readonly Color _normalColor = Color.gray;
        readonly Color _hasBeenReached = Color.yellow;
        readonly Color _isActiveCheckpointColor = Color.green;

        void Awake()
        {
            _checkpoint = GetComponent<Checkpoint>();
            _renderer = GetComponent<Renderer>();

            if (_renderer) _renderer.material.color = _normalColor;
        }

        void OnEnable()
        {
            if (!_checkpoint) return;
            _checkpoint.OnHasBeenReachedChanged += HandleHasBeenReachedChanged;
            // _checkpoint.OnIsActiveCheckpointChanged += HandleIsActiveCheckpointChanged;
            HandleStateChange(_checkpoint.HasBeenReached, false/*_checkpoint.IsActiveCheckpoint*/);
        }

        void OnDisable()
        {
            if (!_checkpoint) return;
            _checkpoint.OnHasBeenReachedChanged -= HandleHasBeenReachedChanged;
            // _checkpoint.OnIsActiveCheckpointChanged -= HandleIsActiveCheckpointChanged;
        }

        void HandleHasBeenReachedChanged(bool isReached) => HandleStateChange(isReached, false/*_checkpoint.IsActiveCheckpoint*/);

        void HandleIsActiveCheckpointChanged(bool isActive) => HandleStateChange(_checkpoint.HasBeenReached, isActive);

        void HandleStateChange(bool isReached, bool isActive)
        {
            if (isActive) _renderer.material.color = _isActiveCheckpointColor;
            else if (isReached) _renderer.material.color = _hasBeenReached;
            else _renderer.material.color = _normalColor;
        }
    }
}