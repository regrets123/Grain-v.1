using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*By Andreas Nilsson*/

public class DynamicSceneManager : MonoBehaviour
{
    [SerializeField]
    private string masterSceneTriggers;

    Slider progressBar;
    GameObject loadingScreen;
    Text progressText;

    static DynamicSceneManager instance;

    public static DynamicSceneManager Instance { get { return instance; } set { if (instance == null) instance = value; } }

    private void Awake()        //Laddar in de områden som ska laddas då spelet startar
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        Instance = this;

        StartCoroutine(Load(masterSceneTriggers));
    }

    public IEnumerator Load(string sceneName)      //Laddar en scen additivt så att den är aktiv tillsammans med redan aktiva scener
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }

    public IEnumerator UnLoad(string sceneName)        //Stänger av en aktiv scen
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
