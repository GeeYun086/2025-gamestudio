using System;
using System.Collections.Generic;
using System.Linq;
using GravityGame.Utils;
using UnityEngine;

namespace GravityGame.CheckpointSystem
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

    public class SaveAndLoad : SingletonMonoBehavior<SaveAndLoad>
    {
        [NonSerialized] public GameSaveData Data = new() { Entries = new List<SaveDataEntry>() };

        public void Save()
        {
            var data = Data.Entries.ToDictionary(e => e.DataID);
            foreach (var (_, saveData) in FindObjectsWithSaveData()) {
                string jsonData = saveData.Save();
                var entry = new SaveDataEntry {
                    DataID = saveData.ID,
                    JsonData = jsonData
                };
                data[saveData.ID] = entry;
            }
            Data.Entries = data.Values.ToList();
        }

        public void Load()
        {
            var data = Data.Entries.ToDictionary(e => e.DataID);
            foreach (var (_, saveData) in FindObjectsWithSaveData()) {
                if (data.TryGetValue(saveData.ID, out var objData)) {
                    saveData.Load(objData.JsonData);
                }
            }
        }

        public static IEnumerable<(GameObject, IWithSaveData)> FindObjectsWithSaveData()
        {
            foreach (var obj in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                if (obj.TryGetComponent<IWithSaveData>(out var saveData)) {
                    yield return (obj.gameObject, saveData);
                }
            }
        }
    }
}