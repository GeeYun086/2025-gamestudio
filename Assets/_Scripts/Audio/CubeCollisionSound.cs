using UnityEngine;

namespace GravityGame.Audio
{
    /// <summary>
    /// Plays collision and drag sounds for a cube with a Rigidbody.
    /// - Plays impact sound on collisions with volume based on collision intensity.
    /// - Plays looping drag sound when the cube slides on the ground.
    /// - Plays a soft "thump" sound when the cube stops moving after sliding.
    /// Attach this script to any cube GameObject with a Rigidbody and assign the appropriate AudioClips and AudioSource.
    /// </summary>
    
    
    // Ensures the object this script is attached to has a Rigidbody
    [RequireComponent(typeof(Rigidbody))]
    public class CubeCollisionSound : MonoBehaviour
    {
        // AudioSource responsible for playing drag and collision sounds
        public AudioSource CubeSound;

        // Sound played when cube collides (e.g., falls to ground)
        public AudioClip CollisionSound;

        // Sound played when cube is sliding on the ground
        public AudioClip DragSound;

        // Speed at which the collision sound reaches max volume
        public int MaxCollisionVelocity = 10;

        // Minimum speed threshold to consider the cube as "moving"
        private const float MinSpeed = 0.1f;

        // Used to throttle checks to every N frames for performance
        private int _frameCounter = 1;
        private const int CheckEveryNFrames = 4;

        // Tracks whether the cube was previously moving
        private bool _wasMoving;

        // Min and max volume bounds for sound playback
        private const float MinVolume = 0.4f;
        private const float MaxVolume = 1f;

        // Minimum relative collision velocity needed to trigger collision sound
        private const float MinRelativeVelocity = 1f;

        // Reference to the attached Rigidbody component
        private Rigidbody _rB;

        /// <summary>
        /// Unity Start method.
        /// Caches the Rigidbody component for physics-based speed checks.
        /// </summary>
        void Start()
        {
            _rB = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Called once when the cube starts colliding with another object.
        /// Plays a collision sound if the impact velocity exceeds a threshold.
        /// </summary>
        /// <param name="collision">Collision data provided by Unity</param>
        void OnCollisionEnter(Collision collision)
        {
            float velocity = collision.relativeVelocity.magnitude;

            if (velocity >= MinRelativeVelocity)
            {
                // If dragging sound is playing, stop it before playing the collision sound
                if (CubeSound.isPlaying && CubeSound.clip == DragSound)
                {
                    CubeSound.Stop();
                }

                // Adjust volume based on collision intensity and play impact sound
                // Changing MaxCollisionVelocity can change the loudness of the sound
                CubeSound.volume = Mathf.Clamp(velocity / MaxCollisionVelocity, MinVolume, MaxVolume);
                CubeSound.PlayOneShot(CollisionSound);
            }
            else
            {
                // If the collision is too soft, stop any sound (optional: may be removed)
                CubeSound.Stop();
            }
        }

        /// <summary>
        /// Called every physics frame the cube stays in contact with another surface.
        /// Handles drag sound playback while moving and a "thump" sound when movement stops.
        /// </summary>
        /// <param name="collision">Collision data from Unity</param>
        void OnCollisionStay(Collision collision)
        {
            // check till every "CheckEveryNFrames" that how many cube points are touching other objects
            _frameCounter++;
            if (_frameCounter % CheckEveryNFrames != 0) return;

            float speed = _rB.linearVelocity.magnitude; // Corrected linearVelocity
            bool isTouching = IsGroundContact(collision);

            // Case 1: Cube is sliding
            if (speed > MinSpeed && isTouching)
            {
                // If drag sound isn't playing or wrong clip is assigned, start dragging sound
                if (!CubeSound.isPlaying || CubeSound.clip != DragSound)
                {
                    CubeSound.clip = DragSound;
                    CubeSound.pitch = Random.Range(0.3f, 0.9f); // Add natural variation
                    CubeSound.loop = true;
                    CubeSound.Play();
                    //Debug.Log("dragging");
                }

                _wasMoving = true;
            }
            // Case 2: Cube just stopped moving (was moving before)
            else if (speed <= MinSpeed && _wasMoving && isTouching)
            {
                if (CubeSound.isPlaying && CubeSound.clip == DragSound)
                {
                    CubeSound.Stop(); // End dragging sound
                }

                // Play a soft "thump" sound indicating cube has come to rest
                CubeSound.pitch = Random.Range(0.3f, 0.9f);
                CubeSound.volume = MinVolume;
                CubeSound.PlayOneShot(CollisionSound);
                //Debug.Log("thump at stop");

                _wasMoving = false;
            }
        }

        /// <summary>
        /// Called once when the cube exits collision with any object.
        /// Stops any currently playing sound to ensure clean transitions.
        /// </summary>
        void OnCollisionExit()
        {
            if (CubeSound.isPlaying)
            {
                CubeSound.Stop();
            }
        }

        /// <summary>
        /// Lightweight check to determine if the cube is still in contact with a surface.
        /// Helps avoid unnecessary processing on collisions without contacts.
        /// </summary>
        /// <param name="collision">Collision info from Unity</param>
        /// <returns>True if contact points exist</returns>
        bool IsGroundContact(Collision collision)
        {
            return collision.contactCount > 0;
        }
    }
}
