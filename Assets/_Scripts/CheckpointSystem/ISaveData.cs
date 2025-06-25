using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    /// <summary>
    ///     Interface for objects that support saving and loading of their state through the <see cref="SaveAndLoad" /> system.
    ///     Implement this interface when you need to save/load game state data of an object .
    /// </summary>
    public interface ISaveData
    {
        string Save();
        void Load(string jsonData);

        /// <summary>
        ///     Unique per-object save data id. Set automatically by <see cref="AssignSaveIDs" />.
        ///     Must be implemented as a serialized field: <code>[field: SerializeField] public int SaveDataID { get; set; }</code>
        /// </summary>
        int SaveDataID { get; set; }
    }

    /// <summary>
    ///     Generic extension of <see cref="ISaveData" /> that allows you to
    ///     specify your data struct that you want to save/load.
    /// </summary>
    /// <typeparam name="TSaveData"> Your type representing the object's saved state. (Must have the [Serializable] attribute). </typeparam>
    public interface ISaveData<TSaveData> : ISaveData
    {
        new TSaveData Save();
        void Load(TSaveData data);

        string ISaveData.Save() => JsonUtility.ToJson(Save());
        void ISaveData.Load(string jsonData) => Load(JsonUtility.FromJson<TSaveData>(jsonData));
    }
}