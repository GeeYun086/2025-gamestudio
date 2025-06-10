using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _speed;
        [SerializeField] Vector3 _direction;
        [SerializeField] List<Rigidbody> _onBelt;

        Material _material;

        void OnValidate() => _direction = _direction.normalized;
        void Start() => _material = GetComponent<MeshRenderer>().material;

        // Note FS: This is tested and looks the best
        void Update() => _material.mainTextureOffset += new Vector2(1, 0) * (_speed * 10f * Time.deltaTime);

        void FixedUpdate()
        {
            for (int i = 0; i <= _onBelt.Count - 1; i++) _onBelt[i].AddForce(_speed * _direction, ForceMode.VelocityChange);
        }

        void OnCollisionEnter(Collision collision) => _onBelt.Add(collision.rigidbody);

        void OnCollisionExit(Collision collision) => _onBelt.Remove(collision.rigidbody);
    }
}