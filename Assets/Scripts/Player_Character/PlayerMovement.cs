using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson && Andreas Nilsson*/

public class PlayerMovement : MonoBehaviour, IPausable
{
    #region Serialized Variables

    [Header("Main Movement")]

    [Space(5)]

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float sprintSpeed;

    [SerializeField]
    int rotspeed;

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

    #endregion

    #region Non-Serialized Variables

    private Rigidbody rb;

    private MovementType currentMovementType = MovementType.Idle;

    private Animator anim;

    private float stamina, delta, h, v, moveAmount, direction, groundDistance = 0.2f;

    private Transform cam;

    private Vector3 camForward, moveDir;

    private CameraFollow camFollow;

    private delegate void Movement();       //Delegatmetod som kontrollerar hur spelaren rör sig beroende på om kameran låsts på en fiende eller ej

    private bool paused = false, isGrounded, jumping = false, superJump = false, isSprinting = false;

    Movement currentMovement;

    LayerMask ignoreLayers;

    #endregion

    #region Properties

    public MovementType CurrentMovementType
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

    #endregion

    #region Main Methods

    void Awake()
    {
        currentMovement = DefaultMovement;
        this.stamina = maxStamina;
        cam = FindObjectOfType<Camera>().transform;
        rb = GetComponent<Rigidbody>();
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

        if (!paused)
        {
            GetInput();
        }
    }

    void FixedUpdate()
    {
        if (!paused)
        {
            currentMovement();

        }
    }

    void LateUpdate()
    {
        //currentMovement();

        if (Input.GetButtonDown("Jump"))
        {
            Jump(superJump);
        }
    }

    public void GetInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (Input.GetButton("Sprint"))
            isSprinting = true;
        else
            isSprinting = false;
    }

    #endregion

    #region Public Methods

    public void ChangeMovement(bool combat)
    {
        if (combat)
        {
            currentMovement = LockOnMovement;
            anim.SetLayerWeight(2, 1);
            moveSpeed = 5f;
        }
        else
        {
            currentMovement = DefaultMovement;
            anim.SetLayerWeight(2, 0);
            moveSpeed = 10f;
        }

        print(currentMovement.Method);
    }

    public void PauseMe(bool pausing)
    {
        paused = pausing;
    }

    #endregion

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

        MovePlayer(moveSpeed, isSprinting);

        Vector3 targetDir = moveDir;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveAmount * rotspeed);
        transform.rotation = targetRotation;
    }

    void LockOnMovement()
    {
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1).normalized);

        Vector3 vertical = v * camForward;
        Vector3 horizontal = h * cam.right;

        moveDir = (vertical + horizontal).normalized;

        float _moveAmount = Mathf.Clamp(v, -1f, 1f);
        float _direction = Mathf.Clamp(h, -1f, 1f);
        moveAmount = _moveAmount;
        direction = _direction;

        MovePlayer(moveSpeed, isSprinting);

        anim.SetFloat("SpeedX", direction);
        anim.SetFloat("SpeedZ", moveAmount);

        transform.LookAt(camFollow.LookAtMe.transform);
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);

        //Vector3 targetDir = camForward;

        //if (targetDir == Vector3.zero)
        //{
        //    targetDir = transform.forward;
        //}

        //Quaternion tr = Quaternion.LookRotation(targetDir);
        //Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveAmount * rotspeed);
        //transform.rotation = targetRotation;
    }

    //IEnumerator JumpEnumerator(float verticalSpeed)
    //{
    //    float startTime = Time.time;
    //    float force = verticalSpeed;
    //    while (Time.time < startTime + jumpTime)
    //    {
    //        transform.Translate(Vector3.up * force);
    //        force -= Time.deltaTime;
    //        Vector3 origin = transform.position + (Vector3.up * groundDistance);
    //        RaycastHit hit;
    //        float dis = groundDistance + 0.2f;
    //        if (force < 0f || Physics.Raycast(origin, Vector3.up, out hit, dis, ignoreLayers))
    //            break;
    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    //void LockOnMovement()          //Den metod som används för att röra spelaren när denne låst kameran på en fiende
    //{

    //}

    void Jump(bool superJump)
    {
        //rb.drag = 0f;

        //float verticalSpeed = superJump ? jumpSpeed * 3f : jumpSpeed;
        ////rb.drag = 0;
        ////rb.AddForce(Vector3.up * verticalSpeed * Time.deltaTime, ForceMode.VelocityChange);
        ////rb.velocity += verticalSpeed * Vector3.up;
        ////moveDir.y += verticalSpeed * Time.deltaTime;
        //StartCoroutine(JumpEnumerator(verticalSpeed));

        if (!isGrounded)
            return;

        jumping = true;
        anim.SetTrigger("Jump");
        Vector3 vel = rb.velocity;
        vel.y = superJump ? superJumpForce : jumpForce;
        rb.velocity = vel;
    }

    #endregion

    public void MovePlayer(float velocity, bool isSprinting)
    {
        if (isSprinting && !camFollow.LockOn)
            velocity = sprintSpeed;
        else
            velocity = moveSpeed;

        Vector3 velY = transform.forward * velocity * moveAmount;
        velY.y = rb.velocity.y;

        Vector3 velX = transform.right * velocity * direction;
        velX.x = rb.velocity.x;

        rb.drag = (moveAmount > 0 || !isGrounded || jumping) ? 0 : 4;

        if (isGrounded)
        {
            if (camFollow.LockOn)
            {
                Vector3 strafeVelocity = (transform.TransformDirection((new Vector3(h, 0, v)) * (velocity > 0 ? velocity : 1f)));
                strafeVelocity.y = rb.velocity.y;
                rb.velocity = Vector3.Lerp(rb.velocity, strafeVelocity, 20f * Time.deltaTime);
            }
            else
            {
                rb.velocity = velY;
                rb.AddForce(moveDir * (velocity * moveAmount) * Time.deltaTime, ForceMode.VelocityChange);
                //rb.velocity = new Vector3(moveDir.x * (moveSpeed * moveAmount * delta), rb.velocity.y, moveDir.z * (moveSpeed * moveAmount * delta));
            }
        }
    }

    public void GroundCheck(float d)
    {
        delta = d;

        isGrounded = OnGround();
        jumping = !isGrounded;
    }

    public bool OnGround()
    {
        Vector3 origin = transform.position + (Vector3.up * groundDistance);
        Vector3 dir = Vector3.down;
        float dis = groundDistance + 0.1f;

        RaycastHit hit;

        if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
        {
            return true;
        }

        return false;
    }


}
