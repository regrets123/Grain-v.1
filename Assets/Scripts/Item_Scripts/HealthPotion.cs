using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class HealthPotion : BaseItemScript
{
    [SerializeField]
    int amount;

    static int currentCharges = 0, maxCharges = 0;

    public static int MaxCharges
    {
        get { return maxCharges; }
    }

    //Restore health to the player based on the amount the potion will give
    public override void UseItem()
    {
        if (currentCharges <= 0 && !coolingDown)
            return;
        base.UseItem();
        currentCharges--;
        //spela animation, partiklar eller w/e
        combat.RestoreHealth(amount);
    }

    public static void RefillPots()
    {
        currentCharges = maxCharges;
    }

    public static void SetNumberOfPots(int maxPots)
    {
        currentCharges = maxPots;
        maxCharges = maxPots;
    }

    public static void AddPot(int noOfPots)
    {
        maxCharges += noOfPots;
        currentCharges += noOfPots;
    }
}
