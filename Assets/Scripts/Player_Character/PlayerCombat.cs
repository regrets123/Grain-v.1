using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson && Andreas Nilsson*/

public interface IKillable          //Interface som används av spelaren och alla fiender samt eventuella förstörbara objekt
{
    void LightAttack();
    void HeavyAttack();
    void TakeDamage(int damage, DamageType dmgType);
    void Kill();
}

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

    bool invulnerable = false, dead = false, canSheathe = true, burning = false, attacking = false, paused = false, fallInvulnerability = false, combo1 = false, combo2 = false, canClick = true;

    float secondsUntilResetClick, attackCountdown = 0f, interactTime, dashedTime, poiseReset, poise, timeToBurn = 0f, noOfClicks = 0f;

    Vector3 hitNormal;

    GameObject weaponToEquip, lastEquippedWeapon, deathScreen, aggroIndicator;

    Slider healthBar;

    Animator anim;

    BaseWeaponScript currentWeapon;

    PlayerMovement movement;

    PauseManager pM;

    List<DamageType> resistances = new List<DamageType>();

    List<BaseEnemyScript> enemiesAggroing = new List<BaseEnemyScript>();

    Rigidbody rb;

    InputManager inputManager;

    #endregion

    #region Properties

    public GameObject WeaponToEquip
    {
        set { this.weaponToEquip = value; }
    }

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

    public bool Attacking
    {
        get { return attacking; }
        set { attacking = value; }
    }

    #endregion

    #region Main Methods

    void Start()
    {
        deathScreen = GameObject.Find("DeathScreen");
        deathScreen.SetActive(false);
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        health = maxHealth;
        healthBar = GameObject.Find("HealthSlider").GetComponent<Slider>();
        aggroIndicator = GameObject.Find("CombatIndicator");
        rb = GetComponent<Rigidbody>();
        pM = FindObjectOfType<PauseManager>();
        pM.Pausables.Add(this);
        aggroIndicator.SetActive(false);
        inputManager = FindObjectOfType<InputManager>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
        if (pM == null)
            return;
    }

    void Update()
    {
        if (inputManager.CurrentInputMode != InputMode.Playing)
            return;
        if (anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack1") || anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack2") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack3") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack1") || 
            anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack2") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack3") ||
            anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack1") || anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack2") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack1") || anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack2"))
        {
            attacking = true;
            rb.velocity -= rb.velocity;
        }
        else
        {
            attacking = false;
        }

        if (!paused && currentWeapon != null && movement.IsGrounded && this.currentWeapon != null)     //Låter spelaren slåss
        {
            if (movement.Stamina >= currentWeapon.LightStaminaCost && Input.GetButtonDown("Fire1"))
            {
                LightAttack();
            }
            if (movement.Stamina >= currentWeapon.HeavyStaminaCost && (Input.GetAxisRaw("Fire2") < -0.5 || Input.GetButtonDown("Fire2")))
            {
                HeavyAttack();
            }
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
                    StopCoroutine(movement.Freeze());
                    StartCoroutine(movement.Freeze());
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
            healthBar.value = health;
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

    void OnAnimatorMove()
    {
        if (anim == null)
        {
            return;
        }
        transform.position = anim.rootPosition;
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
        FindObjectOfType<InputManager>().SetInputMode(InputMode.None);
        healthBar.value = 0f;
        if (hitNormal.y > 0)
        {
            anim.SetTrigger("RightDead");
        }
        else
        {
            anim.SetTrigger("LeftDead");
        }
        movement.ChangeMovement("None");
        deathScreen.SetActive(true);
        dead = true;
        foreach (BaseEnemyScript enemy in enemiesAggroing)
        {
            enemy.LoseAggro();
        }
    }

    public void LightAttack()
    {
        if (canClick)
            noOfClicks++;
        StopCoroutine("AttackLoopPrevention");
        StartCoroutine("AttackLoopPrevention");

        if (noOfClicks == 1 && (!anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack3") || !anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack3")))
        {
            anim.SetInteger("LightAnimation", 1);
        }
    }

    public void HeavyAttack()
    {
        if (canClick)
            noOfClicks++;
        StopCoroutine("AttackLoopPrevention");
        StartCoroutine("AttackLoopPrevention");

        if (noOfClicks == 1 && (!anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack2") || !anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack2")))
            {
                anim.SetInteger("HeavyAnimation", 1);
            }
    }

    #region ComboEvents

    public void LightComboCheck()
    {
        currentWeapon.Attack(1f, false);

        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("LightAttack1") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack1") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack1")) && noOfClicks == 1)
        {//If the first animation is still playing and only 1 click has happened, return to idle
            anim.SetInteger("LightAnimation", 4);
            canClick = true;
            noOfClicks = 0;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("LightAttack1") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack1") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack1")) && noOfClicks >= 2)
        {//If the first animation is still playing and at least 2 clicks have happened, continue the combo     
            anim.SetInteger("LightAnimation", 2);
            canClick = true;
            noOfClicks = 2;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("LightAttack2") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack2") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack2")) && noOfClicks == 2)
        {  //If the second animation is still playing and only 2 clicks have happened, return to idle  
            anim.SetInteger("LightAnimation", 4);
            canClick = true;
            noOfClicks = 0;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("LightAttack2") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack2") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack2")) && noOfClicks >= 3)
        {  //If the second animation is still playing and at least 3 clicks have happened, continue the combo    
            anim.SetInteger("LightAnimation", 3);
            canClick = true;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("LightAttack3") || anim.GetCurrentAnimatorStateInfo(1).IsName("LightAttack3") ||
            anim.GetCurrentAnimatorStateInfo(2).IsName("LightAttack3")))
        { //Since this is the third and last animation, return to idle          
            anim.SetInteger("LightAnimation", 4);
            canClick = true;
            noOfClicks = 0;
        }
    }

    void LightAttackActivate()
    {
        currentWeapon.Attack(1f, false);
    }

    public void HeavyComboCheck()
    {

        if ((anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack1") || anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack1")) && noOfClicks == 1)
        {  //If the first animation is still playing and only 1 clicks have happened, return to idle  
            anim.SetInteger("HeavyAnimation", 3);
            canClick = true;
            attacking = false;
            noOfClicks = 0;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack1") || anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack1")) && noOfClicks >= 2)
        {  //If the first animation is still playing and at least 2 clicks have happened, continue the combo    
            anim.SetInteger("HeavyAnimation", 2);
            canClick = true;
        }
        else if ((anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyAttack2") || anim.GetCurrentAnimatorStateInfo(2).IsName("HeavyAttack2")))
        { //Since this is the second and last animation, return to idle          
            anim.SetInteger("HeavyAnimation", 3);
            canClick = true;
            attacking = false;
            noOfClicks = 0;
        }
    }

    void HeavyAttackActivate()
    {
        currentWeapon.Attack(1f, true);
    }

    void ResetNoOfClicks()
    {
        noOfClicks = 0;
    }

    #endregion

    #endregion

    public void RestoreHealth(int amount)           //Låter spelaren få tillbaka liv
    {
        this.health = Mathf.Clamp(this.health + amount, 0, maxHealth);
        healthBar.value = health;
    }

    #region Equipment
    public void SheatheAndUnsheathe()          //Drar och stoppar undan vapen
    {
        if (!dead && canSheathe)
        {
            bool equip = weaponToEquip == null ? false : true;
            anim.SetBool("WeaponDrawn", equip);
            anim.SetTrigger("SheatheAndUnsheathe");

            if (anim.GetBool("WeaponDrawn"))
            {
                anim.SetLayerWeight(1, 1);
            }
            else
            {
                anim.SetLayerWeight(1, 0);
            }
            StartCoroutine("SheathingTimer");
        }
    }

    void EquipWeapon(GameObject weaponToEquip)    //Code for equipping different weapons
    {
        if (dead)
            return;
        if (currentWeapon != null)
        {
            UnEquipWeapon();
        }
        this.currentWeapon = Instantiate(weaponToEquip, weaponPosition).GetComponent<BaseWeaponScript>();
        this.currentWeapon.Equipper = this;
        this.lastEquippedWeapon = weaponToEquip;
        FindObjectOfType<SaveManager>().CheckIfUpgraded(this.currentWeapon);
    }

    public void UnEquipWeapon()
    {
        Destroy(this.currentWeapon.gameObject);
        this.currentWeapon = null;
    }
    #endregion

    #region Coroutines

    IEnumerator AttackLoopPrevention()
    {
        yield return new WaitForSeconds(1.5f);
        noOfClicks = 0;
        anim.SetInteger("HeavyAnimation", 0);
        anim.SetInteger("LightAnimation", 0);
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
                SoundManager.instance.RandomizeSfx(swordSheathe, swordSheathe);
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

    public IEnumerator Invulnerability(float time)       //Hindrar spelaren från att ta skada under en viss tid
    {
        invulnerable = true;
        yield return new WaitForSeconds(time);
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

    IEnumerator HeavyAttackWait()
    {
        yield return new WaitForSeconds(0.5f);
        this.currentWeapon.Attack(1.5f, true);
        this.currentWeapon.StartCoroutine("AttackCooldown");
    }

    #endregion
}
