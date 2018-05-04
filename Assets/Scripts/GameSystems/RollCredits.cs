using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*By Johanna Pettersson*/

public class RollCredits : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene("Credits_JP_Final");     //Laddar en credits scen då spelaren träffar en collider i slutet av banan
        }
    }
}
