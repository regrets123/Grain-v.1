using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum Units
{
    Raider, Guardian, Hound, FamineBoss
}

public enum AIStates
{
    Close, Far, Attacking, InSight
}

public class EnemyAi : MonoBehaviour
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

    #endregion

    #region States

    bool isInvincible;
    bool canMove;
    bool isDead;
    bool canAttack;
    bool haveDestination;
    bool burning;
    bool frozen;

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

    float distance;
    float angle;
    float closeNr;
    float distance2;
    float delta;
    float nrOfattacks;

    Vector3 dirToTarget;
    Vector3 targetDestination;

    LayerMask ignoreLayers = ~(1 << 8);

    #endregion

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

    #region Main Methods

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidB = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        pauseManager = GetComponent<PauseManager>();
        target = FindObjectOfType<PlayerCombat>();
        health = maxHealth;
    }

	void Update ()
    {
        distance = DistanceFromTarget();
        angle = AngleToTarget();

        delta = Time.deltaTime;

        animator.SetFloat("Speed", navAgent.velocity.magnitude);

        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("LightAttack1") && 
           !animator.GetCurrentAnimatorStateInfo(0).IsName("LightAttack2") &&
           !animator.GetCurrentAnimatorStateInfo(0).IsName("LightAttack3"))
        {
            canMove = true;
        }
        else
        {
            canMove = false;
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

    #endregion

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
        LookTowardsTarget();
        HandleCooldowns();

        distance2 = Vector3.Distance(targetDestination, target.transform.position);

        if (distance2 > 2)
        {
            haveDestination = false;
            SetDestination(target.transform.position);
        }
        if (distance < 2)
            navAgent.isStopped = true;

        if (nrOfattacks > 0)
        {
            nrOfattacks--;
            return;
        }
        nrOfattacks = attackCount;

        AIAttacks attack = WillAttack();

        if (attack != null)
        {
            aiState = AIStates.Attacking;
            animator.SetTrigger(attack.targetAnim);
            canMove = false;
            attack.cool = attack.cooldown;
            navAgent.isStopped = true;
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
}
