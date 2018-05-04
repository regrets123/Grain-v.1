using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*By Andreas Nilsson*/

public class LoadTrigger : MonoBehaviour
{
    [SerializeField]
    string [] loadNames, unloadNames;

	private void OnTriggerEnter(Collider col)       //Laddar in en scen additivt då spelaren träffar en collider
    {
        if (col.gameObject.tag == "Player")
        {
            if (loadNames != null)
            {
                foreach (string scene in loadNames)

                    StartCoroutine(DynamicSceneManager.instance.Load(scene));
            }

            if (unloadNames != null)
            {
                StartCoroutine("UnloadScene");
            }
        }
    }

    IEnumerator UnloadScene()       //Tar bort en laddad scen då spelaren träffar en collider
    {        
        yield return new WaitForSeconds(0.01f);
        foreach (string scene in unloadNames)
        {
            StartCoroutine(DynamicSceneManager.instance.UnLoad(scene));
        }
    }
}
