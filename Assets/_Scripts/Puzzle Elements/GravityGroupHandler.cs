using GravityGame.Gravity;
using GravityGame.Utils;
using UnityEngine;
using static GravityGame.Gravity.GravityModifier;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Whenever an object of a <see cref="GravityModifier.GravityGroup" /> changes
    ///     <see cref="GravityModifier.GravityDirection" />,
    ///     the object invokes <see cref="AlertGravityGroup" />. All objects within the
    ///     <see cref="GravityModifier.GravityGroup" /> are then notified to change their
    ///     <see cref="GravityModifier.GravityDirection" /> respectivly.
    /// </summary>
    public static class GravityGroupHandler
    {
        public delegate void OnGravityGroupDirectionChangeDelegate(
            GravityGroup gravityGroup,
            Vector3 newDirection
        );

        public static event OnGravityGroupDirectionChangeDelegate OnGravityGroupDirectionChange;


        public static void AlertGravityGroup(GravityGroup gravityGroup, Vector3 newGravityDirection)
        {
            if (gravityGroup != GravityGroup.None)
                OnGravityGroupDirectionChange?.Invoke(gravityGroup, newGravityDirection);
        }
    }
}