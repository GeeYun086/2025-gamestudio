using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GravityGame.Utils
{
    public class SetUpChainedCube : MonoBehaviour
    {
        GameObject _previousLink;
        GameObject _currentLink;

        [SerializeField] GameObject _link;
        [SerializeField] GameObject _cube;
        [SerializeField] GameObject _anchor;
        [SerializeField] GameObject _anchorPoint;
        [SerializeField] GameObject _cubePoint;

        void Awake()
        {
            _cubePoint.GetComponent<Renderer>().enabled = false;
            _cube.SetActive(true);
            _anchor.SetActive(true);

            _anchor.transform.position = _anchorPoint.transform.position;
            _cube.transform.position = _anchorPoint.transform.position
                                       + (_anchorPoint.transform.position - _cubePoint.transform.position).magnitude * Vector3.down;
            _cube.transform.localScale = new Vector3(_cube.transform.localScale.x, _cubePoint.transform.localScale.y, _cube.transform.localScale.z);
            _link.transform.parent = transform;
            _cube.transform.localScale = _cubePoint.transform.localScale;
            _link.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);

            _link.GetComponent<Joint>().connectedBody = _cube.GetComponent<Rigidbody>();
        }

        void Start()
        {
            _currentLink = _link;

            int i = 0;
            while ((_currentLink.transform.position - _anchor.transform.position).magnitude > .10f && i < 100) {
                GameObject temp = Instantiate(
                    _currentLink, _currentLink.transform.position + transform.up * .4f, _currentLink.transform.rotation, transform
                );
                _previousLink = _currentLink;
                _currentLink = temp;

                _currentLink.GetComponent<Joint>().connectedBody = _previousLink.GetComponent<Rigidbody>();
                i++;
            }
            _anchor.GetComponent<Joint>().connectedBody = _currentLink.GetComponent<Rigidbody>();
            SetLookRotation();
            transform.position += _anchorPoint.transform.position - _anchor.transform.position;
        }

        void SetLookRotation()
        {
            Vector3 up = (_anchorPoint.transform.position - _cubePoint.transform.position).normalized;
            Vector3 arbitrary = Mathf.Abs(Vector3.Dot(up, Vector3.forward)) < 0.99f ? Vector3.forward : Vector3.right;

            // Generate a forward vector orthogonal to up
            Vector3 forward = Vector3.Cross(up, arbitrary).normalized;

            // Recalculate right and get a corrected forward that is exactly orthogonal
            Vector3 right = Vector3.Cross(up, forward);
            forward = Vector3.Cross(right, up);

            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}