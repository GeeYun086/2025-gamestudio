using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityGame
{
    [RequireComponent(typeof(Collider))]
    public class RiderAttach : MonoBehaviour
    {
        [Tooltip("Drag in your spider's carrySocket here.")]
        [SerializeField] Transform _carrySocket;
        GameObject _player;
        public bool CanAttach = false;

        void Update()
        {
            if(_player != null)
                _player.transform.localPosition = Vector3.zero;
            if (Input.GetButtonDown("Jump") && _player != null) {
                _player.transform.SetParent(null, true);
                _player = null;
            }
        }

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (CanAttach && other.CompareTag("Player"))
            {
                _player = other.gameObject;
                _player.transform.SetParent(_carrySocket, true);
                _player.transform.localPosition = Vector3.zero;
                _player.transform.localRotation = Quaternion.identity;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(null, true);
            }
        }
    }
}