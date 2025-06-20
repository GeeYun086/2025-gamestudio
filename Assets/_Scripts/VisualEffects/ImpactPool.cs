using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Implements GameObject pool enabling reuse of generic GameObjects instead of destroying and instantiating all the time
    /// </summary>
    public class ImpactPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int MaxPoolCount = 5;
        readonly Queue<GameObject> _pool = new();
        int _total;

        void Start()
        {
            _total = 0;
        }

        public GameObject GetObject()
        {
            if (_pool.Count > 0) {
                var obj = _pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            if (_total < MaxPoolCount) {
                _total++;
                return Instantiate(Prefab);
            }
            return null;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}