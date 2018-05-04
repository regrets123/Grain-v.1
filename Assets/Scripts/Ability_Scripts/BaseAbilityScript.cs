using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class BaseAbilityScript : BaseEquippableObject       //Ett script alla magic abilities ärver från
{
    [SerializeField]
    protected int abilityCost;

    [SerializeField]
    protected Sprite myRune;

    protected static bool coolingDown = false;

    public static bool CoolingDown
    {
        get { return coolingDown; }
        set { coolingDown = value; }
    }

    public Sprite MyRune
    {
        get { return this.myRune; }
    }


    public virtual void UseAbility()                      //Virtuell metod som overrideas av alla abilities så att de faktiskt gör olika saker
    {
        player.StartCoroutine("AbilityCooldown");       //Startar en cooldown när spelaren använder en ability
    }

    protected virtual void Update()
    {
        if ((player.CurrentMovementType == MovementType.Idle
            || player.CurrentMovementType == MovementType.Sprinting //Låter spelaren använda abilities när den inte attackerar, dodgar eller liknande
            || player.CurrentMovementType == MovementType.Walking
            || player.CurrentMovementType == MovementType.Jumping)
            && Input.GetButtonDown("Ability")
            && !coolingDown && !player.Dead)
        {
            if (player.LifeForce >= abilityCost)    //Drar resurser för att använda abilities
            {
                player.LifeForce -= abilityCost;
                player.LifeforceBar.value = player.LifeForce;
            }
            else
            {
                player.Health -= abilityCost;
                player.HealthBar.value = player.Health;
            }

            UseAbility();
        }
    }
}
