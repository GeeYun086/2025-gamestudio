using UnityEngine;

namespace GravityGame.Audio
{
    public class CubeCollisionSound : MonoBehaviour
    {
        public AudioSource CubeCollide; // cube collision sound
        public AudioSource DragSound; // cube dragging friction sound
        
        // note: maxCollisionVelocity will be used to set at what speed the music volume will be max
        public int MaxCollisionVelocity = 10;
        private int _frameCounter = 1;
        private const int CheckEveryNFrames = 4;


        private Rigidbody _rB;

        void Start()
        {
            _rB = GetComponent<Rigidbody>();
            
        }

        // Note: collision when cube fell without any contact with wall
        // Note: linear interpolation to get volume from different height
        void OnCollisionEnter(Collision collision)
        {
            
            if (collision.relativeVelocity.magnitude >= 1f)
            {
                CubeCollide.volume = Mathf.Clamp(collision.relativeVelocity.magnitude / MaxCollisionVelocity, 0.5f, 1f) * 1f;
                CubeCollide.PlayOneShot(CubeCollide.clip);
            }
        }

        //Note:: this is for dragging sound of cube 
        //Note:: it calculates for 3 frames whether the cube is moving or not 
        void OnCollisionStay(Collision collision)
        {
            _frameCounter++;
            if (_frameCounter % CheckEveryNFrames != 0) return;

            if (_rB.linearVelocity.magnitude > 0.1f && IsGroundContact(collision))
            {
                

                if (!DragSound.isPlaying)
                {
                    DragSound.pitch = Random.Range(0.3f, 0.8f);
                    DragSound.Play();
                }
            }
            else
            {
                if (DragSound.isPlaying)
                {
                    DragSound.Stop();
                    CubeCollide.PlayOneShot(CubeCollide.clip);
                }
            }
        }

        void OnCollisionExit()
        {
            if (DragSound.isPlaying)
            {
                DragSound.Stop();
            }
        }

        //Note:: it is to check whether the cube is touching the wall or ground till 120 degree
        // only collision first value are used to reduce calculation load
        bool IsGroundContact(Collision collision)
        {
            if (collision.contactCount > 0)
            {
                return true;
            }

            else
            {
                return false;
            }
        } 
    } 
}