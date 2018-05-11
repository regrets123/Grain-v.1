using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilssion && Björn Andersson*/

public class MagicDash : BaseAbilityScript
{
    [SerializeField]
    float duration;

    public override void UseAbility()    //Activated from the BaseAbility script. If the player have enough stamina the ability will activate and drain the staminaCost
    {
        base.UseAbility();
        //player.Anim.SetTrigger("Dash");
        StartCoroutine("Dash");
    }

    IEnumerator Dash()    //Enumerator smooths out the dash so it doesn't happen instantaneously
    {
        movement.ChangeMovement("Dash");
        yield return new WaitForSeconds(duration);
        movement.ChangeMovement("Previous");
    }
}
