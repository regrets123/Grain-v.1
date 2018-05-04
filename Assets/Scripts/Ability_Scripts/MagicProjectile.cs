using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class MagicProjectile : BaseAbilityScript
{
    public GameObject magicProjectilePrefab;
    public Camera cameraPosition;

    //A currently public float, 
    //so you can adjust the speed as necessary.
    public float speed = 20.0f;

    public override void UseAbility()
    {
        base.UseAbility();
        //instantiate a magic projectile
        GameObject magicProjectile = Instantiate(magicProjectilePrefab, transform.position, transform.rotation);


        //Add a force to the magic going forward form your current position.
        magicProjectile.GetComponent<Rigidbody>().AddForce(magicProjectile.transform.forward * speed, ForceMode.Impulse);
    }
}
