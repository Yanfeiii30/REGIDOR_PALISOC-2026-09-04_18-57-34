using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }
}