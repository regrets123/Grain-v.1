using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson && Björn Andersson*/

public class MagicForceJump : BaseAbilityScript
{
    [SerializeField]
    GameObject effectsPrefab, spawnPos;

    [SerializeField]
    float delayTime;

    public override void UseAbility()
    {
            base.UseAbility();
            //instantiate a magic jump circle
            StartCoroutine("SuperJump");
    }

    IEnumerator SuperJump()
    {   
        GameObject jumpParticles = Instantiate(effectsPrefab, spawnPos.transform.position, spawnPos.transform.rotation);
        movement.Anim.SetTrigger("SuperJump");
        movement.ChangeMovement("None");
        yield return new WaitForSeconds(delayTime);
        movement.ChangeMovement("Previous");
        movement.Jump(true);
        Destroy(jumpParticles, 1.5f);
    }
}
