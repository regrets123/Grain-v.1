using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson && Andreas Nilsson*/

public enum DamageType      //Olika typer av skada som kan hanteras på olika sätt och göra olika saker
{
    Physical, Frost, Fire, Falling, Leech, AutoStagger
}

public enum AttackMoves
{
    QuickAttack, StrongAttack, Sideswipe, PiercingAttack
}

public enum AttackMoveSets
{
    LightWeapon, HeavyWeapon
}

public enum Upgrade         //Olika uppgraderingstyper
{
    None, DamageUpgrade, FireUpgrade, FrostUpgrade, LeechUpgrade
}

public class BaseWeaponScript : BaseEquippableObject
{
    [SerializeField]
    protected int origninalLightDamage, originalHeavyDamage, lightDamage;

    protected int heavyDamage;

    [SerializeField]
    protected float attackSpeed;

    [SerializeField]
    protected AttackMoves[] attacks;

    [SerializeField]
    protected DamageType dmgType;

    [SerializeField]
    protected AudioClip enemyHit1, enemyHit2, enemyHit3, swing1, swing2, thrust;

    protected bool canAttack = true, heavy = false;

    protected MovementType previousMovement;

    protected Upgrade currentUpgrade = Upgrade.None;

    protected float currentSpeed;

    protected int upgradeLevel = 0;

    protected Collider myColl;

    public int UpgradeLevel
    {
        get { return this.upgradeLevel; }
    }

    public Upgrade CurrentUpgrade
    {
        get { return this.currentUpgrade; }
    }

    public float CurrentSpeed
    {
        get { return this.currentSpeed; }
        set { this.currentSpeed = value; StartCoroutine("ResetSpeed"); }
    }

    public bool CanAttack
    {
        get { return this.canAttack; }
    }

    public float AttackSpeed
    {
        get { return this.attackSpeed; }
    }

    public AttackMoves[] Attacks
    {
        get { return this.attacks; }
    }

    protected IKillable equipper;

    public IKillable Equipper
    {
        set { if (this.equipper == null) this.equipper = value; }
        get { return this.equipper; }
    }

    protected override void Start()
    {
        base.Start();
        this.myColl = GetComponent<Collider>();
        this.currentSpeed = attackSpeed;
        if (this.lightDamage == 0)
        {
            this.lightDamage = origninalLightDamage;
            this.heavyDamage = originalHeavyDamage;
        }
        this.equipper = GetComponentInParent<IKillable>();
        this.myColl.enabled = false;
    }

    public void Attack(float attackTime, bool heavy)        //Håller koll på om vapnet ska göra light eller heavy skada
    {
        if (!canAttack)
            return;
        this.heavy = heavy;
        StartCoroutine(AttackMove(attackTime));
    }

    protected IEnumerator AttackMove(float attackTime)      //Tillåter vapnet att göra skada under tiden det svingas
    {
        myColl.enabled = true;
        yield return new WaitForSeconds(attackTime);
        myColl.enabled = false;
    }

    public IEnumerator AttackCooldown()         //Hindrar vapnet från att attackera under en viss tid efter det attackerat
    {
        this.canAttack = false;
        if (equipper is PlayerControls)
        {
            previousMovement = player.CurrentMovementType;
            player.CurrentMovementType = MovementType.Attacking;
            yield return new WaitForSeconds(currentSpeed);
            player.CurrentMovementType = previousMovement;
        }
        else if (equipper is BaseEnemyScript)
        {
            previousMovement = (equipper as BaseEnemyScript).CurrentMovementType;
            (equipper as BaseEnemyScript).CurrentMovementType = MovementType.Attacking;
            yield return new WaitForSeconds(currentSpeed);
            (equipper as BaseEnemyScript).CurrentMovementType = previousMovement;
        }
        else
        {
            print("nu gick nåt åt helvete");
        }
        this.canAttack = true;
        if (equipper is PlayerControls)
            (equipper as PlayerControls).CurrentMovementType = MovementType.Idle;
        else
            (equipper as BaseEnemyScript).CurrentMovementType = MovementType.Idle;
    }

    public void ApplyUpgrade(Upgrade upgrade)       //Uppgraderar vapnet
    {
        if (this.currentUpgrade != Upgrade.DamageUpgrade)
            this.upgradeLevel = 0;
        this.currentUpgrade = upgrade;
        if (upgrade == Upgrade.DamageUpgrade && upgradeLevel < 3)
        {
            upgradeLevel++;
            this.lightDamage += lightDamage / 2;
            this.heavyDamage += heavyDamage / 2;
        }
        else
        {
            this.upgradeLevel = 1;
            switch (upgrade)
            {
                case Upgrade.FireUpgrade:
                    this.dmgType = DamageType.Fire;
                    break;

                case Upgrade.FrostUpgrade:
                    this.dmgType = DamageType.Frost;
                    break;

                case Upgrade.LeechUpgrade:
                    this.dmgType = DamageType.Leech;
                    break;

                default:
                    print("Nu blev nåt fel här");
                    break;
            }
            this.lightDamage = origninalLightDamage;
            this.heavyDamage = originalHeavyDamage;
        }
    }

    //Deals damage to an object with IKillable on it.
    public virtual void DealDamage(IKillable target)
    {
        int damage = (heavy == true ? heavyDamage : lightDamage);
        target.TakeDamage(damage, dmgType);
    }

    protected IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(1f);
        this.currentSpeed = attackSpeed;
    }

    //When a weapon hits a killable target the script triggers and deals damage to target
    public void OnTriggerEnter(Collider other)
    {
        if ((equipper is BaseEnemyScript && (equipper as BaseEnemyScript).CurrentMovementType == MovementType.Attacking)
            || (equipper is PlayerControls && (equipper as PlayerControls).CurrentMovementType == MovementType.Attacking))
        {
            IKillable targetToHit = other.gameObject.GetComponent<IKillable>();
            if (targetToHit == null || (equipper is BaseEnemyScript && targetToHit is BaseEnemyScript) || (equipper is PlayerControls && targetToHit is PlayerControls))
            {
                return;
            }
            DealDamage(targetToHit);
            SoundManager.instance.RandomizeSfx(enemyHit1, enemyHit2, enemyHit3);
        }
    }
}