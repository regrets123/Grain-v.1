using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* By Johanna Pettersson
 * Följande kod är lånad från: <https://www.youtube.com/watch?v=YMj2qPq9CP8&t=350s> */

public class LoadingManager : MonoBehaviour {

    public Slider progressBar;
    public GameObject loadingScreen;
    public Text progressText;

    public void LoadScene (string scene)
    {
        StartCoroutine(LoadingScene(scene));
    }

    IEnumerator LoadingScene (string scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.value = progress;
            progressText.text = progress * 100 + "%";
            print(progressBar.value);
            print(progressText.text);
            print(operation.progress);

            yield return null;
        }
    }

}
