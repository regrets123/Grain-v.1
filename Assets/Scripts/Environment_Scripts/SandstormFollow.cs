using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class SandstormFollow : MonoBehaviour
{
    [SerializeField]
    GameObject effectFollowObj;
    [SerializeField]
    float effectFollowSpeed;

	void LateUpdate ()      //Får en sandstorm att följa efter spelaren för att ge illusionen att det blåser över hela kartan
    {
        Transform target = effectFollowObj.transform;
        float step = effectFollowSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
