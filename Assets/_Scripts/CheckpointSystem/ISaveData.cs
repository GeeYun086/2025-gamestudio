using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    public interface ISaveData
    {
        /// Set automatically by <see cref="AssignSaveIDs"/>
        /// Must be implemented as <code>[field: SerializeField] public int SaveDataID { get; set; }</code>
        int SaveDataID { get; set; }

        string Save();
        void Load(string jsonData);
    }

    public interface ISaveData<T> : ISaveData
    {
        new T Save();
        void Load(T data);

        string ISaveData.Save() => JsonUtility.ToJson(Save());
        void ISaveData.Load(string jsonData) => Load(JsonUtility.FromJson<T>(jsonData));
    }
}