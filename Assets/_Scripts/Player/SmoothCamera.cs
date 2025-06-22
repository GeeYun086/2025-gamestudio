using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Camera boom arm, smoothes sudden vertical movements of parent
    /// </summary>
    public class SmoothCamera : MonoBehaviour
    {
        public float TimeToCatchUp = 0.03f;

        Vector3 _lastPosition;

        void Update()
        {
            var goal = transform.parent.transform;
            var lastUp = Vector3.Project(_lastPosition, transform.up);
            var currUp = Vector3.Project(transform.position, transform.up);
            var pos = transform.position - currUp + lastUp; // just use last for the up axis

            transform.position = Vector3.Lerp(pos, goal.position, Time.deltaTime / TimeToCatchUp);
            _lastPosition = transform.position;
        }
    }
}