using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public class PlayerCombat : MonoBehaviour, IKillable, IPausable
{
    #region Serialized Variables

    [Header("Combat Stats")]

    [Space(5)]

    [SerializeField]
    int maxHealth;

    [SerializeField]
    int leechAmount;

    [SerializeField]
    float invulnerabilityTime;

    [SerializeField]
    float staggerTime;

    [SerializeField]
    float maxPoise;

    [SerializeField]
    float poiseCooldown;

    [SerializeField]
    int armor;

    [SerializeField]
    int leechPercentage;

    [SerializeField]
    float attackCooldown;

    [Space(10)]

    [Header("Weapon")]

    [Space(5)]

    [SerializeField]
    Transform weaponPosition;

    [Space(10)]

    [Header("Combat Audio")]

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

    #endregion

    #region Non-Serialized Variables

    int health, nuOfClicks = 0;

    bool invulnerable = false, dead = false, canSheathe = true, burning = false, attacked = false, paused = false, fallInvulnerability = false;

    float secondsUntilResetClick, attackCountdown = 0f, interactTime, dashedTime, poiseReset, poise, timeToBurn = 0f;

    Vector3 hitNormal;

    GameObject weaponToEquip, lastEquippedWeapon, deathScreen, aggroIndicator;

    Slider healthBar;

    Animator anim;

    BaseWeaponScript currentWeapon;

    PlayerMovement movement;

    PauseManager pM;

    List<DamageType> resistances = new List<DamageType>();

    List<BaseEnemyScript> enemiesAggroing = new List<BaseEnemyScript>();

    #endregion

    #region Properties

    public bool Dead
    {
        get { return dead; }
    }

    public BaseWeaponScript CurrentWeapon
    {
        get { return this.currentWeapon; }
    }

    public GameObject LastEquippedWeapon
    {
        get { return this.lastEquippedWeapon; }
    }

    public int Health
    {
        get { return this.health; }
        set { this.health = value; this.healthBar.value = health; }
    }

    public Slider HealthBar
    {
        get { return this.healthBar; }
    }

    #endregion

    #region Main Methods

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        health = maxHealth;
        healthBar = GameObject.Find("HealthSlider").GetComponent<Slider>();
        aggroIndicator = GameObject.Find("CombatIndicator");
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
        pM = FindObjectOfType<PauseManager>();
        if (pM == null)
            return;
        pM.Pausables.Add(this);
        aggroIndicator.SetActive(false);
    }

    void Update()
    {
        if (!paused && currentWeapon != null && currentWeapon.CanAttack && movement.IsGrounded && this.currentWeapon != null && this.currentWeapon.CanAttack)     //Låter spelaren slåss
                                                                                                                                                                  //&& (currentMovementType == MovementType.Idle || currentMovementType == MovementType.Running || currentMovementType == MovementType.Sprinting || currentMovementType == MovementType.Walking || currentMovementType != MovementType.Stagger))
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

    public void PauseMe(bool pausing)
    {
        this.paused = pausing;
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
                    //StopCoroutine("Freeze");
                    //StartCoroutine(Freeze(5f));

                    //Lägg Freeze och frozen i PlayerMovement

                    break;

                case DamageType.AutoStagger:
                    StartCoroutine("Stagger");
                    break;

                case DamageType.Falling:
                    if (fallInvulnerability)
                        return;
                    break;
            }
            health -= finalDamage;
            //healthBar.value = health;
            poise -= incomingDamage;

            if (dmgType != DamageType.Falling && incomingDamage < health && poise < incomingDamage)
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
        if (movement.IsGrounded && attackCountdown <= 0f)
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

            attackCountdown = attackCooldown;
        }
    }

    public void HeavyAttack()    //Sets the current movement type as attacking and which attack move thats used
    {
        if (movement.IsGrounded && attackCountdown <= 0f)
        {
            //currentMovementType = MovementType.Attacking;

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
        //iM.SetInputMode(InputMode.Paused);
        deathScreen.SetActive(true);
    }

    #endregion

    public void RestoreHealth(int amount)           //Låter spelaren få tillbaka liv
    {
        this.health = Mathf.Clamp(this.health + amount, 0, maxHealth);
        healthBar.value = health;
    }

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
        yield return new WaitForSeconds(invulnerabilityTime);
        invulnerable = false;
    }

    public IEnumerator PreventFallDamage()          //Hindrar spelaren från att ta fallskada under en viss tid
    {
        fallInvulnerability = true;
        yield return new WaitForSeconds(5f);
        fallInvulnerability = false;

    }

    IEnumerator Stagger()
    {
        //currentMovementType = MovementType.Stagger;
        anim.SetTrigger("Stagger");
        poiseReset = poiseCooldown;
        yield return new WaitForSeconds(staggerTime);
        //currentMovementType = MovementType.Idle;
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
}
