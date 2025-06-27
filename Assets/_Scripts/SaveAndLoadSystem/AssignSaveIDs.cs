#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityGame.SaveAndLoadSystem
{
    [InitializeOnLoad]
    public static class AssignSaveIDs
    {
        static AssignSaveIDs()
        {
            EditorApplication.playModeStateChanged += p => {
                if (p == PlayModeStateChange.EnteredPlayMode)
                    AssignSaveIDsToLoadedObjects();
            };

            bool justSaved = false;
            EditorSceneManager.sceneSaving += (scene, path) => {
                if (justSaved) return;
                AssignSaveIDsToLoadedObjects();
                justSaved = true;
                var f = EditorSceneManager.SaveScene(scene, path);
                Debug.Log(f);
                justSaved = false;
            };

            AssignSaveIDsToLoadedObjects();
        }

        static void AssignSaveIDsToLoadedObjects()
        {
            int count = 0;
            foreach (var (gameObject, saveData) in SaveAndLoad.FindObjectsWithSaveData()) {
                var saveDataMonoBehaviour = (MonoBehaviour)saveData;
                int id = GlobalObjectId.GetGlobalObjectIdSlow(saveDataMonoBehaviour).GetHashCode();
                
                if (saveData.SaveDataID != id) {
                    saveData.SaveDataID = id;
                    // Mark the object as dirty so Unity knows it was modified
                    if (!Application.isPlaying) {
                        EditorUtility.SetDirty(saveDataMonoBehaviour);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(saveDataMonoBehaviour);
                        // Also mark the scene as dirty so the user is prompted to save it
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                    }
                    count++;
                }
            }
            Debug.Log($"[{typeof(AssignSaveIDs)}] assigned save ids to objects in scene ({count}).");
        }
    }
}
#endif