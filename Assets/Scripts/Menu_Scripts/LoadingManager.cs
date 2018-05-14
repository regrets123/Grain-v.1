using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* By Johanna Pettersson
 * Följande kod är lånad från: <https://www.youtube.com/watch?v=YMj2qPq9CP8&t=350s> */

public class LoadingManager : MonoBehaviour
{

    [SerializeField]
    Slider progressBar;

    [SerializeField]
    GameObject loadingScreen;

    [SerializeField]
    Text progressText;

    public GameObject LoadingScreen
    {
        get { return this.loadingScreen; }
    }

    public bool Loaded
    {
        get;
        set;
    }

    public void LoadScene(string scene)
    {
        StartCoroutine(LoadingScene(scene));
    }

    IEnumerator LoadingScene(string scene)
    {
        AsyncOperation operation;
        if (scene == "Master Scene 1 - Managers")
            operation = SceneManager.LoadSceneAsync(scene);
        else
            operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Loaded = false;
            progressBar.value = progress;
            progressText.text = progress * 100 + "%";
            print(progressBar.value);
            print(progressText.text);
            print(operation.progress);
            yield return null;
        }
        Loaded = true;
    }

}
