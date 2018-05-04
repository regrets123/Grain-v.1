using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class BaseTrapScript : MonoBehaviour {
    [SerializeField]
    GameObject trapObj;
    [SerializeField]
    Transform trapObjPos;

    //Trigger when player character enter specific collider area
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Spawns an object to damage player on hit, then destroys the collider so trap is not triggered again
            Instantiate(trapObj, trapObjPos.position, trapObjPos.rotation);
            Destroy(gameObject);
        }
    }
}
