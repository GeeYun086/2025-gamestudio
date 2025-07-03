using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiderAttach : MonoBehaviour
{
    [Tooltip("Drag in your spider's carrySocket here.")]
    [SerializeField] private Transform _carrySocket;

    [Header("Attach Detection")]
    [SerializeField] private float _attachRadius = 1f;
    [SerializeField] private LayerMask _playerLayer;
    
    [Header("Spring Settings")]
    [SerializeField] private float _posSpring   = 800f;
    [SerializeField] private float _posDamper   = 50f;
    [SerializeField] private float _rotSpring   = 500f;
    [SerializeField] private float _rotDamper   = 50f;

    [Header("Penetration Penalty")]
    [SerializeField] float _penSpring = 1000f;
    [SerializeField] float _penDamper = 100f;

    Collider _carryCollider;
    Collider _playerCollider;

    private Rigidbody _playerRb;
    private bool _canAttach = false;
    private bool _isRiding  = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        col.bounds.Expand(_attachRadius);
    }

    void OnTriggerStay(Collider other)
    {
        if (_isRiding) return;
        if ((_playerLayer & 1 << other.gameObject.layer) == 0) return;
        
        Vector3 dirToSocket = (_carrySocket.position - other.transform.position).normalized;
        float dot = Vector3.Dot(dirToSocket, _carrySocket.up);
        if (dot < 0.1f) return;
        
        _playerRb  = other.attachedRigidbody;
        _playerCollider = other;
        if(_playerRb == null) return;

        _playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        _isRiding = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_canAttach || _isRiding) return;
        if (!other.CompareTag("Player")) return;

        _playerRb = other.attachedRigidbody;
        if (_playerRb == null) return;
        _playerCollider = other.GetComponent<CapsuleCollider>();
        _carryCollider = GetComponentInChildren<BoxCollider>();

        _playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _playerRb.interpolation         = RigidbodyInterpolation.Interpolate;

        _isRiding = true;
    }

    void Update()
    {
        if (_isRiding && Input.GetButtonDown("Jump"))
        {
            _isRiding = false;
            _playerRb = null;
            _playerCollider = null;
        }
    }

    void FixedUpdate()
    {
        if (!_isRiding || _playerRb == null) return;

        Vector3 error    = _carrySocket.position - _playerRb.position + new Vector3(0, _carryCollider.bounds.extents.y/2, 0);
        Vector3 vel      = _playerRb.linearVelocity;
        Vector3 force    = error * _posSpring - vel * _posDamper;
        _playerRb.AddForce(force, ForceMode.Acceleration);


        Vector3 surfUp  = _carrySocket.up;
        Vector3 forwardOnPlane = Vector3.Cross(-_carrySocket.right, surfUp).normalized;
        Quaternion targetRot    = Quaternion.LookRotation(forwardOnPlane, surfUp);

        Quaternion qError = targetRot * Quaternion.Inverse(_playerRb.rotation);
        qError.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;
        float angleRad = angleDeg * Mathf.Deg2Rad;

        Vector3 angVel = _playerRb.angularVelocity;
        Vector3 torque = axis.normalized * (angleRad * _rotSpring)
                         - angVel * _rotDamper;
        _playerRb.AddTorque(torque, ForceMode.Acceleration);

        /*if (_playerCollider != null && _carryCollider != null) {
            bool overlapped = Physics.ComputePenetration(
                _playerCollider, _playerCollider.transform.position, _playerCollider.transform.rotation, 
                _carryCollider, _carryCollider.transform.position,_carryCollider.transform.rotation,
                out var sepDir, out float sepDist
            );
            if (overlapped && sepDist > 0f) {
                Vector3 velAlong = Vector3.Project(_playerRb.linearVelocity, sepDir);
                Vector3 penForce = sepDir * (sepDist * _penSpring) -velAlong * _penDamper;
                _playerRb.AddForce(penForce, ForceMode.Acceleration);
            }
        }*/
    }
    
    public bool CanAttach
    {
        get => _canAttach;
        set
        {
            _canAttach = value;
            if (!_canAttach && _isRiding)
            {
                _isRiding = false;
                _playerRb = null;
            }
        }
    }
}
