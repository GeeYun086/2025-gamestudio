using Codice.Client.Common.EventTracking;
using UnityEngine;

namespace GravityGame
{
    public class PressurePlate : RedstoneComponent
    {
        [SerializeField] bool _isPowered;
        public override bool IsPowered
        {
            get => _isPowered;
            set {
                _isPowered = value;
                OnTriggerEvent();
            }
        }

        void OnTriggerEvent()
        {
            if (CompareTag("Block") && !_isPowered) {
                _isPowered = true;
            }
        }
    }
}