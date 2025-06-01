using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class SetUpPlatformOnRail : MonoBehaviour
    {
        [SerializeField] GameObject _platform;
        [SerializeField] GameObject _rail;
        [SerializeField] GameObject _startPosition;
        [SerializeField] GameObject _endPosition;

        [SerializeField] Direction _platformNormalParallelToAxis;

        enum Direction { X, Y, Z }

        void Awake()
        {
            _platform.GetComponent<MeshRenderer>().enabled = true;
            _rail.GetComponentInChildren<MeshRenderer>().enabled = true;
            SetUpRail();
            RotatePlatform();
            _rail.GetComponent<ConfigurableJoint>().connectedBody = _platform.GetComponent<Rigidbody>();
        }

        void RotatePlatform()
        {
            Vector3 reorientation = -_rail.GetComponent<Rigidbody>().rotation.eulerAngles;
            Vector3 rotate = Vector3.zero;
            switch (_platformNormalParallelToAxis) {
                case Direction.X:
                    rotate = new Vector3(0, 0, 90);
                    break;
                case Direction.Z:
                    rotate = new Vector3(0, 90, 90);
                    break;
                case Direction.Y:
                    break;
            }
            _platform.GetComponent<Rigidbody>().rotation = Quaternion.Euler(reorientation + rotate);
        }

        void SetUpRail()
        {
            Vector3 path = _endPosition.transform.position - _startPosition.transform.position;
            if (path == Vector3.zero)
                return;
            float length = path.magnitude;
            Vector3 middlePoint = _startPosition.transform.position + path * .5f;
            _rail.transform.position = middlePoint;
            _rail.GetComponentInChildren<Transform>().localScale = new Vector3(1f, 1f, length);
            _rail.GetComponent<ConfigurableJoint>().linearLimit = new SoftJointLimit {
                limit = length / 2f
            };
            _rail.transform.rotation = Quaternion.LookRotation(path);
        }
    }
}