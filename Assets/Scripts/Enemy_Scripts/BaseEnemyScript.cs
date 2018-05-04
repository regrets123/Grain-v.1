using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/*By Björn Andersson*/

public class BaseEnemyScript : MonoBehaviour, IKillable, IPausable
{
    #region Serialized Variables
    [SerializeField]
    protected float aggroRange, attackRange, invulnerabilityTime, attackSpeed, loseAggroTime, loseAggroDistance, maxAggroFollow, maxPoise, staggerTime, poiseCooldown, attackColliderActivationSpeed,
                    attackColliderDeactivationSpeed;

    [SerializeField]
    protected int maxHealth, lifeForce;

    [SerializeField]
    protected string unitName;

    [SerializeField]
    protected GameObject aggroCenter, soul;

    [SerializeField]
    protected GameObject weapon;

    [SerializeField]
    protected Transform weaponPos;

    [SerializeField]
    protected DamageType[] resistances;

    [SerializeField]
    protected AudioClip lightAttack1, lightAttack2, sandSteps, stoneSteps, woodSteps, chargeAttack, aggro, death, bossTeleportDisappear, bossTeleportAppear, bossMagicAttack, bossEnrage;

    [SerializeField]
    protected AudioSource footSteps, attacks, otherSFX;

    [SerializeField]
    protected Slider healthBar;

    [SerializeField]
    protected Canvas enemyCanvas;

    [SerializeField]
    float lowPitchedRange = 0.95f, highPitchedRange = 1.05f;
    #endregion

    #region Non-Serialized Variables
    protected bool canAttack = true, burning = false, frozen = false;

    protected int health, lightAttack, heavyAttack, attack;

    protected float poiseReset, poise, timeToBurn = 0f;

    protected Collider aggroCollider;

    protected PauseManager pM;

    protected PlayerControls target;

    protected SphereCollider aggroBubble;

    protected NavMeshAgent nav;

    protected Animator anim;

    protected bool invulnerable = false, alive = true, losingAggro = false;

    protected Vector3 initialPos;

    public bool Alive
    {
        get { return this.alive; }
    }

    protected MovementType currentMovementType, previousMovementType;

    public MovementType CurrentMovementType
    {
        get { return this.currentMovementType; }
        set { this.currentMovementType = value; }
    }
    #endregion

    public string UnitName
    {
        get { return this.unitName; }
    }

    #region Main Methods
    protected virtual void Start()      //Hittar alla relevanta saker och instansierar vapen
    {
        this.nav = GetComponent<NavMeshAgent>();
        this.health = maxHealth;
        this.currentMovementType = MovementType.Idle;
        this.pM = FindObjectOfType<PauseManager>();
        this.anim = GetComponentInChildren<Animator>();
        pM.Pausables.Add(this);
        aggroBubble = aggroCenter.AddComponent<SphereCollider>();
        aggroBubble.isTrigger = true;
        aggroBubble.radius = aggroRange;

        healthBar.maxValue = maxHealth;
        
        enemyCanvas.enabled = false;

        if (this.weapon != null)
        {
            this.weapon = Instantiate(weapon, weaponPos);
        }
        weapon.GetComponent<BaseWeaponScript>().GetComponent<Collider>().enabled = false;
    }

    protected virtual void Update()
    {
        if (alive && target != null)
        {
            if (currentMovementType != MovementType.Attacking && (!(this is FamineBossAI) || !(this as FamineBossAI).Consuming))
            {
                gameObject.transform.LookAt(target.transform);
                gameObject.transform.rotation = new Quaternion(0f, gameObject.transform.rotation.y, 0f, gameObject.transform.rotation.w);
            }

            if (canAttack && weapon.GetComponent<BaseWeaponScript>().CanAttack && !target.Dead && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                if (currentMovementType != MovementType.Stagger)
                {
                    attack = Random.Range(1, 3);

                    if (attack == 1)
                    {
                        LightAttack();
                    }
                    else if (attack == 2)
                    {
                        HeavyAttack();
                    }
                }
            }
            else if (target.Dead)
            {
                LoseAggro();
                return;
            }
            else if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
            {
                if (currentMovementType != MovementType.Stagger && currentMovementType != MovementType.Attacking)
                {
                    if (!(this is FamineBossAI) || (!(this as FamineBossAI).Enraged || !(this as FamineBossAI).Consuming))
                    {
                        nav.isStopped = false;
                        nav.SetDestination(target.transform.position);
                    }
                    
                }
            }
            if (!losingAggro && Vector3.Distance(transform.position, target.transform.position) > loseAggroDistance)
            {
                StartCoroutine("LoseAggroTimer");
            }
            if (initialPos != null && Vector3.Distance(transform.position, initialPos) > maxAggroFollow)
            {
                LoseAggro();
            }
        }
        anim.SetFloat("Speed", nav.velocity.magnitude);

        if (poiseReset > 0)
        {
            poiseReset -= Time.deltaTime;
        }

        if (poiseReset <= 0)
        {
            poise = maxPoise;
        }
    }
    #endregion

    public void PauseMe(bool pausing)
    {
        if (!alive)
            return;

        nav.isStopped = !nav.isStopped;
    }

    #region Sounds
    void Footstep()         //Spelar upp fotstegsljud då fienden rör sig
    {
        if (!nav.isStopped)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (hit.collider.gameObject.tag == "Sand")
                {
                    footSteps.PlayOneShot(sandSteps);
                }
                else if (hit.collider.gameObject.tag == "Stone")
                {
                    footSteps.PlayOneShot(stoneSteps);
                }
                else if (hit.collider.gameObject.tag == "Wood")
                {
                    footSteps.PlayOneShot(woodSteps);
                }
            }
        }
    }
    #endregion

    #region Combat
    //Får fienden att anfalla spelaren när spelaren kommer tillräckligt nära
    protected void OnTriggerEnter(Collider other)
    {
        if (alive && other.gameObject.tag == "Player")
        {
            if (target == null)
                Aggro(other.gameObject.GetComponent<PlayerControls>());
            else if (target == other.gameObject.GetComponent<PlayerControls>())
            {
                StopCoroutine("LoseAggroTimer");
            }
        }
    }

    protected virtual void Aggro(PlayerControls newTarget)       //Får fienden att bli aggressiv mot spelaren
    {
        if (this.initialPos == null)
            this.initialPos = this.transform.position;
        if (newTarget == null)
            return;
        this.target = newTarget;
        target.EnemyAggro(this, true);
        enemyCanvas.enabled = true;
        healthBar.value = health;
    }

    //Gör att fienden kan bli skadad
    public virtual void TakeDamage(int incomingDamage, DamageType dmgType)          //Låter fienden ta skada och gör olika saker beroende på skadetyp
    {
        if (!alive || invulnerable)
        {
            return;
        }
        int damage = ModifyDamage(incomingDamage, dmgType);
        this.health -= damage;
        healthBar.value = health;
        poise -= incomingDamage;

        switch (dmgType)
        {
            case DamageType.Fire:
                StopCoroutine("Burn");
                StartCoroutine(Burn(5f, damage / 5));
                break;

            case DamageType.Frost:
                StopCoroutine("Freeze");
                StartCoroutine(Freeze(5f));
                break;

            case DamageType.Leech:
                FindObjectOfType<PlayerControls>().Leech(damage);
                break;
        }
        if (incomingDamage < health && poise < incomingDamage)
        {
            StartCoroutine("Stagger");
        }

        if (this.health <= 0)
        {
            Death();
        }
        else
        {
            StartCoroutine("Invulnerability");
        }
    }

    //Låter fienden attackera
    public virtual void LightAttack()
    {
        if (!alive)
        {
            return;
        }
        StartCoroutine(FreezeNav(2f));
        previousMovementType = currentMovementType;
        this.currentMovementType = MovementType.Attacking;
        int maxAttack = (this is RaiderAI ? 4 : 3);
        lightAttack = Random.Range(1, maxAttack);

        attackColliderActivationSpeed = 0.5f;
        attackColliderDeactivationSpeed = 1.0f;

        StartCoroutine(ActivateAttackCollider(lightAttack));

        if (lightAttack == 1)
        {
            anim.SetTrigger("LightAttack1");
        }
        else if (lightAttack == 2)
        {
            anim.SetTrigger("LightAttack2");
        }
        else if (lightAttack == 3)
        {
            anim.SetTrigger("LightAttack3");
        }

        SoundManager.instance.RandomizeSfx(lightAttack1, lightAttack2);

        weapon.GetComponent<BaseWeaponScript>().StartCoroutine("AttackCooldown");
        StartCoroutine("AttackCooldown");
    }

    public virtual void HeavyAttack()           //Metod som overrideas av fiender som har heavy attacks
    {
        LightAttack();
    }

    protected virtual void Dodge()              //Metod som overrideas av fiender som har dodge
    {
        return;
    }

    protected void LoseAggro()                  //Låter fienden sluta vara aggressiv mot spelaren
    {
        target.EnemyAggro(this, false);
        target = null;
        losingAggro = false;
        nav.SetDestination(initialPos);
    }
    
    protected virtual int ModifyDamage(int damage, DamageType dmgType)    //Modifierar skadan fienden tar efter armor, resistance och liknande
    {
        foreach (DamageType resistance in this.resistances)
        {
            if (dmgType == resistance)
            {
                damage /= 2;
                break;
            }
        }
        return damage;
    }

    protected virtual void Death()          //Kallas när fienden dör
    {
        alive = false;
        anim.SetTrigger("Death");
        SoundManager.instance.RandomizeSfx(death);
        this.target = null;
        nav.isStopped = true;
        PlayerControls player = FindObjectOfType<PlayerControls>();
        if (player.Inventory.EquippableAbilities != null && player.Inventory.EquippableAbilities.Count > 0)
        {
            Instantiate(soul, transform.position, Quaternion.identity).GetComponent<LifeForceTransmitterScript>().StartMe(player, lifeForce, this);
        }
        Destroy(gameObject, 7);
    }

    public void Kill()          //Dödar automatiskt fienden
    {
        alive = false;
        Death();
    }
    #endregion

    #region Coroutines

    protected IEnumerator AttackCooldown()          //Ser till att en viss tid går mellan fiendens attacker
    {
        canAttack = false;
        yield return new WaitForSeconds(attackSpeed);
        currentMovementType = previousMovementType;
        canAttack = true;
    }
        
    protected virtual IEnumerator ActivateAttackCollider(int attackNo)          //Aktiverar fiendens vapens collider under en viss tid så att vapnet kan göra skada
    {
        yield return new WaitForSeconds(attackColliderActivationSpeed);
        weapon.GetComponent<BaseWeaponScript>().GetComponent<Collider>().enabled = true;
        yield return new WaitForSeconds(attackColliderDeactivationSpeed);
        weapon.GetComponent<BaseWeaponScript>().GetComponent<Collider>().enabled = false;
    }

    protected IEnumerator LoseAggroTimer()      //Får fienden att sluta vara aggressiv mot spelaren efter en viss tid
    {
        losingAggro = true;
        yield return new WaitForSeconds(loseAggroTime);
        LoseAggro();
    }

    protected IEnumerator FreezeNav(float freezeTime)           //Hindrar fienden från att röra sig under en viss tid
    {
        nav.isStopped = true;
        yield return new WaitForSeconds(freezeTime);
        nav.isStopped = false;
    }

    protected IEnumerator Burn(float burnDuration, int burnDamage)              //Gör eldskada på fienden under en viss tid
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

    protected IEnumerator Freeze(float freezeTime)          //Sänker fiendens fart under en viss tid
    {
        if (!frozen)
        {
            frozen = true;
            float originalSpeed = nav.speed;
            nav.speed /= 2f;
            yield return new WaitForSeconds(freezeTime);
            nav.speed = originalSpeed;
            frozen = false;
        }
    }

    protected IEnumerator Stagger()
    {
        if (currentMovementType != MovementType.Stagger)
            previousMovementType = currentMovementType;
        currentMovementType = MovementType.Stagger;
        anim.SetTrigger("Stagger");
        poiseReset = poiseCooldown;
        yield return new WaitForSeconds(staggerTime);
        currentMovementType = previousMovementType;
    }

    protected IEnumerator Invulnerability()         //Hindrar fienden från att ta skada under en viss tid
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        invulnerable = false;
    }
    #endregion

    #region Sound
    public void RandomizeAttackSfx(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchedRange, highPitchedRange);

        attacks.pitch = randomPitch;
        attacks.clip = clips[randomIndex];
        attacks.PlayOneShot(attacks.clip);
    }

    public void RandomizeOtherSfx(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchedRange, highPitchedRange);

        otherSFX.pitch = randomPitch;
        otherSFX.clip = clips[randomIndex];
        otherSFX.PlayOneShot(otherSFX.clip);
    }

    public void RandomizeFootstepSfx(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchedRange, highPitchedRange);

        footSteps.pitch = randomPitch;
        footSteps.clip = clips[randomIndex];
        footSteps.PlayOneShot(footSteps.clip);
    }
    #endregion
}
