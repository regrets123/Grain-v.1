using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class MagicProjectile : BaseAbilityScript
{
    [SerializeField]
    GameObject magicProjectilePrefab;
    
    //A currently public float, 
    //so you can adjust the speed as necessary.

    [SerializeField]
    float speed = 20.0f;

    Vector3 dir;

    public override void UseAbility()
    {
        base.UseAbility();
        //instantiate a magic projectile
        GameObject magicProjectile = Instantiate
            (
                magicProjectilePrefab, transform.position, new Quaternion(camera.transform.rotation.x, movement.transform.rotation.y, camera.transform.rotation.z, movement.transform.rotation.w)
            );

        abilities.Anim.SetTrigger("MagicAttack");

        if (!camFollow.LockOn)
        {
            dir = magicProjectile.transform.forward + Vector3.up;

        }
        else if (camFollow.LockOn)
        {
            dir = ((camFollow.LookAtMe.transform.position - magicProjectile.transform.position)/10) + Vector3.up;
        }

        //Add a force to the magic going forward form your current position.
        magicProjectile.GetComponent<Rigidbody>().AddForce(dir * speed, ForceMode.Impulse);
    }
}
