using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.VisualEffects
{
    public class GravityFluid : MonoBehaviour
    {
        Renderer _renderer;
        [SerializeField] GravityModifier _gravity;
        [SerializeField] float _swooshDuration = 0.5f;
        float _startTime;
        Vector3 _currentGravity;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponents();
            _currentGravity = _gravity.GravityDirection;
        }

        void GetComponents()
        {
            if (!_renderer) _renderer = GetComponent<Renderer>();
            if (!_gravity) _gravity = GetComponent<GravityModifier>();
        }
        

        // Update is called once per frame
        void Update()
        {
            //TODO: slerp gravity vector so fluid movement isn't abrupt
            if (_gravity && _currentGravity != _gravity.GravityDirection) {
                if (_startTime < 0) {
                    _startTime = Time.time;
                }
                float frac = (Time.time - _startTime) / _swooshDuration;
                if (_renderer.materials.Length > 1) {
                    foreach (Material m in _renderer.materials)
                    {
                        if (m.name == "GravityFluid (Instance)") {
                            m.SetVector("_GravityDirection", Vector3.Slerp(_currentGravity,_gravity.GravityDirection.normalized,frac));;    
                        }
                    }   
                } else {
                    _renderer.material.SetVector("_GravityDirection", Vector3.Slerp(_currentGravity,_gravity.GravityDirection.normalized,frac));
                }
                if (frac >= 1) {
                    _startTime = -1;
                    _currentGravity = _gravity.GravityDirection;
                }
            }
        }
    }
}
