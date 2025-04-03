using UnityEngine;

namespace GravityGame.Gravity
{
    /// <summary>
    ///     GameObjects with this component have custom gravity
    ///     that can be manipulated by editing <see cref="GravityDirection" /> and <see cref="GravityMagnitude" />
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityModifier : MonoBehaviour
    {
        public Vector3 GravityDirection = Vector3.down;
        public float GravityMagnitude = 9.81f;

        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
        }

        void FixedUpdate()
        {
            _rigidbody.AddForce(GravityDirection.normalized * GravityMagnitude, ForceMode.Acceleration);
        }
    }
}