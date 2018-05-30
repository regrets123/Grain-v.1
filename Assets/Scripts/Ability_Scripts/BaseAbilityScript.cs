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

    public virtual bool UseAbility()                      //Virtuell metod som overrideas av alla abilities så att de faktiskt gör olika saker
    {
        if ( movement.Stamina >= abilityCost)
        {
            abilities.StartCoroutine("AbilityCooldown");
            movement.Stamina -= (abilityCost);
            return true;
        }
        return false;
    }
}
