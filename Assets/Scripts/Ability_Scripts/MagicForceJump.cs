using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson && Björn Andersson*/

public class MagicForceJump : BaseAbilityScript
{
    [SerializeField]
    GameObject effectsPrefab, spawnPos;

    [SerializeField]
    float magicJumpSpeed, delayTime;

    public override void UseAbility()
    {
            base.UseAbility();
            //instantiate a magic jump circle
            StartCoroutine("SuperJump");
    }
    
    protected override void Update()
    {
        if (/*(player.CurrentMovementType == MovementType.Idle
           || player.CurrentMovementType == MovementType.Sprinting //Låter spelaren använda abilities när den inte attackerar, dodgar eller liknande
           || player.CurrentMovementType == MovementType.Walking
           || player.CurrentMovementType == MovementType.Jumping)
           && */Input.GetButtonDown("Ability")
           && !coolingDown && !combat.Dead)
        {
            if (movement.Stamina >= abilityCost)          //Får magicforcejump att dra stamina istället för lifeforce
            {
                movement.Stamina -= abilityCost;
                UseAbility();
            }
        }
    }

    IEnumerator SuperJump()
    {   
        GameObject jumpParticles = Instantiate(effectsPrefab, spawnPos.transform.position, spawnPos.transform.rotation);
        //player.Anim.SetTrigger("SuperJump");
        yield return new WaitForSeconds(delayTime);
        //Add a force to the player going up form your current position.
        //movement.YVelocity = magicJumpSpeed;
        Destroy(jumpParticles, 1.5f);
    }
}
