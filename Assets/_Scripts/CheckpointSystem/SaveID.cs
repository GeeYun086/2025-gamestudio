#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace GravityGame.CheckpointSystem
{
    /// <summary>
    ///     TODO Briefly explain what this class does / how it is used / what it is used for
    /// </summary>
    public class SaveID : MonoBehaviour
    { 
        public int ObjectID;

#if UNITY_EDITOR
        void OnValidate()
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
            ObjectID = id.ToString().GetHashCode();
        }
#endif
    }
}