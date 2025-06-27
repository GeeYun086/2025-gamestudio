using System;
using System.Collections.Generic;
using System.Linq;
using GravityGame.Utils;
using UnityEngine;

namespace GravityGame.SaveAndLoadSystem
{
    [Serializable]
    public struct SaveDataEntry
    {
        public int DataID;
        public string JsonData;
    }

    [Serializable]
    public struct GameSaveData
    {
        public List<SaveDataEntry> Entries;
    }

    /// <summary>
    ///     Singleton that handles saving/loading of game state
    ///     Saves / Loads all objects that implement <see cref="ISaveData" />
    /// </summary>
    public class SaveAndLoad : SingletonMonoBehavior<SaveAndLoad>
    {
        [NonSerialized] public GameSaveData Data = new() { Entries = new List<SaveDataEntry>() };

        public void Save()
        {
            var data = Data.Entries.ToDictionary(e => e.DataID);
            foreach (var (_, saveData) in FindObjectsWithSaveData()) {
                string jsonData = saveData.SaveToJson();
                var entry = new SaveDataEntry {
                    DataID = saveData.SaveDataID,
                    JsonData = jsonData
                };
                data[saveData.SaveDataID] = entry;
            }
            Data.Entries = data.Values.ToList();
            Debug.Log($"[{typeof(SaveAndLoad)}] saved ({Data.Entries.Count}) entries.");
        }

        public void Load()
        {
            var data = Data.Entries.ToDictionary(e => e.DataID);
            foreach (var (_, saveData) in FindObjectsWithSaveData()) {
                if (data.TryGetValue(saveData.SaveDataID, out var objData)) {
                    saveData.LoadFromJson(objData.JsonData);
                }
            }
            Debug.Log($"[{typeof(SaveAndLoad)}] loaded ({Data.Entries.Count}) entries.");
        }

        public static IEnumerable<(GameObject, ISaveData)> FindObjectsWithSaveData()
        {
            foreach (var obj in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                foreach (var saveData in obj.GetComponents<ISaveData>()) {
                    if (saveData is { ShouldBeSaved: true }) {
                        yield return (obj.gameObject, saveData);
                    }
                }
            }
        }
    }
}