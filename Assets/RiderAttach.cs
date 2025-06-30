using UnityEngine;

namespace GravityGame
{
    [RequireComponent(typeof(Collider))]
    public class RiderAttach : MonoBehaviour
    {
        [Tooltip("Drag in your spider's carrySocket here.")]
        [SerializeField] Transform _carrySocket;

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(_carrySocket, true);
                other.transform.localPosition = Vector3.zero;
                other.transform.localRotation = Quaternion.identity;
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