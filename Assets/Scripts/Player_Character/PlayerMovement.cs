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

    private bool paused = false, onGround;

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

    void FixedUpdate()
    {
        if (!paused)
        {
            GetInput();
            currentMovement();
            Tick(Time.fixedDeltaTime);
            Tock(Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        //Tock(Time.deltaTime);
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

    float maxX = 10f, maxZ = 10f;

    void DefaultMovement()          //Den metod som används för att röra spelaren när denne inte låst kameran på en fiende
    {
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1).normalized);

        Vector3 vertical = v * camForward;
        Vector3 horizontal = h * cam.right;

        moveDir = (vertical + horizontal).normalized;

        float m = Mathf.Abs(v) + Mathf.Abs(h);

        moveAmount = (Mathf.Clamp01(m));

        anim.SetFloat("Speed", moveAmount);

        //Vector3 move = h * cam.right + v * camForward;
        //move *= moveSpeed;
        //rb.velocity = new Vector3(Mathf.Clamp(move.x * moveSpeed, -maxX, maxX), rb.velocity.y, Mathf.Clamp(move.z * moveSpeed, -maxX, maxX));
        //rb.AddForce((move * moveSpeed) * Time.deltaTime, ForceMode.Force);
        //rb.AddForce(move * moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
        //rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxX, maxX), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxZ, maxZ));
        //    anim.SetFloat("Speed", move.magnitude);
        //    if (move.magnitude > 0.01f)
        //    {
        //        rb.rotation = Quaternion.LookRotation(move);
        //    }
        //    if (Input.GetButtonDown("Jump"))
        //    {
        //        Jump(false);
        //    }
        if (onGround && Input.GetButtonDown("Jump"))
        {
            Jump(false);
        }
    }

    void LockOnMovement()          //Den metod som används för att röra spelaren när denne låst kameran på en fiende
    {

    }

    void Jump(bool superJump)
    {
        anim.SetTrigger("Jump");
        float verticalSpeed = superJump ? jumpSpeed * 3f : jumpSpeed;
        rb.drag = 0;
        rb.AddForce(Vector3.up * verticalSpeed/* * Time.deltaTime*/, ForceMode.Impulse);
        //rb.velocity = jumpSpeed * Vector3.up;
    }

    #endregion

    public void GetInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }

    public void Tick(float d)
    {
        delta = d;

        rb.drag = (moveAmount > 0 || !onGround) ? 0 : 999;

        if (onGround)
        {
            rb.velocity = moveDir * (moveSpeed * moveAmount * delta);
        }

        Vector3 targetDir = moveDir;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotspeed);
        transform.rotation = targetRotation;
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

    public void Tock(float d)
    {
        delta = d;

        onGround = OnGround();
    }
}
