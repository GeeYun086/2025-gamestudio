using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class EnergyBridge : MonoBehaviour
    {
        float _bridgeLength;
        float _bridgePosition;
        public bool IsActive = true;
        
        
        void Awake()
        {
            _bridgeLength = transform.localScale.x;
            _bridgePosition = transform.localPosition.x;
        }

        void Update()
        {
            float currentLength = transform.localScale.x;
            float targetLength = IsActive ? _bridgeLength : 0f;
            if (Mathf.Abs(currentLength - targetLength) > 0.01f)
            {
                float newLength = Mathf.Lerp(currentLength, targetLength, Time.deltaTime * 5f);
                transform.localScale = new Vector3(newLength, transform.localScale.y, transform.localScale.z);
                transform.localPosition = new Vector3(_bridgePosition + (newLength - _bridgeLength) / 2f, transform.localPosition.y, transform.localPosition.z);
            }
        }
    }
}