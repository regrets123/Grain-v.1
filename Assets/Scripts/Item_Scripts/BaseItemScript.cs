using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class BaseItemScript : BaseEquippableObject
{

    protected static bool coolingDown = false;

    public static bool CoolingDown
    {
        get { return coolingDown; }
        set { coolingDown = value; }
    }

    public virtual void UseItem()
    {

    }

    public virtual void EquipItem()
    {

    }
}
