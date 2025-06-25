using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    
    public interface IWithSaveData
    {
        int ID { get; internal set; }
        
        string Save();
        void Load(string jsonData);
    }

    public interface IWithSaveData<T> : IWithSaveData
    {
        new T Save();
        void Load(T data);

        string IWithSaveData.Save()
        {
            var data = Save();
            return JsonUtility.ToJson(data);
        }
        
        void IWithSaveData.Load(string jsonData)
        {
            var data = JsonUtility.FromJson<T>(jsonData);
            Load(data);
        }
    }
}