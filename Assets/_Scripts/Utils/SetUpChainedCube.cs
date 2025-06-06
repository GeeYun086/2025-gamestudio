using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GravityGame.Utils
{
    public class SetUpChainedCube : MonoBehaviour
    {
        GameObject _previousLink;
        GameObject _currentLink;

        void Start()
        {
            _currentLink = gameObject.transform.GetChild(0).gameObject;
            int i = 0;
            while ((_currentLink.transform.position - transform.position).magnitude > .11f && i<1000) {
                GameObject temp = Instantiate(_currentLink, _currentLink.transform.position + Vector3.up *.08f, _currentLink.transform.rotation, transform);
                _previousLink = _currentLink;
                _currentLink = temp;
                
                Vector3 directionToPrevious = _previousLink.transform.position - _currentLink.transform.position;
                //_currentLink.transform.rotation = Quaternion.LookRotation(Vector3.forward, -directionToPrevious);

                _currentLink.GetComponent<Joint>().connectedBody = _previousLink.GetComponent<Rigidbody>();
            i++;
            }
            GetComponent<Joint>().connectedBody = _currentLink.GetComponent<Rigidbody>();
        }
    }
}
