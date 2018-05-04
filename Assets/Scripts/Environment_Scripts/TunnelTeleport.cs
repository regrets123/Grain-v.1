using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class TunnelTeleport : MonoBehaviour {

    [SerializeField]
    Transform destination;

    private void OnTriggerEnter(Collider other)             //Teleporterar spelaren
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.position = destination.position;
            other.gameObject.transform.rotation = destination.rotation;
        }
    }
}
