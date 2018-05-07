using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class GuardianMiniBoss : GuardianAI {

    [SerializeField]
    GameObject myDrop;

    protected override void Death()
    {
        base.Death();
        GameObject drop = Instantiate(myDrop, transform);        //Får minibossen att droppa en dash ability då den dör
        drop.transform.parent.DetachChildren();
    }
}
