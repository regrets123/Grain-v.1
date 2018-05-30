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
        if (abilities.LifeForce >= abilityCost)
        {
            abilities.StartCoroutine("AbilityCooldown");       //Startar en cooldown när spelaren använder en ability
            abilities.LifeForce -= abilityCost;
            return true;
        }
        if (abilities.LifeForce + movement.Stamina >= abilityCost)
        {
            abilities.StartCoroutine("AbilityCooldown");
            movement.Stamina -= (abilityCost - abilities.LifeForce);
            abilities.LifeForce -= abilities.LifeForce;
            return true;
        }
        return false;
    }
}
