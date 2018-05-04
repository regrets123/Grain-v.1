using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class GuardianAI : BaseEnemyScript
{
    [SerializeField]
    Transform weaponPos2;

    [SerializeField]
    GameObject weapon2;

    protected override void Start()
    {
        base.Start();

        this.weapon2 = Instantiate(weapon2, weaponPos2);        //Då Guardian har två vapen instantieras det andra vapnet här
        weapon2.GetComponent<BaseWeaponScript>().GetComponent<BoxCollider>().enabled = false;
    }

    protected override void Update()
    {
        base.Update();
        if (alive && target != null && canAttack && weapon.GetComponent<BaseWeaponScript>().CanAttack && !target.Dead && Vector3.Distance(transform.position, target.transform.position) > 8 &&
            Vector3.Distance(transform.position, target.transform.position) < aggroRange && this.currentMovementType != MovementType.Attacking)
        {
            StartCoroutine("DashAttack");
        }
    }

    protected IEnumerator DashAttack()       //Låter Guardian göra en dash attack mot spelaren
    {
        if (alive && target != null)
        {
            previousMovementType = currentMovementType;
            this.currentMovementType = MovementType.Attacking;
            attackColliderActivationSpeed = 0.5f;
            attackColliderDeactivationSpeed = 1.0f;
            nav.isStopped = true;
            yield return new WaitForSeconds(0.5f);
            nav.isStopped = false;
            attackColliderActivationSpeed = 0.2f;
            attackColliderDeactivationSpeed = 4f;
            StartCoroutine(ActivateAttackCollider(1));
            anim.SetTrigger("DashAttack");
            RandomizeAttackSfx(lightAttack1, lightAttack2);
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

    protected override IEnumerator ActivateAttackCollider(int attackNo)     //Slår av och på collidern på rätt vapen så att Guardian kan göra skada på spelaren
    {
        GameObject weaponToAttackWith = (attackNo == 1 ? weapon2 : weapon);
        yield return new WaitForSeconds(attackColliderActivationSpeed);
        weaponToAttackWith.GetComponent<BaseWeaponScript>().GetComponent<BoxCollider>().enabled = true;
        yield return new WaitForSeconds(attackColliderDeactivationSpeed);
        weaponToAttackWith.GetComponent<BaseWeaponScript>().GetComponent<BoxCollider>().enabled = false;
    }
}
