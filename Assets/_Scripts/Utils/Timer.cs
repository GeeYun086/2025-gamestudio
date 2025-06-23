using System;
using UnityEngine;

namespace GravityGame.Utils
{
    [Serializable]
    public class Timer
    {
        public float Duration; 
        public float StartTime { get; set; }
        
        public bool IsActive => StartTime + Duration > Time.time;

        public Timer(float duration)
        {
            Duration = duration;
        }

        public void Start() => StartTime = Time.time;
        public void Stop() => StartTime = 0;
    }
}