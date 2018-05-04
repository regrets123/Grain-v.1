using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class HealthPotion : BaseAbilityScript
{
    [SerializeField]
    int amount;

    //Restore health to the player based on the amount the potion will give
    public override void UseAbility()
    {
        base.UseAbility();
        player.RestoreHealth(amount);
    }
}
