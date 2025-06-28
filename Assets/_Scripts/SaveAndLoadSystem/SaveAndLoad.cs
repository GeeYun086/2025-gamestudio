using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public static GameSaveData New() => new () { Entries = new List<SaveDataEntry>() };
    }

    /// <summary>
    ///     Singleton that handles saving/loading of game state
    ///     Saves / Loads all objects that implement <see cref="ISaveData" />
    /// </summary>
    public class SaveAndLoad : SingletonMonoBehavior<SaveAndLoad>
    {
        [NonSerialized] public GameSaveData Data = GameSaveData.New();
        public bool ShouldSaveToFile = true;
        string SavePath => Application.persistentDataPath + "/SaveData.json"; 
        
        void OnEnable()
        {
            SceneManager.sceneLoaded += (_, _) => {
                // if(Data.Entries.Count == 0) Save(); // initial save
            };
            DontDestroyOnLoad(this);
        }
        
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
            Debug.Log($"[{nameof(SaveAndLoad)}] saved ({Data.Entries.Count}) entries.");
            
            if(ShouldSaveToFile) SaveToFile.Save(Data, SavePath); 
        }

        public void Load()
        {
            if(ShouldSaveToFile) Data = SaveToFile.Load(SavePath) ?? GameSaveData.New();
            
            var data = Data.Entries.ToDictionary(e => e.DataID);
            var toLoad = FindObjectsWithSaveData().ToList();
            foreach (var (_, saveData) in toLoad) {
                if (data.TryGetValue(saveData.SaveDataID, out var objData)) {
                    saveData.LoadFromJson(objData.JsonData);
                }
            }
            foreach (var (_, saveData) in toLoad) {
                saveData.OnAfterLoad();
            }
            Debug.Log($"[{nameof(SaveAndLoad)}] loaded ({Data.Entries.Count}) entries.");
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

    public static class SaveToFile
    {
        public static void Save(GameSaveData data, string path)
        {
            try {
                using var stream = new FileStream(path, FileMode.Create);
                using var writer = new StreamWriter(stream);
                writer.Write(JsonUtility.ToJson(data));
            } catch (Exception e) {
                Debug.Log($"An Error occured while saving to {path}: {e}");
            }
        }

        public static GameSaveData? Load(string path)
        {
            if (File.Exists(path)) {
                try {
                    using var stream = new FileStream(path, FileMode.Open);
                    using var reader = new StreamReader(stream);
                    return JsonUtility.FromJson<GameSaveData>(reader.ReadToEnd());    
                } catch (Exception e) {
                    Debug.Log($"An Error occured while loading save from {path}: {e}");
                    return null;
                }
            }
            return null;
        }
    }
}