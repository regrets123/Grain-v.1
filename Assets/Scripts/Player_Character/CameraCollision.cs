
/*using UnityEngine;
using System.Collections;
public class CameraCollision : MonoBehaviour
{

    [Header("Camera Properties")]
    private float DistanceAway;                     //how far the camera is from the player.

    public float minDistance = 1;                //min camera distance
    public float maxDistance = 2;                //max camera distance

    public float DistanceUp = -2;                    //how high the camera is above the player
    public float smooth = 4.0f;                    //how smooth the camera moves into place
    public float rotateAround = 70f;            //the angle at which you will rotate the camera (on an axis)

    [Header("Player to follow")]
    public Transform target;                    //the target the camera follows

    [Header("Layer(s) to include")]
    public LayerMask CamOcclusion;                //the layers that will be affected by collision

    [Header("Map coordinate script")]
    //    public worldVectorMap wvm;
    RaycastHit hit;
    float cameraHeight = 55f;
    float cameraPan = 0f;
    float camRotateSpeed = 180f;
    Vector3 camPosition;
    Vector3 camMask;
    Vector3 followMask;

    private float HorizontalAxis;
    private float VerticalAxis;

    // Use this for initialization
    void Start()
    {
        //the statement below automatically positions the camera behind the target.

        target = GameObject.Find("CameraFollow").transform;
        rotateAround = target.eulerAngles.y - 45f;
    }

    void LateUpdate()
    {

        HorizontalAxis = Input.GetAxis("Horizontal");
        VerticalAxis = Input.GetAxis("Vertical");

        //Offset of the targets transform (Since the pivot point is usually at the feet).
        Vector3 targetOffset = new Vector3(target.position.x, (target.position.y + 2f), target.position.z);
        Quaternion rotation = Quaternion.Euler(cameraHeight, rotateAround, cameraPan);
        Vector3 vectorMask = Vector3.one;
        Vector3 rotateVector = rotation * vectorMask;
        //this determines where both the camera and it's mask will be.
        //the camMask is for forcing the camera to push away from walls.
        //camPosition = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;
        camMask = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;
        occludeRay(ref targetOffset);

        smoothCamMethod();


        #region wrap the cam orbit rotation
        if (rotateAround > 360)
        {
            rotateAround = 0f;
        }
        else if (rotateAround < 0f)
        {
            rotateAround = (rotateAround + 360f);
        }
        #endregion

        rotateAround += HorizontalAxis * camRotateSpeed * Time.deltaTime;
        DistanceUp = Mathf.Clamp(DistanceUp += VerticalAxis, -0.79f, 2.3f);
        DistanceAway = Mathf.Clamp(DistanceAway + VerticalAxis, minDistance, maxDistance);
        //transform.LookAt(target);

    }
    void smoothCamMethod()
    {
        //smooth = 4f;
        transform.parent.localPosition = Vector3.Lerp(transform.parent.position, camPosition, Time.deltaTime * smooth);

    }
    void occludeRay(ref Vector3 targetFollow)
    {
        #region prevent wall clipping
        //declare a new raycast hit.
        RaycastHit wallHit = new RaycastHit();
        //linecast from your player (targetFollow) to your cameras mask (camMask) to find collisions.
        if (Physics.Linecast(targetFollow, camMask, out wallHit, CamOcclusion))
        {
            //the smooth is increased so you detect geometry collisions faster.
            smooth = 10f;
            //the x and z coordinates are pushed away from the wall by hit.normal.
            //the y coordinate stays the same.
            camPosition = new Vector3(wallHit.point.x + wallHit.normal.x * 0.5f, camPosition.y, wallHit.point.z + wallHit.normal.z * 0.5f);
        }
        #endregion
    }
}
*/








using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
