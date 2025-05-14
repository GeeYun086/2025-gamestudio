using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Whenever an object of a <see cref="GravityModifier.ColorGroup" /> changes <see cref="GravityModifier.GravityDirection" />,
    ///     the object invokes <see cref="AlertColorGroup" />. All objects within the <see cref="GravityModifier.ColorGroup" /> are then notified to change their <see cref="GravityModifier.GravityDirection" /> respectivly.
    /// </summary>
    public class ColorGroupHandler : MonoBehaviour
    {
        public static ColorGroupHandler Instance;
        public delegate void OnColorGroupDirectionChangeDelegate(Vector3 newDirection);

        public event OnColorGroupDirectionChangeDelegate OnRedGroupGravityDirectionChange;
        public event OnColorGroupDirectionChangeDelegate OnBlueGroupGravityDirectionChange;
        public event OnColorGroupDirectionChangeDelegate OnGreenGroupGravityDirectionChange;

        void Awake()
        {
            if(!Instance)
                Instance=this;
            else
                Destroy(this);
        }

        public void AlertColorGroup(GravityModifier.ColorGroup colorGroup, Vector3 newGravityDirection)
        {
            switch (colorGroup) {
                case GravityModifier.ColorGroup.Red:
                    OnRedGroupGravityDirectionChange?.Invoke(newGravityDirection);
                    break;
                case GravityModifier.ColorGroup.Blue:
                    OnBlueGroupGravityDirectionChange?.Invoke(newGravityDirection);
                    break;
                case GravityModifier.ColorGroup.Green:
                    OnGreenGroupGravityDirectionChange?.Invoke(newGravityDirection);
                    break;
            }
        }
    }
}
