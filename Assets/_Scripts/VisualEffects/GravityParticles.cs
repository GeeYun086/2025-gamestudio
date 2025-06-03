using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame
{
    public class GravityParticles : MonoBehaviour
    {
        GravityModifier _currentGravity;
        Vector2 currentDirection;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currentGravity = GetComponent<GravityModifier>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
