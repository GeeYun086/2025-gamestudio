using UnityEngine;

namespace GravityGame.Audio
{
    [RequireComponent(typeof(Rigidbody))]
    public class CubeCollisionSound : MonoBehaviour
    {
        public AudioSource CubeSound; // cube collision sound
        
        public AudioClip CollisionSound;
        public AudioClip DragSound;
        // note: maxCollisionVelocity will be used to set at what speed the music volume will be max
        public int MaxCollisionVelocity = 10;
        private float MinSpeed = 0.1f;
        private int _frameCounter = 1;
        private const int CheckEveryNFrames = 4;


        [SerializeField] const float min_volume = 0.4f;
        [SerializeField] const float max_volume = 1f;
        [SerializeField] const float minRelativeVelocity = 1f;

        


        private Rigidbody _rB;

        void Start()
        {
            _rB = GetComponent<Rigidbody>();
            
        }

        // Note: collision when cube fell without any contact with wall
        // Note: linear interpolation to get volume from different height
        void OnCollisionEnter(Collision collision)
        {
            float velocity = collision.relativeVelocity.magnitude;
            if (velocity >= minRelativeVelocity)
            {
                // Stop dragging sound if it's playing
                if (CubeSound.isPlaying && CubeSound.clip == DragSound)
                {
                    CubeSound.Stop();
                }

                // Set volume and play collision sound
                CubeSound.volume = Mathf.Clamp(velocity / MaxCollisionVelocity, min_volume, max_volume);
                CubeSound.PlayOneShot(CollisionSound);
            }
        }

        //Note:: this is for dragging sound of cube 
        //Note:: it calculates for 3 frames whether the cube is moving or not 
        void OnCollisionStay(Collision collision)
        {
            _frameCounter++;
            if (_frameCounter % CheckEveryNFrames != 0) return;

            if (_rB.linearVelocity.magnitude > MinSpeed && IsGroundContact(collision))
            {
                

                if (!CubeSound.isPlaying || CubeSound.clip != DragSound)
                {
                    CubeSound.clip = DragSound;
                    CubeSound.pitch = Random.Range(0.3f, 0.9f);
                    CubeSound.loop = false;
                    CubeSound.Play();
                }
            }
            else
            {
                if (CubeSound.isPlaying && CubeSound.clip == DragSound)
                {
                    CubeSound.Stop();
                }
            }
        }

        void OnCollisionExit()
        {
            if (CubeSound.isPlaying && CubeSound.clip == DragSound)
            {
                CubeSound.Stop();
            }
        }

        //Note:: it is to check whether the cube is touching the wall or ground till 120 degree
        // only collision first value are used to reduce calculation load
        bool IsGroundContact(Collision collision)
        {
            return collision.contactCount > 0;
        } 
    } 
}