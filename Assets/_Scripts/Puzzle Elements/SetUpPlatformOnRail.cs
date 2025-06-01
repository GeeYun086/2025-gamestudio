using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class SetUpPlatformOnRail : MonoBehaviour
    {
        [SerializeField] MeshRenderer _platformRenderer;
        [SerializeField] MeshRenderer _railRenderer;
        [SerializeField] GameObject _startPosition;
        [SerializeField] GameObject _endPosition;

        [SerializeField] Direction _platformNormalParallelToAxis;

        enum Direction
        {
            X,
            Y,
            Z
        }
    void Awake()
        {
            _platformRenderer.enabled = true;
            _railRenderer.enabled = true;
        }
    }
}
