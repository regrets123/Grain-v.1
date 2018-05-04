using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class HealthBarRotation : MonoBehaviour {

    Camera cameraToLookAt;

    void Start()
    {
        cameraToLookAt = FindObjectOfType<Camera>();
    }

    void Update()       //Roterar fienders hälsomätare mot kameran
    {
        Vector3 v = cameraToLookAt.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(cameraToLookAt.transform.position - v);
        transform.Rotate(0, 180, 0);
    }
}
