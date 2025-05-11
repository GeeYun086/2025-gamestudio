using System;
using UnityEngine;

namespace GravityGame
{
    //Carl: Add to Door GameObject
    public class Door : RedstoneComponent
    {
        [SerializeField] private bool isPowered;

        public override bool IsPowered
        {
            get => isPowered;
            set { isPowered = value; }
        }
    }
}