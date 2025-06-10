using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _speed;
        [SerializeField] List<Rigidbody> _onBelt;

        Material _material;
        void Start() => _material = GetComponent<MeshRenderer>().material;

        // Note FS: This is tested and looks the best
        void Update() => _material.mainTextureOffset += new Vector2(0, 1) * (_speed * 7f * Time.deltaTime);

        void FixedUpdate()
        {
            for (int i = 0; i <= _onBelt.Count - 1; i++) {
                // Note FS: Test with max rotation damping
                if (!_onBelt[i].GetComponent<PlayerMovement>()) _onBelt[i].angularDamping = 100f;
                _onBelt[i].AddForce(_speed * transform.forward, ForceMode.VelocityChange);
            }
        }


        void OnCollisionEnter(Collision collision) => _onBelt.Add(collision.rigidbody);

        void OnCollisionExit(Collision collision) => _onBelt.Remove(collision.rigidbody);
    }
}