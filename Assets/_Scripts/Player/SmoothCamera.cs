using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Camera boom arm, smoothes sudden movements of parent
    /// </summary>
    public class SmoothCamera : MonoBehaviour
    {
        public float TimeToCatchUp = 0.03f;

        Vector3 _lastPosition;

        void Update()
        {
            var goal = transform.parent.transform;
            transform.position = Vector3.Lerp(_lastPosition, goal.position, Time.deltaTime/TimeToCatchUp);
            _lastPosition = transform.position;
        }
    }
}