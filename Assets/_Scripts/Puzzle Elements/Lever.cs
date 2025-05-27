using System.Collections.Generic;
using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Add to Lever GameObject
    /// </summary>
    public class Lever : RedstoneComponent
    {
        [SerializeField] bool _isPowered;

        /// <summary>
        /// Need to Add Door script to doors and drag those GameObjects into this script of the Lever
        /// </summary>
        [SerializeField] public List<RedstoneComponent> LogicComponents = new List<RedstoneComponent>();

        public override bool IsPowered
        {
            get => _isPowered;
            set
            {
                _isPowered = value;
                UpdateConnectedComponents();
            }
        }


        /// <summary>
        /// Changes IsPowered of all Doors in List.
        /// Switch between on and off.
        /// Need to set in Door`s scripts which Doors are already powered.
        /// </summary>
        void UpdateConnectedComponents()
        {
            foreach (var component in LogicComponents)
            {
                if (component != null)
                    component.IsPowered = !component.IsPowered;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Carl: To debug mechanism without the need of playing the level.
        /// But also updates IsPowered while in Play mode.
        /// </summary>
        void OnValidate()
        {
            UpdateConnectedComponents();
        }
#endif
    }
}