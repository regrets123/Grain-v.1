using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class AntiFallDamageScript : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        PlayerCombat player = other.GetComponent<PlayerCombat>();
        if (player != null)
        {
            player.StartCoroutine("PreventFallDamage");     //Hindrar spelaren från att ta fallskada under en viss tid
        }
    }
}