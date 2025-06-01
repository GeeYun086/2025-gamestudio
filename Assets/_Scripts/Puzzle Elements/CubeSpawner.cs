using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
     /// (Re)spawns assigned (<see cref="_fuseTime"/>) cube on load and interact.
     /// </summary>
    public class CubeSpawner : MonoBehaviour
    {
        public GameObject Cube;
        GameObject _currentCube;
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
            Destroy(_currentCube);
            _currentCube = newCube;

            newCube.TryGetComponent(out GravityModifier gravityModifier);
            gravityModifier.GravityDirection = -transform.up;
        }
    }
}