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
        public int ObjectID;
        public int DataTypeID;
        public string JsonData;
    }

    [Serializable]
    public struct GameSaveData
    {
        public List<SaveDataEntry> Entries;
    }

    public class SaveAndLoad : SingletonMonoBehavior<SaveAndLoad>
    {
        [NonSerialized] public GameSaveData Data = new(){Entries = new List<SaveDataEntry>()};

        public void Save()
        {
            var saveDataForObject = Data.Entries.ToDictionary(e => e.ObjectID + e.DataTypeID);
            foreach ((int objectID, int dataTypeID, var saveData) in FindObjectsWithSaveData()) {
                var jsonData = saveData.Save();
                var entry = new SaveDataEntry {
                    ObjectID = objectID,
                    DataTypeID = dataTypeID,
                    JsonData = jsonData
                };
                saveDataForObject[objectID + dataTypeID] = entry;
            }
            Data.Entries = saveDataForObject.Values.ToList();
        }

        public void Load()
        {
            var saveDataForObject = Data.Entries.ToDictionary(e => e.ObjectID + e.DataTypeID);
            foreach ((int objectID, int dataTypeID, var saveData) in FindObjectsWithSaveData()) {
                if (saveDataForObject.TryGetValue(objectID + dataTypeID, out var objData)) {
                    saveData.Load(objData.JsonData);
                }
            }
        }

        static IEnumerable<(int ObjectID, int DataTypeID, IWithRawSaveData SaveData)> FindObjectsWithSaveData()
        {
            var savableObjects = FindObjectsByType<SaveID>(FindObjectsSortMode.None);
            foreach (var savableObject in savableObjects) {
                foreach (var saveData in savableObject.GetComponents<IWithRawSaveData>()) {
                    var objectId = savableObject.ObjectID;
                    var dataTypeId = saveData.SaveDataTypeID;
                    yield return (objectId, dataTypeId, saveData);
                }
            }
        }
    }
}