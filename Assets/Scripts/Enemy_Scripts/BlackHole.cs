using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class BlackHole : MonoBehaviour
{
    void OnTriggerStay(Collider other)      //Drar spelaren mot en punkt
    {
        if(other.gameObject.tag == "Player")
        {
            other.transform.position = Vector3.Lerp(other.transform.position, transform.position, 0.02f);
        }
    }
}
