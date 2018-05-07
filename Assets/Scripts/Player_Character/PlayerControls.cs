using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Andreas Nilsson && Björn Andersson*/

public interface IKillable          //Interface som används av spelaren och alla fiender samt eventuella förstörbara objekt
{
    void LightAttack();
    void HeavyAttack();
    void TakeDamage(int damage, DamageType dmgType);
    void Kill();
}

public enum MovementType            //Håller koll på hur spelaren rör sig för tillfället för att tillåta och hindra spelaren att göra vissa saker
{
    Idle, Walking, Sprinting, Attacking, Dodging, Dashing, Jumping, Running, Interacting, SuperJumping, Stagger
}

public class PlayerControls : MonoBehaviour, IKillable, IPausable
{
    #region Serialized Variables

    [Header("Player Stats")]

    [Space(5)]

    [SerializeField]
    int maxHealth;

    [SerializeField]
    float maxStamina;

    [SerializeField]
    int leechAmount;

    [SerializeField]
    float maxLifeForce;

    [SerializeField]
    float maxPoise;

    [SerializeField]
    float staminaRegen;

    [SerializeField]
    float staminaRegenWait;

    [SerializeField]
    float invulnerablityTime;

    [SerializeField]
    float staggerTime;

    [SerializeField]
    float poiseCooldown;

    [SerializeField]
    int armor;

    [SerializeField]
    int leechPercentage;

    [Space(10)]

    [Header("Player Movement")]

    [Space(5)]

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float sprintSpeed;

    [SerializeField]
    float jumpSpeed;

    [SerializeField]
    float jumpCooldown;

    [SerializeField]
    int rotspeed;

    [Space(10)]

    [Header("Player Actions")]

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
    float attackMoveLength;

    [SerializeField]
    float attackCooldown;

    [SerializeField]
    float abilityCooldown;

    [Space(10)]

    [Header("Player Physics")]

    [Space(5)]

    [SerializeField]
    float gravity;

    [SerializeField]
    float slopeLimit;

    [SerializeField]
    float slideFriction;

    [SerializeField]
    float safeFallDistance;

    [SerializeField]
    Transform playerNewGamePos;

    [Space(10)]

    [Header("Player Sounds")]

    [Space(5)]

    [SerializeField]
    AudioClip swordSheathe;

    [SerializeField]
    AudioClip swordUnsheathe;

    [SerializeField]
    AudioClip lightAttack1;

    [SerializeField]
    AudioClip lightAttack2;

    [SerializeField]
    AudioClip lightAttack3;

    [SerializeField]
    AudioClip heavyAttack1;

    [SerializeField]
    AudioClip heavyAttack2;

    [SerializeField]
    AudioClip sandSteps;

    [SerializeField]
    AudioClip stoneSteps;

    [SerializeField]
    AudioClip woodSteps;

    [SerializeField]
    AudioSource rightFoot;

    [SerializeField]
    AudioSource leftFoot;

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

    [Space(10)]

    [Header("Player Items")]

    [Space(5)]

    [SerializeField]
    Transform weaponPosition;

    [SerializeField]
    SpriteRenderer currentRune;

    [Space(10)]

    [Header("Player Canvas")]

    [Space(5)]

    [SerializeField]
    Slider healthBar;

    [SerializeField]
    Slider staminaBar;

    [SerializeField]
    Slider lifeForceBar;

    [SerializeField]
    GameObject aggroIndicator;

    [SerializeField]
    GameObject deathScreen;

    #endregion

    #region Non-Serialized Variables
    CharacterController charController;

    Vector3 move, dashVelocity, dodgeVelocity, hitNormal;

    Vector3? dashDir, dodgeDir;

    MovementType currentMovementType, previousMovementType;

    PauseManager pM;

    InventoryManager inventory;

    IInteractable currentInteractable;

    InputManager iM;

    BaseWeaponScript currentWeapon;

    CameraFollow cameraFollow;

    GameObject weaponToEquip, lastEquippedWeapon;

    ClimbableScript currentClimbable;

    Animator anim;

    float yVelocity, stamina, h, v, secondsUntilResetClick, attackCountdown = 0f, interactTime, dashedTime, poiseReset, poise, timeToBurn = 0f, charSpeed;

    int health, lifeForce = 0, nuOfClicks = 0, abilityNo = 0;

    private Transform cam;

    private Vector3 camForward;

    List<BaseEnemyScript> enemiesAggroing = new List<BaseEnemyScript>();

    List<DamageType> resistances = new List<DamageType>();

    BaseAbilityScript currentAbility;

    bool inputEnabled = true, jumpMomentum = false, grounded, invulnerable = false, canDodge = true, dead = false, canSheathe = true, burning = false, frozen = false, wasGrounded,
        combatStance = false, attacked = false, climbing = false, staminaRegenerating = false, staminaRegWait = false, canJump = true, fallInvulerability = false, noGravity = true;    //bool shitManyBools = trueAF;
    #endregion

    #region Properties
    //Describes which kind of movement that is currently being used
    public MovementType CurrentMovementType
    {
        get { return this.currentMovementType; }
        set { this.currentMovementType = value; }
    }

    public ClimbableScript CurrentClimbable
    {
        set { this.currentClimbable = value; }
    }

    public InventoryManager Inventory
    {
        get { return this.inventory; }
    }

    public BaseAbilityScript CurrentAbility
    {
        get { return this.currentAbility; }
    }

    public GameObject LastEquippedWeapon
    {
        get { return this.lastEquippedWeapon; }
    }

    public Slider StaminaBar
    {
        get { return this.staminaBar; }
    }

    public Slider LifeforceBar
    {
        get { return lifeForceBar; }
        set { lifeForceBar = LifeforceBar; }
    }

    public Slider HealthBar
    {
        get { return healthBar; }
        set { healthBar = HealthBar; }
    }

    //Gets and sets the current weapon
    public BaseWeaponScript CurrentWeapon
    {
        get { return this.currentWeapon; }
    }

    public float Stamina
    {
        get { return this.stamina; }
        set { this.stamina = Mathf.Clamp(value, 0f, maxStamina); staminaBar.value = stamina; }
    }

    public int Health
    {
        get { return this.health; }
        set { this.health = value; if (this.health <= 0f) { Death(); } }
    }

    public int LifeForce
    {
        get { return this.lifeForce; }
        set { lifeForce = value; }
    }

    public float YVelocity
    {
        set { this.yVelocity = value; }
    }

    public float InteractTime
    {
        set { interactTime = value; }
    }

    public Animator Anim
    {
        get { return anim; }
        set { anim = Anim; }
    }

    public bool Dead
    {
        get { return dead; }
        set { Dead = dead; }
    }
    #endregion

    #region Main Methods

    void Awake()
    {
        //Just setting all the variables needed
        iM = FindObjectOfType<InputManager>();
        charController = GetComponent<CharacterController>();
        cam = FindObjectOfType<Camera>().transform;
        this.health = maxHealth;
        this.stamina = maxStamina;
        currentMovementType = MovementType.Idle;
        pM = FindObjectOfType<PauseManager>();
        if (pM == null)
            return;
        pM.Pausables.Add(this);
        inventory = gameObject.AddComponent<InventoryManager>();
        slopeLimit = charController.slopeLimit;
        anim = GetComponentInChildren<Animator>();
        healthBar.maxValue = maxHealth;
        staminaBar.maxValue = maxStamina;
        lifeForceBar.maxValue = maxLifeForce;
        healthBar.value = health;
        staminaBar.value = stamina;
        lifeForceBar.value = lifeForce;
        aggroIndicator.SetActive(false);
        cameraFollow = FindObjectOfType<CameraFollow>();
        StartCoroutine("ZeroGravity");
    }

    private void Update()
    {
        if (inputEnabled && !dead)
        {
            //A sprint function which drains the stamina float upon activation
            bool sprinting = false;
            if (!Input.GetButton("Sprint") && currentMovementType != MovementType.Attacking && currentMovementType != MovementType.Dashing/* && currentMovementType != MovementType.Dodging*/ && currentMovementType != MovementType.Sprinting && currentMovementType != MovementType.SuperJumping && stamina < maxStamina)
            {
                if (staminaRegenerating)
                {
                    stamina += (staminaRegen * Time.deltaTime); //Låter spelaren få tillbaka stamina då denne inte sprintar eller använder staminabaserade anilities

                    if (stamina > maxStamina)
                    {
                        stamina = maxStamina;
                    }
                }
                else if (!staminaRegWait)
                {
                    StartCoroutine("StaminaRegenerationWait");
                }
            }
            else
            {
                StopCoroutine("StaminaRegenerationWait");
                staminaRegenerating = false;
                staminaRegWait = false;
                if (charController.isGrounded && Input.GetButton("Sprint") && stamina > 0f && move != Vector3.zero)
                {
                    stamina -= (0.5f * Time.deltaTime);
                    sprinting = true;

                }
            }
            staminaBar.value = stamina;
            if (currentMovementType != MovementType.Attacking && currentMovementType != MovementType.Interacting && currentMovementType != MovementType.Stagger)
            {
                PlayerMovement(sprinting); //Tillåter spelaren att röra sig
            }
            if (!charController.isGrounded && !climbing && currentMovementType != MovementType.Interacting && !noGravity)
            {
                yVelocity -= gravity; //Ser till så att spelaren faller

                if (yVelocity < 0)
                {
                    anim.SetBool("Falling", true);
                }
            }
            if (currentMovementType == MovementType.Stagger && !climbing)
            {
                move = Vector3.zero;
            }

            //Lets the character move with the character controller
            if (!climbing)
                charController.Move(move * Time.deltaTime);

            if (currentInteractable != null && Input.GetButtonDown("Interact"))     //Låter spelaren interagera med föremål i närheten som implementerar IInteractable
            {
                previousMovementType = currentMovementType;
                currentMovementType = MovementType.Interacting;
                currentInteractable.Interact(this);
                this.currentInteractable = null;
                move = Vector3.zero;
                StartCoroutine("NonMovingInteract");
            }

            if (currentWeapon != null && currentWeapon.CanAttack && charController.isGrounded && this.currentWeapon != null && this.currentWeapon.CanAttack     //Låter spelaren slåss
                && (currentMovementType == MovementType.Idle || currentMovementType == MovementType.Running || currentMovementType == MovementType.Sprinting || currentMovementType == MovementType.Walking || currentMovementType != MovementType.Stagger))
            {
                if (Input.GetAxisRaw("Fire2") < -0.5 || Input.GetButtonDown("Fire2"))
                {
                    if (!attacked)
                    {
                        HeavyAttack();
                        attacked = true;
                    }
                }

                if (Input.GetButtonDown("Fire1"))
                {
                    LightAttack();
                }
            }

            if (attacked && (Input.GetAxisRaw("Fire2") > -0.5 || Input.GetAxisRaw("Fire2") < 0.5))
            {
                attacked = false;
            }

            if (secondsUntilResetClick > 0)
            {
                secondsUntilResetClick -= Time.deltaTime;
            }

            if (attackCountdown > 0)
            {
                attackCountdown -= Time.deltaTime;
            }

            if (poiseReset > 0)
            {
                poiseReset -= Time.deltaTime;
            }
            else
            {
                poise = maxPoise;
            }
        }
        else if (!inputEnabled)
        {
            move = Vector3.zero;
        }
    }
    #endregion

    #region Resources
    public void RestoreHealth(int amount)           //Låter spelaren få tillbaka liv
    {
        this.health = Mathf.Clamp(this.health + amount, 0, maxHealth);
        healthBar.value = health;
    }

    public void ReceiveLifeForce(int value)         //Låter spelaren få lifeforce
    {
        this.lifeForce = Mathf.Clamp(this.lifeForce + value, 0, 100);
        lifeForceBar.value = lifeForce;
        print(lifeForce);
    }
    #endregion

    #region Equipment
    void SheatheAndUnsheathe()          //Drar och stoppar undan vapen
    {
        if (!dead && canSheathe)
        {
            bool equip = weaponToEquip == null ? false : true;
            anim.SetBool("WeaponDrawn", equip);
            anim.SetTrigger("SheatheAndUnsheathe");

            if (anim.GetBool("WeaponDrawn"))
            {
                SoundManager.instance.RandomizeSfx(swordSheathe, swordSheathe);
                anim.SetLayerWeight(1, 1);
            }
            else
            {
                anim.SetLayerWeight(1, 0);
            }
            StartCoroutine("SheathingTimer");
        }
    }

    public void Equip(GameObject equipment)         //Equippar föremål
    {
        if (dead)
        {
            return;
        }
        if (equipment == null)
        {
            this.weaponToEquip = equipment;
            SheatheAndUnsheathe();
            return;
        }
        switch (equipment.GetComponent<BaseEquippableObject>().MyType)
        {
            case EquipableType.Ability:
                if (currentAbility != null)
                    Destroy(currentAbility.gameObject);
                currentAbility = Instantiate(equipment).GetComponent<BaseEquippableObject>() as BaseAbilityScript;
                currentRune.sprite = equipment.GetComponent<BaseAbilityScript>().MyRune;
                inventory.EquippedAbilityImage.sprite = equipment.GetComponent<BaseAbilityScript>().InventoryIcon;
                break;

            case EquipableType.Weapon:
                this.weaponToEquip = equipment;
                SheatheAndUnsheathe();
                lastEquippedWeapon = equipment;
                inventory.EquippedWeaponImage.sprite = equipment.GetComponent<BaseWeaponScript>().InventoryIcon;
                break;

            default:
                print("unspecified object type, gör om gör rätt");
                break;
        }
    }

    public void EquipWeapon(GameObject weaponToEquip)    //Code for equipping different weapons
    {
        if (dead)
            return;
        if (currentWeapon != null)
        {
            UnEquipWeapon();
        }
        this.currentWeapon = Instantiate(weaponToEquip, weaponPosition).GetComponent<BaseWeaponScript>();
        this.currentWeapon.Equipper = this;
        FindObjectOfType<SaveManager>().CheckIfUpgraded(this.currentWeapon);
    }

    public void UnEquipWeapon()
    {
        Destroy(this.currentWeapon.gameObject);
        this.currentWeapon = null;
    }
    #endregion

    #region Combat
    //Damage to player
    public void TakeDamage(int incomingDamage, DamageType dmgType)          //Gör så att spelaren kan ta skada och att olika saker händer beroende på vilken typ av skada det är
    {
        if (dead || invulnerable)
        {
            return;
        }
        else
        {
            int finalDamage = ModifyDamage(incomingDamage, dmgType);

            if (finalDamage <= 0)
            {
                return;
            }
            switch (dmgType)
            {
                case DamageType.Fire:
                    StopCoroutine("Burn");
                    StartCoroutine(Burn(5f, finalDamage / 2));
                    break;

                case DamageType.Frost:
                    StopCoroutine("Freeze");
                    StartCoroutine(Freeze(5f));
                    break;

                case DamageType.AutoStagger:
                    StartCoroutine("Stagger");
                    break;

                case DamageType.Falling:
                    if (fallInvulerability)
                        return;
                    break;
            }
            health -= finalDamage;
            healthBar.value = health;
            poise -= incomingDamage;

            if (incomingDamage < health && poise < incomingDamage)
            {
                StartCoroutine("Stagger");
            }
        }
        if (health <= 0)
        {
            Death();
        }
        else
        {
            StartCoroutine("Invulnerability");
        }
    }

    public void EnemyAggro(BaseEnemyScript enemy, bool aggroing)        //Låter spelaren veta att en fiende upptäckt spelaren
    {
        if (aggroing)
        {
            enemiesAggroing.Add(enemy);
        }
        else
        {
            enemiesAggroing.Remove(enemy);
        }
        if (enemiesAggroing.Count > 0)
        {
            aggroIndicator.SetActive(true);
        }
        else
            aggroIndicator.SetActive(false);
    }

    public void LightAttack()    //Sets the current movement type as attacking and which attack move thats used
    {
        if (charController.isGrounded && grounded && attackCountdown <= 0f)
        {
            this.currentWeapon.Attack(1f, false);
            this.currentWeapon.StartCoroutine("AttackCooldown");

            attackCooldown = 0.5f;

            currentWeapon.CurrentSpeed = 0.5f;

            if (secondsUntilResetClick <= 0)
            {
                nuOfClicks = 0;
            }

            Mathf.Clamp(nuOfClicks, 0, 3);

            nuOfClicks++;

            if (nuOfClicks == 1)
            {
                anim.SetTrigger("LightAttack1");
                secondsUntilResetClick = 1.5f;
            }

            if (nuOfClicks == 2)
            {
                anim.SetTrigger("LightAttack2");
                secondsUntilResetClick = 1.5f;
            }

            if (nuOfClicks == 3)
            {
                anim.SetTrigger("LightAttack3");
                nuOfClicks = 0;
                attackCooldown = 1f;
                currentWeapon.CurrentSpeed = 1f;
            }

            SoundManager.instance.RandomizeSfx(lightAttack1, lightAttack2);

            move = Vector3.zero;
            move += transform.forward * attackMoveLength;

            attackCountdown = attackCooldown;
        }
    }

    public void HeavyAttack()    //Sets the current movement type as attacking and which attack move thats used
    {
        if (charController.isGrounded && grounded && attackCountdown <= 0f)
        {
            currentMovementType = MovementType.Attacking;

            attackCooldown = 0.5f;

            currentWeapon.CurrentSpeed = 0.5f;

            if (secondsUntilResetClick <= 0)
            {
                nuOfClicks = 0;
            }

            Mathf.Clamp(nuOfClicks, 0, 2);

            nuOfClicks++;

            if (nuOfClicks == 1)
            {
                anim.SetTrigger("HeavyAttack1");
                secondsUntilResetClick = 1.5f;
            }

            if (nuOfClicks == 2 || nuOfClicks == 3)
            {
                anim.SetTrigger("HeavyAttack2");
                nuOfClicks = 0;
                attackCooldown = 1f;
            }
            SoundManager.instance.RandomizeSfx(heavyAttack1, heavyAttack2);

            move = Vector3.zero;

            attackCountdown = attackCooldown;

            StartCoroutine("HeavyAttackWait");
        }
    }

    IEnumerator HeavyAttackWait()
    {
        yield return new WaitForSeconds(0.5f);
        this.currentWeapon.Attack(1.5f, true);
        this.currentWeapon.StartCoroutine("AttackCooldown");
    }

    public void Leech(int damageDealt)      //Om spelaren slåss med ett vapen med leech får denne tillbaka 10% av skadan som liv
    {
        RestoreHealth(((damageDealt / 10) * leechAmount));
        float floatDmg = damageDealt;
        RestoreHealth(Mathf.RoundToInt(floatDmg / 100f) * leechPercentage);
    }

    int ModifyDamage(int damage, DamageType dmgType)    //Modifies damage depending on armor, resistance etc
    {
        if (dmgType == DamageType.Physical)
        {
            damage -= armor;
        }
        else
            foreach (DamageType resistance in resistances)
            {
                if (dmgType == resistance)
                {
                    damage /= 2;
                    break;
                }
            }
        return damage;
    }

    public void Kill()      //Dödar spelaren
    {
        if (!dead)
        {
            Death();
        }
    }

    void Death()            //Kallas när spelaren dör, via skada eller Kill()
    {
        dead = true;
        healthBar.value = 0f;
        if (hitNormal.y > 0)
        {
            anim.SetTrigger("RightDead");
        }
        else if (hitNormal.y < 0)
        {
            anim.SetTrigger("LeftDead");
        }
        iM.SetInputMode(InputMode.Paused);
        deathScreen.SetActive(true);
    }

    #endregion

    #region Systems
    public void PauseMe(bool pausing)       //Ser till att spelaren
    {
        inputEnabled = !pausing;
    }
    #endregion

    #region Movement
    public void PlayerMovement(bool sprinting)      //Sköter spelarens rörelser
    {
        if (climbing)
            return;
        // Gets the movement axis' for character controller and assigns them to variables
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //Creates a vector3 to change the character controllers forward to the direction of the camera
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1).normalized);

        move = v * camForward + h * cam.right;

        if (move.magnitude > 0.0000001f && currentMovementType != MovementType.Dashing && currentMovementType != MovementType.Dodging)
        {
            dashDir = null;
            move *= moveSpeed;

            if (sprinting || jumpMomentum)
            {
                move *= sprintSpeed;
            }
            //Changes the character models rotation to be in the direction its moving
            if (!cameraFollow.LockOn)
            {
                anim.SetLayerWeight(2, 0);
                transform.rotation = Quaternion.LookRotation(move);
            }
            else if (cameraFollow.LockOn)
            {
                transform.LookAt(cameraFollow.LookAtMe.transform);
                transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
                anim.SetLayerWeight(1, 0);
                anim.SetLayerWeight(2, 1);
            }
        }

        charSpeed = CalculateSpeed(charController.velocity);

        if (currentMovementType != MovementType.Dodging && currentMovementType != MovementType.Dashing && currentMovementType != MovementType.SuperJumping)
        {
            anim.SetFloat("Speed", charSpeed);
            anim.SetFloat("SpeedX", h);
            anim.SetFloat("SpeedZ", v);

            if (charSpeed < 1 && currentMovementType != MovementType.Jumping)
            {
                currentMovementType = MovementType.Idle;
            }
            else if (charSpeed >= 1 && charSpeed < 5 && currentMovementType != MovementType.Jumping)
            {
                currentMovementType = MovementType.Walking;
            }
            else if (charSpeed >= 5 && charSpeed < 15 && currentMovementType != MovementType.Jumping)
            {
                currentMovementType = MovementType.Running;
            }
        }

        //If the player character is on the ground you can jump
        if (charController.isGrounded && canJump)
        {
            if (Input.GetButtonDown("Jump") && grounded && iM.CurrentInputMode == InputMode.Playing && currentMovementType != MovementType.Dashing && currentMovementType != MovementType.Dodging
                && currentMovementType != MovementType.Attacking && currentMovementType != MovementType.Interacting && currentMovementType != MovementType.Stagger)
            {
                if (currentClimbable != null)
                {
                    currentClimbable.Climb(this);
                }
                else
                {
                    if (sprinting)
                    {
                        jumpMomentum = true;
                    }
                    canJump = false;
                    yVelocity = jumpSpeed;
                    anim.SetTrigger("Jump");
                    currentMovementType = MovementType.Jumping;
                }
            }
        }
        move.y += (yVelocity * Time.deltaTime);

        //If the player character is on the ground you may dodge/roll/evade as a way to avoid something
        if (charController.isGrounded)
        {
            if (Input.GetButtonDown("Dodge") && currentMovementType != MovementType.Jumping && canJump)
            {
                print("dodge?");
                if (stamina >= dodgeCost && canDodge)
                {
                    print("Dodge!");
                    stamina -= dodgeCost;
                    staminaBar.value = stamina;
                    anim.SetTrigger("Dodge");
                    StartCoroutine("Dodge");
                    StartCoroutine("DodgeCooldown");
                }
            }
        }

        //Using the character transforms' forward direction to assign which direction to dash and then moves the character in the direction with higher velocity than normal
        if (currentMovementType == MovementType.Dodging)
        {
            if (dodgeDir == null)
            {
                dodgeVelocity = transform.forward * dodgeSpeed;
                move += dodgeVelocity;
                dodgeDir = move;
            }
            else
            {
                move = (Vector3)dodgeDir;
            }
        }

        //Using the character transforms' forward direction to assign which direction to dash and then moves the character in the direction with high velocity
        if (currentMovementType == MovementType.Dashing)
        {
            if (dashDir == null)
            {
                dashVelocity = transform.forward * 3;
                move.y = 0;
                move += dashVelocity * Time.deltaTime;
                dashDir = move;
                dashedTime = 0f;
            }
            else
            {
                move = (Vector3)dashDir;
                if (dashedTime < 2f)
                {
                    dashedTime += Time.deltaTime;
                }
                else
                {
                    currentMovementType = MovementType.Idle;
                }
            }
        }

        if (!grounded && hitNormal.y >= 0f) //Får spelaren att glida ned för branta ytor
        {
            float xVal = -hitNormal.y * hitNormal.x * slideFriction;
            float zVal = -hitNormal.y * hitNormal.z * slideFriction;
            move.x += xVal;
            move.z += zVal;
        }

        //If the angle of the object hit by the character controller collider is less or equal to the slopelimit you are grounded and wont slide down
        grounded = (Vector3.Angle(Vector3.up, hitNormal) <= slopeLimit);

        if (!wasGrounded && charController.isGrounded && currentMovementType != MovementType.Dodging && currentMovementType != MovementType.Dashing)  //När spelaren landar efter ett hopp
        {
            StartCoroutine("JumpCooldown");

            Landing();

            anim.SetBool("Falling", false);

            if (move.y < -safeFallDistance)
            {
                //TakeDamage(Mathf.Abs(Mathf.RoundToInt((move.y * 3f) + safeFallDistance)), DamageType.Falling);  //FallDamage, avstängd för att den dödar spelaren utan att spelaren faktiskt faller då vissa av spelarens handlingar/animationer får spelaren att inte räknas som grounded
            }

            jumpMomentum = false;
            currentMovementType = MovementType.Idle;
        }

        if (sprinting && charController.velocity.magnitude > 0f && currentMovementType != MovementType.Jumping && currentMovementType != MovementType.Dodging && currentMovementType != MovementType.Dashing)
        {
            currentMovementType = MovementType.Sprinting;
        }
        wasGrounded = charController.isGrounded;
        anim.SetBool("Land", charController.isGrounded);
    }

    float CalculateSpeed(Vector3 velocity)      //Räknar ut vilken fart spelaren har så att animatorn kan spela upp rätt animation
    {
        Vector3 newVelocity = new Vector3(velocity.x, 0f, velocity.z);
        return newVelocity.magnitude;
    }
    #endregion

    #region Sound Events

    void Footsteps()                            //Spelar upp fotstegsljud då spelaren går
    {
        if (charController.isGrounded && grounded && move != Vector3.zero)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (hit.collider.gameObject.tag == "Sand")
                {
                    leftFoot.volume = footStepsVolume;
                    leftFoot.PlayOneShot(sandSteps);
                }
                else if (hit.collider.gameObject.tag == "Stone")
                {
                    leftFoot.volume = footStepsVolume;
                    leftFoot.PlayOneShot(stoneSteps);
                }
                else if (hit.collider.gameObject.tag == "Wood")
                {
                    leftFoot.volume = footStepsVolume;
                    leftFoot.PlayOneShot(woodSteps);
                }
            }
        }
    }

    void Landing()      //Spelar upp ett ljud då spelaren landar efter ett hopp
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            if (hit.collider.gameObject.tag == "Sand")
            {
                leftFoot.volume = landingVolume;
                rightFoot.volume = landingVolume;
                leftFoot.PlayOneShot(landingSand);
                rightFoot.PlayOneShot(landingSand);
            }
            else if (hit.collider.gameObject.tag == "Stone")
            {
                leftFoot.volume = landingVolume;
                rightFoot.volume = landingVolume;
                leftFoot.PlayOneShot(landingStone);
                rightFoot.PlayOneShot(landingStone);
            }
            else if (hit.collider.gameObject.tag == "Wood")
            {
                leftFoot.volume = landingVolume;
                rightFoot.volume = landingVolume;
                leftFoot.PlayOneShot(landingWood);
                rightFoot.PlayOneShot(landingWood);
            }
        }
    }
    #endregion

    #region Colliders
    void OnTriggerEnter(Collider other)         //Avgör vilken IIinteractable spelaren kan interagera med
    {
        if (other.gameObject.GetComponent<IInteractable>() != null)
            currentInteractable = other.gameObject.GetComponent<IInteractable>();
    }

    void OnTriggerExit(Collider other)
    {
        IInteractable otherInteractable = other.gameObject.GetComponent<IInteractable>();
        if (otherInteractable != null && currentInteractable == otherInteractable)
            currentInteractable = null;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)         //Hjälper till att få spelaren att glida ned för branta ytor
    {
        hitNormal = hit.normal;
    }
    #endregion

    #region Coroutines
    public IEnumerator ZeroGravity()
    {
        yield return new WaitForSeconds(5);
        noGravity = false;
    }

    public IEnumerator Climb(AnimationClip climbAnim)           //Får spelaren att kunna klättra upp för vissa ytor
    {
        ClimbableScript currentClimb = currentClimbable;
        gameObject.transform.LookAt(currentClimbable.FinalClimbingPosition);
        gameObject.transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
        this.climbing = true;
        string animTrigger = (climbAnim.length > 2f) ? "Climb1" : "Climb2";
        anim.SetTrigger(animTrigger);
        yield return new WaitForSeconds(climbAnim.length - 0.15f);
        gameObject.transform.position = currentClimb.FinalClimbingPosition.position;
        this.climbing = false;
    }

    public IEnumerator AbilityCooldown()                //Hindrar spelaren från att använda abilities under en tid efter att en ability använts
    {
        BaseAbilityScript.CoolingDown = true;
        yield return new WaitForSeconds(abilityCooldown);
        BaseAbilityScript.CoolingDown = false;
    }

    IEnumerator DodgeCooldown()                         //Hindrar spelaren från att hoppa eller använda sin dodge under en tid efter att spelaren hoppat eller dodgen använts
    {
        if (!dead)
        {
            canDodge = false;
            canJump = false;
            yield return new WaitForSeconds(dodgeCooldown);
            canJump = true;
            canDodge = true;
        }
    }

    IEnumerator Dodge()              //Enumerator smooths out the dodge/roll/evade so it doesn't happen instantaneously
    {
        if (!dead)
        {
            currentMovementType = MovementType.Dodging;
            yield return new WaitForSeconds(dodgeLength);
            currentMovementType = MovementType.Running;
            dodgeDir = null;
        }
    }

    IEnumerator NonMovingInteract()             //Hindrar spelaren från att röra sig medan denne interagerar med något
    {
        canJump = false;
        yield return new WaitForSeconds(interactTime);
        currentMovementType = MovementType.Idle;
        canJump = true;
    }

    IEnumerator SheathingTimer()                //Spawnar och despawnar vapen efter en viss tid för att matcha med animationer
    {
        if (!dead)
        {
            canSheathe = false;
            yield return new WaitForSeconds(0.4f);
            if (weaponToEquip != null)
            {
                SoundManager.instance.RandomizeSfx(swordUnsheathe, swordUnsheathe);
                EquipWeapon(weaponToEquip);
            }
            else if (currentWeapon != null)
            {
                UnEquipWeapon();
            }
            canSheathe = true;
        }
    }

    IEnumerator Invulnerability()       //Hindrar spelaren från att ta skada under en viss tid
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerablityTime);
        invulnerable = false;
    }

    public IEnumerator PreventFallDamage()          //Hindrar spelaren från att ta fallskada under en viss tid
    {
        fallInvulerability = true;
        yield return new WaitForSeconds(5f);
        fallInvulerability = false;

    }

    IEnumerator Stagger()
    {
        currentMovementType = MovementType.Stagger;
        anim.SetTrigger("Stagger");
        poiseReset = poiseCooldown;
        yield return new WaitForSeconds(staggerTime);
        currentMovementType = MovementType.Idle;
    }

    protected IEnumerator Burn(float burnDuration, int burnDamage)      //Gör så att spelaren tar eldskada under en viss tid
    {
        burning = true;
        timeToBurn += burnDuration;
        while (timeToBurn > 0f)
        {
            yield return new WaitForSeconds(0.5f);
            this.health -= burnDamage;
            timeToBurn -= Time.deltaTime;
        }
        timeToBurn = 0f;
        burning = false;
    }

    protected IEnumerator Freeze(float freezeTime)      //Får spelaren att röra sig långsammare under en viss tid
    {
        if (!frozen)
        {
            frozen = true;
            float originalSpeed = moveSpeed;
            yield return new WaitForSeconds(freezeTime);
            moveSpeed = originalSpeed;
            frozen = false;
        }
    }

    IEnumerator StaminaRegenerationWait()           //Väntar en stund innan spelaren kan återfå stamina
    {
        staminaRegWait = true;
        yield return new WaitForSeconds(staminaRegenWait);
        staminaRegWait = false;
        staminaRegenerating = true;
    }

    IEnumerator JumpCooldown()          //Hindrar spelaren från att hoppa under en viss tid efter att denne hoppat
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
    #endregion
}