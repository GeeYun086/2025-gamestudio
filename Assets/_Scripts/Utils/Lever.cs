using System.Collections.Generic;
using UnityEngine;

namespace GravityGame
{
    //Carl: Add to Lever GameObject
    public class Lever : RedstoneComponent
    {
        [SerializeField] private bool isPowered;

        //Carl: Need to Add Door script to doors and drag those GameObjects into this script of the Lever
        [SerializeField] public List<RedstoneComponent> LogicComponents = new List<RedstoneComponent>();

        public override bool IsPowered
        {
            get => isPowered;
            set
            {
                isPowered = value;
                UpdateConnectedComponents();
            }
        }


        //Carl: Changes IsPowered of all Doors in List
        private void UpdateConnectedComponents()
        {
            foreach (var component in LogicComponents)
            {
                if (component != null)
                    component.IsPowered = IsPowered;
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            //Carl: To debug mechanism without the need of playing the level.
            UpdateConnectedComponents();
        }
#endif
    }
}