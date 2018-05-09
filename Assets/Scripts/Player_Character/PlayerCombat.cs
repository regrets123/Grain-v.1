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
    float invulerabilityTime;

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

    Slider healthBar;

    int health;

    BaseWeaponScript currentWeapon;

    PauseManager pM;

    GameObject weaponToEquip, lastEquippedWeapon, deathScreen, aggroIndicator;

    Animator anim;

    List<DamageType> resistances = new List<DamageType>();

    bool invulnerable = false, dead = false, canSheathe = true, burning = false, attacked = false;

    float secondsUntilResetClick, attackCountdown = 0f, interactTime, dashedTime, poiseReset, poise, timeToBurn = 0f;

    PlayerMovement movement;

    #endregion

    #region Properties

    public bool Dead
    {
        get { return dead; }
        set { Dead = dead; }
    }

    #endregion

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        health = maxHealth;
        //Find healthBar
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
        if (currentWeapon != null && currentWeapon.CanAttack && movement.IsGrounded && this.currentWeapon != null && this.currentWeapon.CanAttack)     //Låter spelaren slåss
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

    }

    public void LightAttack()
    {

    }

    public void HeavyAttack()
    {

    }

    public void TakeDamage(int amount, DamageType dmgType)
    {

    }

    public void Kill()
    {

    }


}
