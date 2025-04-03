using UnityEngine;

namespace GravityGame.Utils
{
    /// <summary>
    ///     Locks the world rotation of the attached object to (0, 0, 0)
    /// </summary>
    [ExecuteInEditMode]
    public class FixedWorldRotation : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}