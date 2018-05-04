using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class UpgradeScript : BaseEquippableObject
{
    [SerializeField]
    Upgrade upgrade;

    [SerializeField]
    string upgradeInfo;

    public Upgrade MyUpgrade
    {
        get { return this.upgrade; }
    }
}