using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*By Andreas Nilsson*/

public class CameraFollow : MonoBehaviour, IPausable
{
    [Header("Camera Settings")]
    [Space(2)]
    [SerializeField]
    float cameraMoveSpeed;
    [SerializeField]
    float clampAngle;
    [SerializeField]
    float inputSensitivity;
    [SerializeField]
    float smoothX;
    [SerializeField]
    float smoothY;

    [Space(10)]

    [Header("Lock-On Settings")]
    [Space(2)]
    [SerializeField]
    float lockOnSmooth;
    [SerializeField]
    float maxLockOnDistance;

    [Space(10)]

    [Header("Prefabs")]
    [Space(2)]
    [SerializeField]
    GameObject cameraObj;
    [SerializeField]
    GameObject playerObj;
    [SerializeField]
    GameObject lockOnSpritePrefab;
    [SerializeField]
    GameObject cameraFollowObj;

    GameObject lockOnSprite;

    PauseManager pM;

    Vector3 followPOS;

    List<BaseEnemyScript> targetsLockOnAble = new List<BaseEnemyScript>();

    List<BaseEnemyScript> visibleEnemies = new List<BaseEnemyScript>();

    BaseEnemyScript targetToLockOn, lookAtMe;

    Quaternion toRotate, localRotation;

    float camDistanceXToPlayer, camDistanceYToPlayer, camDistanceZToPlayer, distance, closestDistance, targetDistance;

    float mouseX, mouseY;

    float finalInputX, finalInputZ;

    float rotX = 0.0f, rotY = 0.0f;

    bool paused = false, lockOn = false;

    public float InputSensitivity
    {
        get { return this.inputSensitivity; }
        set { this.inputSensitivity = value; }
    }

    public bool LockOn
    {
        get { return lockOn; }
    }

    public BaseEnemyScript LookAtMe
    {
        get { return lookAtMe; }
    }
    
    void Start()
    {
        pM = FindObjectOfType<PauseManager>();
        pM.Pausables.Add(this);
        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
    }

    public void PauseMe(bool pausing)
    {
        paused = pausing;
    }

    void LateUpdate()       //Får kameran att följa efter spelaren
    {
        if (!playerObj.GetComponent<PlayerControls>().Dead)
        {
            if (!paused && !lockOn)
            {
                CameraRotation();

                transform.rotation = localRotation;
            }
            else if (lockOn && !paused)
            {
                lockOnSprite.transform.rotation = transform.rotation;
            }

            CameraUpdater();

            CameraLockOn();
        }
    }

    void CameraRotation()
    {
        //Get buttons for cameramovement from character controller, both for a console controller and PC controls
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        rotX += finalInputZ * inputSensitivity * Time.deltaTime;

        //clamps the x rotation so camera isn't able to spin around under the character
        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
    }

    //Lets the camera follow a specific object, in this case and empty object attached to the character
    void CameraUpdater()
    {
        //Object for camera to follow, in this case the player character
        Transform target = cameraFollowObj.transform;

        float step = cameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

    void CameraLockOn()     //Låter kameran fokusera på en fiende framför spelaren
    {
        if (Input.GetButtonDown("LockOn") && targetsLockOnAble != null && !lockOn)
        {
            visibleEnemies.Clear();

            foreach (BaseEnemyScript enemy in targetsLockOnAble)
            {
                RaycastHit hit;

                if (enemy != null && !Physics.Linecast(transform.position, enemy.transform.position, out hit, -(1 << 8)))
                {
                    visibleEnemies.Add(enemy);
                }
            }
            if (visibleEnemies.Count != 0)
            {
                closestDistance = visibleEnemies.Min(e => (e.transform.position - cameraFollowObj.transform.position).magnitude);

                foreach (BaseEnemyScript target in visibleEnemies)
                {
                    distance = Vector3.Distance(cameraFollowObj.transform.position, target.transform.position);

                    if (distance <= closestDistance)
                    {
                        lockOn = true;
                        lookAtMe = target;
                        lockOnSprite = Instantiate(lockOnSpritePrefab, lookAtMe.transform, false);
                        float spriteOffset;
                        switch (target.UnitName)
                        {
                            case "Guardian":
                                spriteOffset = 1.3f;
                                break;

                            case "Faminus":
                                spriteOffset = 2.5f;
                                break;

                            case "Raider":
                                spriteOffset = 0.5f;
                                break;

                            default:
                                spriteOffset = 0.4f;
                                break;
                        }
                        lockOnSprite.transform.position += (Vector3.up * spriteOffset);
                    }
                }
            }
        }
        else if (Input.GetButtonDown("LockOn") && lockOn)
        {
            Destroy(lockOnSprite.gameObject);
            lookAtMe = null;
            Vector3 rot = transform.localRotation.eulerAngles;
            rotX = rot.x;
            rotY = rot.y;
            lockOn = false;
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0f && lockOn)          //Låter spelaren välja vilken fiende kameran ska fokusera på
        {
            visibleEnemies.Clear();


            foreach (BaseEnemyScript enemy in targetsLockOnAble)
            {
                if (!Physics.Linecast(transform.position, enemy.transform.position, -(1 << 8)))
                {
                    visibleEnemies.Add(enemy);
                }
            }

            for (int i = 0; i < visibleEnemies.Count; i++)
            {

                if (visibleEnemies[i] == lookAtMe || visibleEnemies[i] != lookAtMe)
                {
                    int newIndex;

                    if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                        newIndex = i - 1;
                    }
                    else
                    {
                        newIndex = i + 1;
                    }

                    if (newIndex < 0)
                    {
                        lookAtMe = visibleEnemies[visibleEnemies.Count - 1];
                    }
                    else if (newIndex >= visibleEnemies.Count)
                    {
                        lookAtMe = visibleEnemies[0];
                    }
                    else
                    {
                        lookAtMe = visibleEnemies[newIndex];
                    }

                    Destroy(lockOnSprite.gameObject);

                    lockOnSprite = Instantiate(lockOnSpritePrefab, lookAtMe.transform, false);
                    lockOnSprite.transform.position += (Vector3.up * 0.4f);

                    break;
                }
            }
        }

        if (lookAtMe != null)
        {
            toRotate = Quaternion.LookRotation(lookAtMe.transform.position - transform.position);

            transform.rotation = Quaternion.Lerp(transform.rotation, toRotate, lockOnSmooth * Time.deltaTime);

            targetDistance = Vector3.Distance(cameraFollowObj.transform.position, lookAtMe.transform.position);

            if (!lookAtMe.gameObject.GetComponent<BaseEnemyScript>().Alive && lockOn || targetDistance > maxLockOnDistance && lockOn)
            {
                Destroy(lockOnSprite.gameObject);
                lookAtMe = null;
                Vector3 rot = transform.localRotation.eulerAngles;
                rotX = rot.x;
                rotY = rot.y;
                lockOn = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        targetToLockOn = other.gameObject.GetComponent<BaseEnemyScript>();

        if (targetToLockOn != null)
        {
            targetsLockOnAble.Add(targetToLockOn);
        }
    }

    void OnTriggerExit(Collider other)
    {
        targetsLockOnAble.Remove(other.gameObject.GetComponent<BaseEnemyScript>());
    }
}
