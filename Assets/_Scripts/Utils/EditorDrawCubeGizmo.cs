using UnityEngine;

namespace GravityGame.Utils
{
    /// <summary>
    ///     This script draws a cube at the position of the attached object with the given size
    /// </summary>
    public class EditorDrawCubeGizmo : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            if(enabled) Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}