using UnityEngine;

namespace GravityGame.RespawnSystem
{
    public class RespawnController : MonoBehaviour
    {
        public static RespawnController Instance { get; private set; }

        [SerializeField] GameObject playerObject;

        Vector3 _currentRespawnPosition;
        Quaternion _currentRespawnRotation;
        CharacterController _playerCharacterController;
        
        void Awake()
        {
            Instance = this;
            _currentRespawnPosition = playerObject.transform.position;
            _currentRespawnRotation = playerObject.transform.rotation;
            _playerCharacterController = playerObject.GetComponent<CharacterController>();
        }
        
        void SetCurrentCheckpoint(Vector3 newPos, Quaternion newRot)
        {
            _currentRespawnPosition = newPos;
            _currentRespawnRotation = newRot;
        }
        
        void RespawnPlayer()
        {
            _playerCharacterController.enabled = false;
            
            playerObject.transform.position = _currentRespawnPosition;
            playerObject.transform.rotation = _currentRespawnRotation;
            
            _playerCharacterController.enabled = true;
        }
    }
}