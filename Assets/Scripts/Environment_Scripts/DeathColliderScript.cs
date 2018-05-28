using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class DeathColliderScript : MonoBehaviour {
        
    private void OnTriggerEnter(Collider other)     //Dödar spelaren om denne träffar collidern
    {
        PlayerCombat player = other.GetComponent<PlayerCombat>();
        if (player != null)
        {
            player.Kill();
        }
    }
}
