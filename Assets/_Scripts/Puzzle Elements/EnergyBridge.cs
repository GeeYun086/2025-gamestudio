using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// IsActive controls wether the bridge is extended or not.
    /// Changing that value will cause the bridge to extend or retract smoothly.
    /// </summary>
    public class EnergyBridge : RedstoneComponent
    {
        float _bridgeLength;
        Vector3 _bridgePosition;
        Vector3 _bridgeDirection;
        public override bool IsPowered { get; set; }


        void Awake()
        {
            _bridgeLength = transform.localScale.x;
            _bridgePosition = transform.localPosition;
            _bridgeDirection = transform.localRotation * Vector3.right;
        }

        void Update()
        {
            float currentLength = transform.localScale.x;
            float targetLength = IsPowered ? _bridgeLength : 0f;
            if (Mathf.Abs(currentLength - targetLength) > 0.01f)
            {
                float newLength = Mathf.Lerp(currentLength, targetLength, Time.deltaTime * 5f);
                transform.localScale = new Vector3(newLength, transform.localScale.y, transform.localScale.z);
                float offset = (newLength - _bridgeLength) / 2f;
                transform.localPosition = _bridgePosition + _bridgeDirection * offset;
            }
        }
    }
}