using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private bool useNextBuildScene = true;

    public void LoadNextScene()
    {
        if (!string.IsNullOrWhiteSpace(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
            return;
        }

        if (!useNextBuildScene)
        {
            Debug.LogWarning("[NextSceneButton] No target scene name is set.");
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("[NextSceneButton] Current scene is already the last scene in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}