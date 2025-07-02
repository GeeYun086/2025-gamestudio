using UnityEngine;
using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.LoadScene("[2.1]Cargo", LoadSceneMode.Additive);
        SceneManager.LoadScene("[2.2]Cargo", LoadSceneMode.Additive);
        SceneManager.LoadScene("[2.3]Cargo", LoadSceneMode.Additive);
        SceneManager.LoadScene("[2.4]Cargo", LoadSceneMode.Additive);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
