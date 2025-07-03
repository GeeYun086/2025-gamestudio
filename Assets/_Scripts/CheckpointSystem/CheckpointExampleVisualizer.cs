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

        void OnEnable()
        {
            _checkpoint = GetComponent<Checkpoint>();
            _renderer = GetComponent<Renderer>();

            if (_renderer) _renderer.material.color = _normalColor;
            
            if (!_checkpoint) return;
            _checkpoint.OnHasBeenReachedChanged += HandleStateChange;
        }

        void OnDisable()
        {
            if (!_checkpoint) return;
            _checkpoint.OnHasBeenReachedChanged -= HandleStateChange;
        }

        void HandleStateChange()
        {
            if (_checkpoint.IsActiveCheckpoint) _renderer.material.color = _isActiveCheckpointColor;
            else if (_checkpoint.HasBeenReached) _renderer.material.color = _hasBeenReached;
            else _renderer.material.color = _normalColor;
        }
    }
}