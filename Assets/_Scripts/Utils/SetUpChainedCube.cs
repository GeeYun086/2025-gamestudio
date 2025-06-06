using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GravityGame.Utils
{
    public class SetUpChainedCube : MonoBehaviour
    {
        GameObject _previousLink;
        GameObject _currentLink;
        
        [SerializeField] GameObject _firstLink;

        void Start()
        {
            _currentLink = _firstLink;
            Transform cube = _currentLink.GetComponent<Joint>().connectedBody.transform;
            //_currentLink.transform.position = cube.position+Vector3.up * cube.localScale.y;
            
            
            int i = 0;
            while ((_currentLink.transform.position - transform.position).magnitude > .10f && i<100) {
                GameObject temp = Instantiate(_currentLink, _currentLink.transform.position + Vector3.up *.12f, _currentLink.transform.rotation, cube.transform);
                _previousLink = _currentLink;
                _currentLink = temp;

                _currentLink.GetComponent<Joint>().connectedBody = _previousLink.GetComponent<Rigidbody>();
            i++;
            }
            GetComponent<Joint>().connectedBody = _currentLink.GetComponent<Rigidbody>();
        }
    }
}
