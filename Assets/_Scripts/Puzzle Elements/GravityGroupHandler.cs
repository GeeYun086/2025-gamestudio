using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Whenever an object of a <see cref="GravityModifier.GravityGroup" /> changes <see cref="GravityModifier.GravityDirection" />,
    ///     the object invokes <see cref="AlertGravityGroup" />. All objects within the <see cref="GravityModifier.GravityGroup" /> are then notified to change their <see cref="GravityModifier.GravityDirection" /> respectivly.
    /// </summary>
    public class GravityGroupHandler : MonoBehaviour
    {
        public static GravityGroupHandler Instance { get; private set; }
        public delegate void OnGravityGroupDirectionChangeDelegate(GravityModifier.GravityGroup gravityGroup, Vector3 newDirection);
        public event OnGravityGroupDirectionChangeDelegate OnGravityGroupDirectionChange;

        void Awake()
        {
            if(!Instance)
                Instance=this;
            else
                Destroy(this);
        }

        public void AlertGravityGroup(GravityModifier.GravityGroup gravityGroup, Vector3 newGravityDirection)
        {
            if(gravityGroup != GravityModifier.GravityGroup.None)
                OnGravityGroupDirectionChange?.Invoke(gravityGroup,newGravityDirection);
        }
    }
}
