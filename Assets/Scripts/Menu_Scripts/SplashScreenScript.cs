using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*By Björn Andersson*/

public class SplashScreenScript : MonoBehaviour
{

    [SerializeField]
    float time;

    void Start()
    {
        StartCoroutine("LoadMenuSceneTimer");
    }

    private void Update()
    {
        if (Input.anyKeyDown)
            LoadMenuScene();
    }

    IEnumerator LoadMenuSceneTimer()
    {
        yield return new WaitForSeconds(time);
        LoadMenuScene();
    }

    void LoadMenuScene()
    {
        SceneManager.LoadScene("MainMenu_JP_Final");
    }
}
