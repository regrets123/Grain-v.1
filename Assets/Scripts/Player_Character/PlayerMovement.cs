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
    int rotspeed;

    [SerializeField]
    float sprintSpeed;

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
    float jumpSpeed;

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

    private float stamina, delta, h, v, moveAmount, groundDistance = 0.6f;

    private Transform cam;

    private Vector3 camForward, moveDir;

    private CameraFollow camFollow;

    private delegate void Movement();       //Delegatmetod som kontrollerar hur spelaren rör sig beroende på om kameran låsts på en fiende eller ej

    private bool paused = false, isGrounded, jumping = false, superJump = false;

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
        Tock(Time.deltaTime);
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
    }

    #endregion

    #region Public Methods

    public void ChangeMovement(bool combat)
    {
        if (combat)
            currentMovement = LockOnMovement;
        else
            currentMovement = DefaultMovement;
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

        Tick(moveSpeed);
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

    void LockOnMovement()          //Den metod som används för att röra spelaren när denne låst kameran på en fiende
    {

    }

    void Jump(bool superJump)
    {
        //rb.drag = 0f;
        //jumping = true;
        //anim.SetTrigger("Jump");

        //float verticalSpeed = superJump ? jumpSpeed * 3f : jumpSpeed;
        ////rb.drag = 0;
        ////rb.AddForce(Vector3.up * verticalSpeed * Time.deltaTime, ForceMode.VelocityChange);
        ////rb.velocity += verticalSpeed * Vector3.up;
        ////moveDir.y += verticalSpeed * Time.deltaTime;
        //StartCoroutine(JumpEnumerator(verticalSpeed));

        Vector3 vel = rb.velocity;
        vel.y = jumpSpeed;
        rb.velocity = vel;
    }

    #endregion

    public void Tick(float velocity)
    {
        Vector3 velY = transform.forward * velocity * moveAmount;
        velY.y = rb.velocity.y;

        rb.drag = (moveAmount > 0 || !isGrounded || jumping) ? 0 : 4;

        //if (onGround)
        {
            rb.velocity = velY;
            rb.AddForce(moveDir * (velocity * moveAmount) * Time.deltaTime, ForceMode.VelocityChange);
            //rb.velocity = new Vector3(moveDir.x * (moveSpeed * moveAmount * delta), rb.velocity.y, moveDir.z * (moveSpeed * moveAmount * delta));
        }

        Vector3 targetDir = moveDir;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveAmount * rotspeed);
        transform.rotation = targetRotation;

        print(targetDir);
    }

    public void Tock(float d)
    {
        delta = d;

        isGrounded = OnGround();
        jumping = !isGrounded;
    }

    public bool OnGround()
    {
        bool r = false;

        Vector3 origin = transform.position + (Vector3.up * groundDistance);
        Vector3 dir = Vector3.down;
        float dis = groundDistance + 0.4f;

        RaycastHit hit;

        if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
        {
            r = true;

            Vector3 targetPos = hit.point;
            transform.position = targetPos;
        }

        return r;
    }
}
