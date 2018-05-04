using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class BlackHoleDamage : MonoBehaviour
{
    [SerializeField]
    int blackHoleDamage;

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.GetComponent<PlayerControls>().TakeDamage(blackHoleDamage, DamageType.Physical);      //Får spelaren att ta skada då denne sugs in i det svarta hålet
            other.gameObject.GetComponent<PlayerControls>().StartCoroutine("Invulnerability");
        }
    }
		
}
