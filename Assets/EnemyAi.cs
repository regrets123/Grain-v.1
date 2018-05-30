using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

#region Enums

public enum Units
{
    Raider, Guardian, Hound, FamineBoss
}

public enum AIStates
{
    Close, Far, Attacking, InSight
}

#endregion

#region Classes

public class EnemyAi : MonoBehaviour, IKillable, IPausable
{
    #region Inspector

    [Header("Unit Info"), Space(10), Tooltip("The enemy's name or title in the world"), SerializeField]
    Units unitName;
    [Space(5), Tooltip("The enemy's resistances from different types of damages which reduces damage taken from the player"), SerializeField]
    DamageType[] resistances;
    [Space(5), Tooltip("The enemy's different attack animation they may use"), SerializeField]
    AIAttacks[] aiAttacks;

    [Space(10)]

    [Header("Stats"), Space(10), Tooltip("This is the maximum amount of health the enemy may have"), SerializeField]
    int maxHealth;
    [Space(5), Tooltip("This is the life-energy the player will gain from killing this enemy"), SerializeField]
    int maxLifeforce;
    [Space(5), Tooltip("This is this enemy's max resistance to getting staggered/stunned from attacks"), SerializeField]
    int maxPoise;

    [Space(10)]

    [Header("Enemy Objects"), Space(10), Tooltip("This is the souls of the enemy which brings the lifeforce the enemy possess to the player upon death"), SerializeField]
    GameObject soul;
    [Space(5), Tooltip("The enemy unit's weapon"), SerializeField]
    GameObject weapon;
    [Space(5), Tooltip("The enemy unit's weaponposition"), SerializeField]
    Transform weaponPos;
    [Space(5), Tooltip("The enemy unit's weaponcolliders for different attacks"), SerializeField]
    GameObject[] defaultDamageColliders;

    [Space(10)]

    [Header("Enemy UI"), Space(10), Tooltip("The slider-healthbar that shows the enemy's current health with UI elements"), SerializeField]
    Slider healthBar;
    [Space(5), Tooltip("The enemy unit's canvas which all its UI is place in"), SerializeField]
    Canvas enemyCanvas;

    [Space(10)]

    [Header("AI Variables"), Space(10), Tooltip(""), SerializeField]
    int frameCount = 30;
    [Space(5), Tooltip(""), SerializeField]
    float closeCount = 10;
    [Space(5), Tooltip(""), SerializeField]
    float sight;
    [Space(5), Tooltip(""), SerializeField]
    float fovAngle;
    [Space(5), Tooltip(""), SerializeField]
    float attackCount = 30;
    [Space(5), Tooltip(""), SerializeField]
    float attackDistance;

    #endregion

    #region States

    bool isInvincible;
    bool canMove;
    bool isDead;
    bool canAttack;
    bool haveDestination;
    bool rotateToTarget;
    bool burning;
    bool frozen;
    bool alive = true;

    AIStates aiState;

    #endregion

    #region References

    Animator animator;
    Rigidbody rigidB;
    PlayerCombat target;
    NavMeshAgent navAgent;
    PauseManager pauseManager;

    #endregion

    #region Variables

    int health;
    int lifeForce;
    int poise;
    int frame;

    string attackName;

    float distance;
    float angle;
    float closeNr;
    float distance2;
    float delta;
    float nrOfattacks;
    float timeToBurn = 0f;
    float DistanceFromTarget()
    {
        if (target == null)
            return 100;

        return Vector3.Distance(target.transform.position, transform.position);
    }
    float AngleToTarget()
    {
        float a = 180;

        if (target)
        {
            Vector3 d = dirToTarget;
            a = Vector3.Angle(d, transform.forward);
        }

        return a;
    }

    Vector3 dirToTarget;
    Vector3 targetDestination;

    LayerMask ignoreLayers = ~(1 << 8);

    AIAttacks currentAttack;

    #endregion

    #region Properties

    public Units UnitName
    {
        get { return unitName; }
    }

    public bool Alive
    {
        get { return this.alive; }
    }

    #endregion

    #region Main Methods

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidB = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        pauseManager = GetComponent<PauseManager>();
        target = FindObjectOfType<PlayerCombat>();
        navAgent.stoppingDistance = attackDistance - 0.5f;
        health = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

	void Update ()
    {
        if (alive)
        {
            distance = DistanceFromTarget();
            angle = AngleToTarget();

            delta = Time.deltaTime;

            animator.SetFloat("Speed", navAgent.velocity.magnitude);

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(attackName))
            {
                rotateToTarget = false;
                canMove = false;
                navAgent.isStopped = true;
            }
            else
            {
                rotateToTarget = true;
                canMove = true;
                navAgent.isStopped = false;
            }

            if (target)
                dirToTarget = target.transform.position - transform.position;

            switch (aiState)
            {
                case AIStates.Close:
                    HandleCloseSight();
                    break;
                case AIStates.Far:
                    HandleFarSight();
                    break;
                case AIStates.Attacking:
                    if (canMove)
                        aiState = AIStates.InSight;
                    break;
                case AIStates.InSight:
                    InSight();
                    break;
                default:
                    break;
            }
        }
	}

    #endregion

    #region AI Behaviour-Handlers

    public AIAttacks WillAttack()
    {
        int w = 0;

        List<AIAttacks> attacks = new List<AIAttacks>();

        for (int i = 0; i < aiAttacks.Length; i++)
        {
            AIAttacks ai = aiAttacks[i];

            if (ai.cool > 0)
            {
                continue;
            }

            if (distance > ai.minDistance)
                continue;
            if (angle < ai.minAngle)
                continue;
            if (angle > ai.maxAngle)
                continue;
            if (ai.weight == 0)
                continue;

            w += ai.weight;
            attacks.Add(ai);
        }

        if (attacks == null)
            return null;

        int rando = Random.Range(0, w + 1);
        int cw = 0;

        for (int i = 0; i < attacks.Count; i++)
        {
            cw += attacks[i].weight;

            if (cw > rando)
            {
                return attacks[i];
            }
        }

        return null;
    }

    void RaycastToTarget()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        origin.y += 0.5f;
        Vector3 dir = dirToTarget;
        dir.y += 0.5f;

        if(Physics.Raycast(origin, dir, out hit, sight, ignoreLayers))
        {
            if (hit.transform.GetComponent<PlayerCombat>() != null)
            {
                aiState = AIStates.InSight;
                SetDestination(target.transform.position);
            }
        }
    }

    void SetDestination(Vector3 d)
    {
        if (!haveDestination)
        {
            haveDestination = true;
            navAgent.isStopped = false;
            navAgent.SetDestination(d);
            targetDestination = d;
        }
    }

    void HandleCloseSight()
    {
        closeNr++;

        if (closeNr > closeCount)
        {
            closeNr = 0;

            if (distance < sight || angle < fovAngle)
            {
                aiState = AIStates.Far;
                return;
            }
        }

        RaycastToTarget();
    }

    void HandleFarSight()
    {
        if (target == null)
            return;

        frame++;

        if (frame > frameCount)
        {
            frame = 0;
            
            if(distance < sight)
            {
                if(angle < fovAngle)
                {
                    aiState = AIStates.Close;
                }
            }
        }
    }
    
    void InSight()
    {
        if(rotateToTarget)
            LookTowardsTarget();

        HandleCooldowns();

        distance2 = Vector3.Distance(targetDestination, target.transform.position);

        if (distance2 > attackDistance)
        {
            haveDestination = false;
            SetDestination(target.transform.position);
        }
        if (distance < attackDistance)
            navAgent.isStopped = true;

        if (nrOfattacks > 0)
        {
            nrOfattacks--;
            return;
        }
        nrOfattacks = attackCount;

        AIAttacks attack = WillAttack();

        if (attack != null)
            SetCurrentAttack(attack);
        else
            return;

        if (currentAttack != null && attack != null)
        {
            aiState = AIStates.Attacking;
            animator.SetTrigger(currentAttack.targetAnim);
            currentAttack.cool = currentAttack.cooldown;
            attackName = currentAttack.targetAnim;
            return;
        }
        return;
    }

    void HandleCooldowns()
    {
        for (int i = 0; i < aiAttacks.Length; i++)
        {
            AIAttacks ai = aiAttacks[i];

            if (ai.cool > 0)
            {
                ai.cool -= delta;
                if (ai.cool < 0)
                    ai.cool = 0;
            }
        }
    }

    void LookTowardsTarget()
    {
            Vector3 dir = dirToTarget;
            dir.y = 0;

            if (dir == Vector3.zero)
                dir = transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * 5);
    }

    void OnAnimatorMove()
    {
        if (animator == null)
        {
            return;
        }
        transform.position = animator.rootPosition;
    }

    public void LightAttack()
    {

    }

    public void HeavyAttack()
    {

    }

    #endregion

    #region AI Attack-Behaviour

    void SetCurrentAttack(AIAttacks a)
    {
        currentAttack = a;
    }

    void ActivateDamageCollider()
    {
        if (currentAttack == null)
            return;

        if (currentAttack.isDefaultDamageColliders/* || currentAttack.damageCollider.Length < 1*/)
        {
            ObjectListStatus(defaultDamageColliders, true);
        }
        else
        {
            ObjectListStatus(currentAttack.damageCollider, true);
        }
    }

    void DeactivateDamageCollider()
    {
        if (currentAttack == null)
            return;

        if (currentAttack.isDefaultDamageColliders/* || currentAttack.damageCollider.Length < 1*/)
        {
            ObjectListStatus(defaultDamageColliders, false);
        }
        else
        {
            ObjectListStatus(currentAttack.damageCollider, false);
        }
    }

    void ObjectListStatus(GameObject[] l, bool status)
    {
        for (int i = 0; i < l.Length; i++)
        {
            l[i].GetComponent<Collider>().enabled = status;
        }
    }

    #endregion

    #region AI Combat States

    public virtual void TakeDamage(int incomingDamage, DamageType dmgType)          //Låter fienden ta skada och gör olika saker beroende på skadetyp
    {
        if (!alive)
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
                FindObjectOfType<PlayerCombat>().Leech(damage);
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
        animator.SetTrigger("Death");
        //SoundManager.instance.RandomizeSfx(death);
        this.target = null;
        navAgent.isStopped = true;
        PlayerAbilities abilities = FindObjectOfType<PlayerAbilities>();
        if (abilities.GetComponent<InventoryManager>().EquippableAbilities != null && abilities.GetComponent<InventoryManager>().EquippableAbilities.Count > 0)
        {
            //Instantiate(soul, transform.position, Quaternion.identity).GetComponent<LifeForceTransmitterScript>().StartMe(abilities, lifeForce, this);
        }
        Destroy(gameObject, 7);
    }

    public void Kill()          //Dödar automatiskt fienden
    {
        alive = false;
        Death();
    }

    #endregion

    #region AI Animation Events

    void AttackStart()
    {
        ActivateDamageCollider();
    }

    void AttackEnd()
    {
        DeactivateDamageCollider();
    }

    #endregion

    public void PauseMe(bool pausing)
    {
        if (!alive)
            return;

        navAgent.isStopped = !navAgent.isStopped;
    }

    #region Coroutines


    protected IEnumerator FreezeNav(float freezeTime)           //Hindrar fienden från att röra sig under en viss tid
    {
        navAgent.isStopped = true;
        yield return new WaitForSeconds(freezeTime);
        navAgent.isStopped = false;
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
            float originalSpeed = navAgent.speed;
            navAgent.speed /= 2f;
            yield return new WaitForSeconds(freezeTime);
            navAgent.speed = originalSpeed;
            frozen = false;
        }
    }

    protected IEnumerator Stagger()
    {
        animator.SetTrigger("Stagger");
        //poiseReset = poiseCooldown;
        yield return new WaitForSeconds(1);
    }

    #endregion
}

[System.Serializable]
public class AIAttacks
{
    public int weight;
    public float minDistance;
    public float minAngle;
    public float maxAngle;
    public float cooldown = 2;
    public float cool;
    public string targetAnim;
    public bool isDefaultDamageColliders;
    public GameObject[] damageCollider;
}

#endregion
