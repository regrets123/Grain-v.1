﻿using System.Collections;
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
        abilities.StartCoroutine("AbilityCooldown");       //Startar en cooldown när spelaren använder en ability
    }
}
