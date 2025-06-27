using UnityEngine;

namespace GravityGame.SaveAndLoadSystem
{
    /// <summary>
    ///     Interface for objects that support saving and loading of their state through the <see cref="SaveAndLoad" /> system.
    ///     Implement this interface when you need to save/load game state data of an object .
    /// </summary>
    public interface ISaveData
    {
        string SaveToJson();
        void LoadFromJson(string jsonData);

        /// <summary>
        ///     Unique per-object save data id. Set automatically by <see cref="AssignSaveIDs" />.
        ///     Must be implemented as a serialized field: <code>[field: SerializeField] public int SaveDataID { get; set; }</code>
        /// </summary>
        int SaveDataID { get; set; }
        bool ShouldBeSaved => true;

        void OnAfterLoad() {}
    }

    /// <summary>
    ///     Generic extension of <see cref="ISaveData" /> that allows you to
    ///     specify your data struct that you want to save/load.
    /// </summary>
    /// <typeparam name="TSaveData"> Your type representing the object's saved state. (Must have the [Serializable] attribute). </typeparam>
    public interface ISaveData<TSaveData> : ISaveData
    {
        TSaveData Save();
        void Load(TSaveData data);

        string ISaveData.SaveToJson() => JsonUtility.ToJson(Save());
        void ISaveData.LoadFromJson(string jsonData) => Load(JsonUtility.FromJson<TSaveData>(jsonData));
    }
}