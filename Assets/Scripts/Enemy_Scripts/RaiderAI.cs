using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class RaiderAI : BaseEnemyScript
{
    [SerializeField]
    AudioClip raiderHowl;

    public override void HeavyAttack()      //En heavy attack som tar lång tid att utföra men gör mycket skada
    {
        if (!alive)
        {
            return;
        }
        StartCoroutine(FreezeNav(2f));
        previousMovementType = currentMovementType;
        this.currentMovementType = MovementType.Attacking;
        heavyAttack = Random.Range(1, 3);

        attackColliderActivationSpeed = 1.0f;
        attackColliderDeactivationSpeed = 1.5f;

        StartCoroutine(ActivateAttackCollider(heavyAttack));

        if (heavyAttack == 1)
        {
            anim.SetTrigger("HeavyAttack1");
        }
        else if (heavyAttack == 2)
        {
            anim.SetTrigger("HeavyAttack2");
        }

        weapon.GetComponent<BaseWeaponScript>().StartCoroutine("AttackCooldown");
        StartCoroutine("AttackCooldown");
    }

    protected override void Aggro(PlayerControls newTarget)
    {
        base.Aggro(newTarget);
        SoundManager.instance.RandomizeSfx(raiderHowl, raiderHowl);     //Raidern blir aggressiv mot spelaren som alla andra fiender, men gör också ett ljud då detta händer
    }

    void Footsteps()           //Raidern ska ha fotstegsljud senare som spelas upp härifrån, men detta är inte implementerat ännu
    {
        return;
    }
}
