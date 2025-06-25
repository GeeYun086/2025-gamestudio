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
            EditorSceneManager.sceneSaving += (_, _) => AssignSaveIDsToLoadedObjects();
            EditorApplication.playModeStateChanged += p => {
                if (p == PlayModeStateChange.EnteredPlayMode)
                    AssignSaveIDsToLoadedObjects();
            };
        }

        static void AssignSaveIDsToLoadedObjects()
        {
            int count = 0;
            foreach (var (gameObject, saveData) in SaveAndLoad.FindObjectsWithSaveData()) {
                var id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
                saveData.SaveDataID = id.GetHashCode();
                count++;
            }
            Debug.Log($"[{typeof(AssignSaveIDs)}] assigned save ids to objects in scene ({count}).");
        }
    }
}
#endif