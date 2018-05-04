using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class CameraCollision : MonoBehaviour
{
    [SerializeField]
    float minDistance = 1.0f;
    [SerializeField]
    float maxDistance = 5.0f;
    [SerializeField]
    float smooth = 10.0f;
    [SerializeField]
    Vector3 dollyDirAdjusted;
    [SerializeField]
    float distance;
    [SerializeField]
    LayerMask layerToMask;

    Vector3 dollyDir;
    	
	void Awake ()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
	}
	
	void LateUpdate ()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);

        RaycastHit hit;

        //Raycast checks for objects and camera adjust when objects are hit so it does not clip through terrain
        if(Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, layerToMask))
        {
            //Clamp the distance
            distance = Mathf.Clamp((hit.distance * 0.7f), minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }

        //Lerps the cameras local position and smooths it out
        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
	}
}
