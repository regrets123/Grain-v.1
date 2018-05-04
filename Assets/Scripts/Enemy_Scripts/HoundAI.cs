using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class HoundAI : BaseEnemyScript
{
    [SerializeField]
    float howlTime, minJumpAttackDistance;

    [SerializeField]
    GameObject weapon2;

    [SerializeField]
    Transform weapon2Pos;

    protected override void Start()
    {
        base.Start();
        this.weapon2 = Instantiate(weapon2, weapon2Pos);            //Då Hound har två vapen (tassar) instantieras det andra vapnet här
        weapon2.GetComponent<BaseWeaponScript>().GetComponent<Collider>().enabled = false;
    }

    protected override void Aggro(PlayerControls newTarget)
    {
        StartCoroutine(AggroHowl(newTarget));       //Fryser Hound och får den att yla då den upptäcker spelaren innan den blir aggressiv
    }

    public override void LightAttack()      //Låter Hound göra en swipe attack
    {
        previousMovementType = currentMovementType;
        this.currentMovementType = MovementType.Attacking;
        anim.SetTrigger("SwipeAttack");
        RandomizeAttackSfx(lightAttack1, lightAttack2);
        weapon.GetComponent<BaseWeaponScript>().Attack(0.5f, false);
        weapon.GetComponent<BaseWeaponScript>().StartCoroutine("AttackCooldown");
        StartCoroutine("AttackCooldown");
    }

    public override void HeavyAttack()
    {
        if (Vector3.Distance(transform.position, target.transform.position) < minJumpAttackDistance)
        {
            LightAttack();
            return;
        }
        StartCoroutine("JumpAttack");
    }


    protected IEnumerator JumpAttack()      //Låter Hound göra en hoppattack
    {
        if (alive && target != null)
        {
            previousMovementType = MovementType.Idle;
            this.currentMovementType = MovementType.Attacking;
            attackColliderActivationSpeed = 0.5f;
            attackColliderDeactivationSpeed = 1.0f;
            nav.isStopped = true;
            yield return new WaitForSeconds(0.5f);
            nav.isStopped = false;
            attackColliderActivationSpeed = 0.2f;
            attackColliderDeactivationSpeed = 4f;
            StartCoroutine(ActivateAttackCollider(1));
            anim.SetTrigger("JumpAttack");
            RandomizeAttackSfx(chargeAttack);
            Vector3 targetPosition = target.gameObject.transform.position;
            float originalSpeed = nav.speed;
            nav.speed = nav.speed * 4;
            nav.destination = targetPosition;
            weapon.GetComponent<BaseWeaponScript>().StartCoroutine("AttackCooldown");
            StartCoroutine("AttackCooldown");
            yield return new WaitForSeconds(1.5f);
            nav.speed = originalSpeed;
            nav.destination = target.gameObject.transform.position;
        }
    }
    IEnumerator AggroHowl(PlayerControls newTarget)     //Hound ylar innan den blir aggressiv mot spelaren
    {
        transform.LookAt(newTarget.transform);
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
        anim.SetTrigger("Aggro");
        RandomizeOtherSfx(aggro);
        yield return new WaitForSeconds(howlTime);
        base.Aggro(newTarget);
    }
}
