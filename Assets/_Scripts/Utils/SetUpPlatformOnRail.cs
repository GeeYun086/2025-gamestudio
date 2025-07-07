using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Provides easier setup for Platform on Rails Prefab. User defines rail by moving the start- and endpoint. Platform
    ///     can either be defined by its normal being parallel to an axis or have a custom rotation.
    /// </summary>
    public class SetUpPlatformOnRail : MonoBehaviour
    {
        [SerializeField] GameObject _platform;
        [SerializeField] GameObject _customPlatformRotation;
        [SerializeField] GameObject _rail;
        [SerializeField] GameObject _startPosition;
        [SerializeField] GameObject _endPosition;

        [SerializeField] Direction _platformNormalParallelToAxis;

        enum Direction { X, Y, Z, CUSTOM }

        void Awake()
        {
            _platform.GetComponent<MeshRenderer>().enabled = true;
            _rail.GetComponentInChildren<MeshRenderer>().enabled = true;
            RotatePlatform();
            SetUpRail();
        }

        void RotatePlatform()
        {
            var rotate = Vector3.zero;
            switch (_platformNormalParallelToAxis) {
                case Direction.X:
                    rotate = new Vector3(0, 0, 90);
                    break;
                case Direction.Z:
                    rotate = new Vector3(0, 90, 90);
                    break;
                case Direction.CUSTOM:
                    rotate = _customPlatformRotation.transform.eulerAngles;
                    break;
            }
            _customPlatformRotation.SetActive(false);
            _platform.GetComponent<Rigidbody>().rotation = Quaternion.Euler(rotate);
            _platform.GetComponent<Rigidbody>().freezeRotation = true;
        }

        void SetUpRail()
        {
            var path = _endPosition.transform.position - _startPosition.transform.position;
            if (path == Vector3.zero)
                return;
            float length = path.magnitude;
            var middlePoint = _startPosition.transform.position + path * .5f;
            _rail.transform.parent.position = middlePoint;
            _rail.GetComponentInChildren<Transform>().localScale = new Vector3(1f, 1f, length);
            _rail.GetComponent<ConfigurableJoint>().linearLimit = new SoftJointLimit {
                limit = length / 2f,
                contactDistance = .05f
            };
            _rail.transform.parent.rotation = Quaternion.LookRotation(path);
        }
    }
}