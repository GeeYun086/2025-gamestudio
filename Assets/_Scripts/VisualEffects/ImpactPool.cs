using System.Collections.Generic;
using UnityEngine;

namespace GravityGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int MaxPoolCount = 5;
        readonly Queue<GameObject> _pool = new();
        int _total;

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