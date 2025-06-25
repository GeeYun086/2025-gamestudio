using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    
    public interface IWithRawSaveData
    {
        int DataTypeID { get; }
        
        string Save();
        void Load(string jsonData);
    }

    public interface IWithSaveData<T> : IWithRawSaveData
    {
        new T Save();
        void Load(T data);

        string IWithRawSaveData.Save()
        {
            var data = Save();
            return JsonUtility.ToJson(data);
        }
        
        void IWithRawSaveData.Load(string jsonData)
        {
            var data = JsonUtility.FromJson<T>(jsonData);
            Load(data);
        }
    }
}