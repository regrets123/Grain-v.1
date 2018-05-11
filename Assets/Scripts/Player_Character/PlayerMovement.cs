using System.Collections;
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

    private float stamina, delta, h, v, moveAmount, groundDistance = 0.2f;

    private Transform cam;

    private Vector3 camForward, moveDir;

    private CameraFollow camFollow;

    private delegate void Movement();       //Delegatmetod som kontrollerar hur spelaren rör sig beroende på om kameran låsts på en fiende eller ej

    private bool paused = false, isGrounded, jumping = false, superJump = false, jump = false, interacting = false;

    private Movement currentMovement;

    private Movement previousMovement;

    private LayerMask ignoreLayers;

    private PlayerCombat combat;

    private string currentMovementType = "Default";

    private Collider playerCollider;

    private RaycastHit groundHit;

    //private Vector3? dashDir, dodgeDir;

    private Vector3? dashVelocity, dodgeVelocity;

    #endregion

    #region Properties

    public string CurrentMovementType
    {
        get { return this.currentMovementType; }
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

    #endregion

    #region Main Methods

    void Awake()
    {
        playerCollider = GetComponent<Collider>();
        combat = GetComponent<PlayerCombat>();
        currentMovement = DefaultMovement;
        previousMovement = DefaultMovement;
        this.stamina = maxStamina;
        cam = FindObjectOfType<Camera>().transform;
        rb = GetComponent<Rigidbody>();
        //staminaBar = GameObject.Find("StaminaSlider").GetComponent<Slider>();
        //staminaBar.maxValue = maxStamina;
        //staminaBar.value = stamina;
        FindObjectOfType<PauseManager>().Pausables.Add(this);
        anim = GetComponent<Animator>();
        camFollow = FindObjectOfType<CameraFollow>();
        ignoreLayers = ~(1 << 5);
    }

    void Update()
    {
        GroundCheck(Time.deltaTime);

        if (!paused && !interacting)
        {
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
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        else if (Input.GetButtonDown("Dodge"))
            StartCoroutine("Dodge");
    }

    #endregion

    #region Public Methods

    public void ChangeMovement(string movementType)
    {
        dashVelocity = null;
        dodgeVelocity = null;
        switch (movementType)
        {
            case "Previous":
                currentMovement = previousMovement;
                currentMovementType = currentMovement == DefaultMovement ? "Default" : "LockOn";
                break;

            case "Default":
                currentMovementType = "Default";
                currentMovement = DefaultMovement;
                previousMovement = currentMovement;
                break;

            case "LockOn":
                currentMovementType = "LockOn";
                currentMovement = LockOnMovement;
                previousMovement = currentMovement;
                break;

            case "Dash":
                if (currentMovement == LockOnMovement || currentMovement == DefaultMovement)
                {
                    currentMovementType = "Dash";
                    previousMovement = currentMovement;
                    currentMovement = DashMovement;
                }
                break;

            case "Dodge":
                if (currentMovement == LockOnMovement || currentMovement == DefaultMovement)
                {
                    currentMovementType = "Dodge";
                    previousMovement = currentMovement;
                    currentMovement = DodgeMovement;
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

        anim.SetFloat("Speed", moveAmount);

        MovePlayer(moveSpeed);
        if (jump)
            Jump(superJump);
        jump = false;
    }

    void DodgeMovement()
    {
        if (dodgeVelocity == null)
            dodgeVelocity = (transform.forward + new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * dodgeSpeed);
        rb.velocity = (Vector3)dodgeVelocity;
    }

    void DashMovement()
    {
        rb.velocity = transform.forward * moveSpeed * 3;
    }

    void LockOnMovement()          //Den metod som används för att röra spelaren när denne låst kameran på en fiende
    {

    }

    void Jump(bool superJump)
    {
        if (!isGrounded)
            return;

        jumping = true;
        anim.SetTrigger("Jump");
        Vector3 vel = rb.velocity;
        vel.y = superJump ? superJumpForce : jumpForce;
        rb.velocity = vel;
    }

    public void MovePlayer(float velocity)
    {
        Vector3 velY = transform.forward * velocity * moveAmount;
        velY.y = rb.velocity.y;

        rb.drag = (moveAmount > 0 || !isGrounded || jumping) ? 0 : 4;

        if ((isGrounded || airControl) && !Sliding())
        {
            rb.velocity = velY;
            rb.AddForce(moveDir * (velocity * moveAmount) * Time.deltaTime, ForceMode.VelocityChange);
        }

        Vector3 targetDir = moveDir;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveAmount * rotspeed);
        transform.rotation = targetRotation;

        //rb.velocity = new Vector3(moveDir.x * (moveSpeed * moveAmount * delta), rb.velocity.y, moveDir.z * (moveSpeed * moveAmount * delta));
    }

    #endregion

    #region Ground Checks

    public void GroundCheck(float d)
    {
        delta = d;
        bool inAir = jumping;
        isGrounded = OnGround();
        jumping = !isGrounded;

        if (!jumping && inAir && rb.velocity.y < -safeFallDistance)
        {
            //print(Math.Round(rb.velocity.y, 5));
            if (rb.velocity.y < 0f && rb.velocity.y + safeFallDistance < 0f)
                combat.TakeDamage((int)-(rb.velocity.y + safeFallDistance), DamageType.Falling);      //Fallskada
        }

        if (!Sliding() && !inAir && moveDir != Vector3.zero)
            playerCollider.material = frictionMaterial;
        else if (!Sliding() && !inAir && moveDir == Vector3.zero)
            playerCollider.material = maxFrictionMaterial;
        else
            playerCollider.material = slipperyMaterial;
    }

    public bool OnGround()
    {
        Vector3 origin = transform.position + (Vector3.up * groundDistance);
        Vector3 dir = Vector3.down;
        float dis = groundDistance + 0.3f;

        if (Physics.Raycast(origin, dir, out groundHit, dis, ignoreLayers))
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
            float slideVelocity = (GroundAngle() - slopeLimit) * 2f;
            slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
            rb.velocity = new Vector3(rb.velocity.x, -slideVelocity, rb.velocity.z);

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

    IEnumerator Dodge()
    {
        ChangeMovement("Dodge");
        yield return new WaitForSeconds(dodgeLength);
        dodgeVelocity = null;
        ChangeMovement("Previous");
    }

    IEnumerator JumpEnumerator(float verticalSpeed)
    {
        float startTime = Time.time;
        float force = verticalSpeed;
        while (Time.time < startTime + jumpTime)
        {
            transform.Translate(Vector3.up * force);
            force -= Time.deltaTime;
            Vector3 origin = transform.position + (Vector3.up * groundDistance);
            RaycastHit hit;
            float dis = groundDistance + 0.2f;
            if (force < 0f || Physics.Raycast(origin, Vector3.up, out hit, dis, ignoreLayers))
                break;
            yield return new WaitForFixedUpdate();
        }
    }

    #endregion
}

