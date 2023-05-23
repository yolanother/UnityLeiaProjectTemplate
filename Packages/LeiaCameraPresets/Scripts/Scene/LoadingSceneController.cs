using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Image progressBar;

    private static int nextSceneIndex = -1;

    public static void LoadScene(int sceneIndex)
    {
        nextSceneIndex = sceneIndex;
        SceneManager.LoadScene(0); // Loading scene is at index 0
    }

    private void Start()
    {
        // If no next scene is specified or this is the first load, load scene 1 (main menu)
        if (nextSceneIndex < 0)
        {
            nextSceneIndex = 1;
        }

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(nextSceneIndex);
        while (!asyncOperation.isDone)
        {
            progressBar.fillAmount = asyncOperation.progress;
            yield return null;
        }
    }
}