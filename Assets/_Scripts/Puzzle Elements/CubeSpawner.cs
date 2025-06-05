using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class CubeSpawner : MonoBehaviour
    {
        public GameObject Cube;
        Vector3 _cubePosition;
        void Start()
        {
            _cubePosition = transform.position + transform.up;
            Respawn();
        }

        public void Respawn()
        {
            Debug.Log("Respawn");
            GameObject newCube = Instantiate(Cube, _cubePosition, transform.rotation, transform);
            Destroy(Cube);
            Cube = newCube;
            
            newCube.TryGetComponent(out GravityModifier gravityModifier);
            gravityModifier.GravityDirection = -transform.up;
        }
    }
}
