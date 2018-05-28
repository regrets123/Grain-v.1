﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/*By Björn Andersson && Andreas Nilsson*/

public class PlayerMovement : MonoBehaviour, IPausable
{
    #region Serialized Variables

    [Header("Main Movement")]

    [Space(5)]

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    int rotspeed;

    [SerializeField]
    float sprintSpeed;

    [SerializeField]
    float slopeLimit;

    [SerializeField]
    GameObject cameraFollowObj;

    [Tooltip("SICK AIR BRAH!")]
    [SerializeField]
    bool airControl = false;

    [Space(10)]

    [Header("Stamina")]

    [Space(5)]

    [SerializeField]
    float maxStamina;

    [SerializeField]
    float staminaRegen;

    [SerializeField]
    float staminaRegenWait;

    [SerializeField]
    float staminaSprintDrain;

    [Space(10)]

    [Header("Jump")]

    [Space(5)]

    [SerializeField]
    float jumpForce;

    [SerializeField]
    float superJumpForce;

    [SerializeField]
    float jumpCooldown;

    [SerializeField]
    float jumpTime;

    [Space(10)]

    [Header("Dodge")]

    [Space(5)]

    [SerializeField]
    float dodgeCost;

    [SerializeField]
    float dodgeCooldown;

    [SerializeField]
    float dodgeLength;

    [SerializeField]
    float dodgeSpeed;

    [SerializeField]
    float strafeDodgeSpeed;

    [Space(10)]

    [Header("Falling")]

    [Space(5)]

    [SerializeField]
    float safeFallDistance;

    [Space(10)]

    [Header("Audio")]

    [Space(5)]

    [SerializeField]
    AudioClip sandSteps;

    [SerializeField]
    AudioClip stoneSteps;

    [SerializeField]
    AudioClip woodSteps;

    [SerializeField]
    AudioClip landingSand;

    [SerializeField]
    AudioClip landingStone;

    [SerializeField]
    AudioClip landingWood;

    [SerializeField]
    float footStepsVolume;

    [SerializeField]
    float landingVolume;

    [SerializeField]
    AudioSource footsteps;

    [Space(10)]

    [Header("UI")]

    [Space(5)]

    [SerializeField]
    Slider staminaBar;

    [Space(10)]

    [Header("Physics Material")]

    [Space(5)]

    [SerializeField]
    PhysicMaterial slipperyMaterial;

    [SerializeField]
    PhysicMaterial frictionMaterial;

    [SerializeField]
    PhysicMaterial maxFrictionMaterial;

    #endregion

    #region Non-Serialized Variables

    private Rigidbody rb;

    private Animator anim;

    private float stamina, delta, h, v, moveAmount, direction, groundDistance = 0.2f, staminaRegenCountdown, freezeTime, dodgePower;

    private Transform cam;

    private Vector3 camForward, moveDir;

    private CameraFollow camFollow;

    private delegate void Movement();       //Delegatmetod som kontrollerar hur spelaren rör sig beroende på om kameran låsts på en fiende eller ej

    private delegate void JumpType(bool superJump);

    private bool paused = false, isGrounded, jumping = false, superJump = false, jump = false, interacting = false, isSprinting = false, canJump = true, climbing = false, landed = false, frozen = false, dodging = false;

    private Movement currentMovement;

    private Movement previousMovement;

    private JumpType currentJump;

    private LayerMask ignoreLayers;

    private PlayerCombat combat;

    private PlayerInteractions interactions;

    private string currentMovementType = "Default";

    private Collider playerCollider;

    private RaycastHit groundHit;

    private Vector3? dashVelocity, dodgeVelocity;

    private ConsoleScript console;

    #endregion

    #region Properties

    public string CurrentMovementType
    {
        get { return this.currentMovementType; }
    }

    public GameObject CameraFollowObj
    {
        get { return this.cameraFollowObj; }
    }

    public float Stamina
    {
        get { return this.stamina; }
        set { this.stamina = Mathf.Clamp(value, 0f, maxStamina); staminaBar.value = stamina; }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
        set { IsGrounded = value; }
    }

    public bool Interacting
    {
        set { this.interacting = value; }
    }

    public Animator Anim
    {
        get { return this.anim; }
    }

    #endregion

    #region Main Methods

    void Awake()
    {
        console = FindObjectOfType<ConsoleScript>();
        playerCollider = GetComponent<Collider>();
        interactions = GetComponent<PlayerInteractions>();
        combat = GetComponent<PlayerCombat>();
        currentMovement = DefaultMovement;
        previousMovement = DefaultMovement;
        currentJump = Jump;
        this.stamina = maxStamina;
        rb = GetComponent<Rigidbody>();
        staminaBar = GameObject.Find("StaminaSlider").GetComponent<Slider>();
        staminaBar.maxValue = maxStamina;
        staminaBar.value = stamina;
        FindObjectOfType<PauseManager>().Pausables.Add(this);
        anim = GetComponent<Animator>();
        ignoreLayers = ~(1 << 5);
    }

    void Update()
    {
        if (!paused)
        {
            GroundCheck(Time.deltaTime);
            GetInput();
        }
    }

    void FixedUpdate()
    {
        if (!paused && !interacting)
        {
            currentMovement();
        }
    }

    public void GetInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && canJump)
        {
            jump = true;
            //canJump = false;
        }

        if (Input.GetButtonDown("Dodge") && isGrounded && !Sliding() && !jump && OnGround())
            StartCoroutine("Dodge");

        if (Input.GetButton("Sprint") && stamina >= staminaSprintDrain && isGrounded && !Sliding())
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
            if (console != null && console.Sprint != null)
                console.StopDejaVu();
        }
    }

    #endregion

    #region Public Methods

    public void SetCam(Transform camTransform, CameraFollow camFollow)
    {
        this.cam = camTransform;
        this.camFollow = camFollow;
    }

    public void ChangeJump(string newJump)
    {
        switch (newJump)
        {
            case "Jump":
                currentJump = Jump;
                break;

            case "Climb":
                currentJump = Climb;
                break;
        }
    }

    public void ChangeMovement(string movementType)
    {
        dashVelocity = null;
        dodgeVelocity = null;
        if (combat.Dead)
        {
            currentMovement = NoMovement;
            return;
        }
        switch (movementType)
        {
            case "None":
                currentMovement = NoMovement;
                break;

            case "Previous":
                currentMovement = previousMovement;
                ChangeMovement(currentMovement == DefaultMovement ? "Default" : "LockOn");
                break;

            case "Default":
                currentMovementType = "Default";
                currentMovement = DefaultMovement;
                previousMovement = currentMovement;
                anim.SetLayerWeight(2, 0);
                moveSpeed = 10f;
                break;

            case "LockOn":
                currentMovementType = "LockOn";
                currentMovement = LockOnMovement;
                previousMovement = currentMovement;
                anim.SetLayerWeight(2, 1);
                moveSpeed = 5f;
                break;

            case "Dash":
                if (currentMovement == LockOnMovement || currentMovement == DefaultMovement)
                {
                    anim.SetTrigger("Dash");
                    currentMovementType = "Dash";
                    previousMovement = currentMovement;
                    currentMovement = DashMovement;
                }
                break;

            case "Dodge":
                if (currentMovement == LockOnMovement || currentMovement == DefaultMovement)
                {
                    anim.SetTrigger("Dodge");
                    currentMovementType = "Dodge";
                    previousMovement = currentMovement;
                    currentMovement = DodgeMovement;
                }
                break;

            case "StrafeDodge":
                if (currentMovement == LockOnMovement)
                {
                    anim.SetTrigger("Dodge");
                    currentMovementType = "StrafeDodge";
                    previousMovement = currentMovement;
                    currentMovement = StrafeDodgeMovement;
                }
                break;
        }
    }

    public void PauseMe(bool pausing)
    {
        paused = pausing;
    }

    #endregion

    //Olika metoder som främst används via delegatmetoden currentMovement för att röra spelaren på olika sätt
    #region Movement Methods

    void DefaultMovement()          //Den metod som används för att röra spelaren när denne inte låst kameran på en fiende
    {
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1).normalized);

        Vector3 vertical = v * camForward;
        Vector3 horizontal = h * cam.right;

        moveDir = (vertical + horizontal).normalized;

        float m = Mathf.Abs(v) + Mathf.Abs(h);

        moveAmount = (Mathf.Clamp01(m));

        float speed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

        anim.SetFloat("Speed", speed);

        MovePlayer(moveSpeed);

        Vector3 targetDir = moveDir;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveAmount * rotspeed);
        transform.rotation = targetRotation;

        if (jump && currentMovement != DashMovement && currentMovement != DodgeMovement && !dodging)
            currentJump(superJump);
        jump = false;
    }

    void DodgeMovement()        //Metod som används för att få spelaren att göra en rull-dodge
    {
        dodgePower = dodgeSpeed;
        if (dodgeVelocity == null)
        {
            dodgeVelocity = moveDir;
            if (dodgeVelocity == Vector3.zero)
                dodgeVelocity = rb.transform.forward * 4;
        }
        rb.AddForce((Vector3)dodgeVelocity * dodgePower, ForceMode.Impulse);
    }

    void StrafeDodgeMovement()
    {
        if (anim.GetFloat("SpeedZ") <= 0)
        {
            dodgePower = strafeDodgeSpeed;
            rb.velocity += moveDir * dodgePower;
        }
        else
        {
            dodgePower = strafeDodgeSpeed;
            rb.velocity += (moveDir/2) * dodgePower;
        }
    }

    void DashMovement()         //Metod som används för att få spelaren att göra en dash
    {
        rb.velocity = transform.forward * moveSpeed * 3;
    }

    void NoMovement()
    {
        return;
    }

    void LockOnMovement()          //Den metod som används för att röra spelaren när denne låst kameran på en fiende
    {
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1).normalized);

        Vector3 vertical = v * camForward;
        Vector3 horizontal = h * cam.right;

        moveDir = (vertical + horizontal).normalized;

        float _moveAmount = Mathf.Clamp(v, -1f, 1f);
        float _direction = Mathf.Clamp(h, -1f, 1f);
        moveAmount = _moveAmount;
        direction = _direction;

        MovePlayer(moveSpeed);

        anim.SetFloat("SpeedX", direction);
        anim.SetFloat("SpeedZ", moveAmount);

        transform.LookAt(camFollow.LookAtMe.transform);
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
    }

    void Climb(bool interfaceRequirement)
    {
        ClimbableScript currentClimbable = interactions.CurrentInteractable as ClimbableScript;
        transform.LookAt(currentClimbable.FinalClimbingPosition);
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
        StartCoroutine(Climbing(currentClimbable.SuperClimb));
    }

    public void Jump(bool superJump)
    {
        if (!isGrounded || !OnGround() || currentMovement == DashMovement || currentMovement == DodgeMovement || dodging)
            return;

        jumping = true;

        if (!superJump)
            anim.SetTrigger("Jump");

        landed = false;
        Vector3 vel = rb.velocity;
        canJump = false;
        vel.y = superJump ? superJumpForce : jumpForce;
        rb.velocity = vel;
        superJump = false;
    }

    public void MovePlayer(float velocity)
    {
        velocity = StaminaHandler(velocity);

        Vector3 velY = transform.forward * velocity * moveAmount;
        velY.y = rb.velocity.y;
        Vector3 velX = transform.right * velocity * direction;
        velX.x = rb.velocity.x;

        rb.drag = (moveAmount > 0 || !isGrounded || jumping) ? 0 : 4;

        if (frozen)
        {
            moveDir /= 2;
        }


        if (!combat.Attacking)
        {
            if (camFollow.LockOn && !(anim.GetCurrentAnimatorStateInfo(0).IsName("StrafeDodge") || anim.GetCurrentAnimatorStateInfo(0).IsName("DodgeBackwards") 
                || anim.GetCurrentAnimatorStateInfo(0).IsName("DodgeForward")))
            {
                Vector3 strafeVelocity = (transform.TransformDirection((new Vector3(h, 0, v)) * (velocity > 0 ? velocity : 1f)));
                strafeVelocity.y = rb.velocity.y;
                rb.velocity = Vector3.Lerp(rb.velocity, strafeVelocity, 20f * Time.deltaTime);
            }
            else
            {
                if ((isGrounded || airControl) && !Sliding() && !(anim.GetCurrentAnimatorStateInfo(0).IsName("StrafeDodge") || anim.GetCurrentAnimatorStateInfo(0).IsName("DodgeBackwards")
                || anim.GetCurrentAnimatorStateInfo(0).IsName("DodgeForward")))
                {
                    rb.velocity = velY;
                    rb.AddForce(moveDir * (velocity * moveAmount) * Time.deltaTime, ForceMode.VelocityChange);
                }
            }
        }
    }

    float StaminaHandler(float velocity)
    {
        if (isSprinting && !camFollow.LockOn)
        {
            velocity = sprintSpeed;
            staminaRegenCountdown = staminaRegenWait;
            stamina -= staminaSprintDrain * Time.deltaTime;
            staminaBar.value = stamina;
            if (console != null && console.Sprint != null)
                console.Sprint.Invoke();
        }
        else
        {
            velocity = moveSpeed;

            if (staminaRegenCountdown > 0)
            {
                staminaRegenCountdown -= Time.deltaTime;
            }
            if (staminaRegenCountdown <= 0)
            {
                stamina = Mathf.Clamp(stamina + staminaRegen, 0f, maxStamina);
                staminaBar.value = stamina;
            }
        }
        return velocity;
    }

    #endregion

    #region Ground Checks

    public void GroundCheck(float d)
    {
        delta = d;
        bool inAir = jumping;
        anim.SetBool("Falling", inAir);
        isGrounded = OnGround();
        jumping = !isGrounded;

        if (!jumping && inAir && rb.velocity.y < -safeFallDistance)
        {
            anim.SetTrigger("Landing");

            if (rb.velocity.y < 0f && rb.velocity.y + safeFallDistance < 0f)
                combat.TakeDamage((int)-(rb.velocity.y + safeFallDistance), DamageType.Falling);      //Fallskada
        }

        if (inAir)
        {
            landed = false;
        }

        if (!Sliding() && !inAir && moveDir != Vector3.zero)
            playerCollider.material = frictionMaterial;
        else if (!Sliding() && !inAir && moveDir == Vector3.zero)
            playerCollider.material = maxFrictionMaterial;
        else
            playerCollider.material = slipperyMaterial;

        if (!inAir && !landed)
        {
            StartCoroutine("JumpCooldown");
        }
    }

    public bool OnGround()
    {
        Vector3 origin = transform.position + (Vector3.up * groundDistance);
        Vector3 dir = Vector3.down;
        float dis = groundDistance + 0.5f;

        if (Physics.SphereCast(origin, ((CapsuleCollider)playerCollider).radius - 0.1f, dir, out groundHit, dis, ignoreLayers))
        {
            return true;
        }
        return false;
    }

    public bool Sliding()
    {
        if (GroundAngle() > slopeLimit)
        {
            isGrounded = false;
            //float slideVelocity = (GroundAngle() - slopeLimit) * 2f;
            //slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
            //rb.velocity = new Vector3(rb.velocity.x, -slideVelocity, rb.velocity.z);

            return true;
        }
        else
        {
            isGrounded = true;
            return false;
        }
    }

    float GroundAngle()
    {
        float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        return Mathf.Abs(groundAngle);
    }

    #endregion

    #region Coroutines

    public IEnumerator Freeze()
    {
        frozen = true;
        yield return new WaitForSeconds(freezeTime);
        frozen = false;
    }

    IEnumerator Dodge()
    {
        canJump = false;
        dodging = true;
        if (currentMovementType == "Default")
            ChangeMovement("Dodge");
        else
            anim.SetTrigger("Dodge");
            //ChangeMovement("StrafeDodge");

        yield return new WaitForSeconds(dodgeLength);
        dodgeVelocity = null;
        canJump = true;
        dodging = false;
        ChangeMovement("Previous");
    }

    IEnumerator JumpCooldown()
    {
        landed = true;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    IEnumerator Climbing(bool superClimb)
    {
        climbing = true;
        ChangeMovement("None");
        string climbType = superClimb ? "Climb2" : "Climb1";
        float climbTime = superClimb ? 2.4f : 2f;
        float defaultMass = rb.mass;
        rb.useGravity = false;
        anim.SetTrigger(climbType);
        yield return new WaitForSeconds(climbTime);
        climbing = false;
        rb.useGravity = true;
        ChangeJump("Jump");
        ChangeMovement("Previous");
    }

    #endregion
}