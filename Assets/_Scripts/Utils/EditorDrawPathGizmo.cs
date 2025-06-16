using UnityEngine;

namespace GravityGame
{
    /// <summary>
    ///     This script draws a yellow sphere around the attached object (unless <see cref="drawSphere" /> is unchecked in
    ///     editor) and if the object has children, it will draw a line between them
    /// </summary>
    public class EditorDrawPathGizmo : MonoBehaviour
    {
        public bool drawSphere = true;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (drawSphere) Gizmos.DrawSphere(transform.position, 0.2f);

            int count = transform.childCount;
            if (count < 2) return;

            for (int i = 0; i < count - 1; i++) {
                var a = transform.GetChild(i);
                var b = transform.GetChild(i + 1);
                Gizmos.DrawLine(a.position, b.position);
            }
        }
    }
}