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
    float defaultSpeed = 20.0f;

    float speed;

    Vector3 dir;

    public override void UseAbility()
    {
        base.UseAbility();
        //instantiate a magic projectile
        GameObject magicProjectile = Instantiate
            (
                magicProjectilePrefab, transform.position, new Quaternion(cam.transform.rotation.x, movement.transform.rotation.y, cam.transform.rotation.z, movement.transform.rotation.w)
            );

        abilities.Anim.SetTrigger("MagicAttack");

        if (!camFollow.LockOn)
        {
            dir = magicProjectile.transform.forward + Vector3.up / 1.5f;
            speed = defaultSpeed;
        }
        else if (camFollow.LockOn)
        {
            dir = ((camFollow.LookAtMe.transform.position - magicProjectile.transform.position) / 10) + (Vector3.up / 12) * (Vector3.Distance(camFollow.LookAtMe.transform.position, magicProjectile.transform.position));
            speed = defaultSpeed / (Vector3.Distance(camFollow.LookAtMe.transform.position, magicProjectile.transform.position) / 5.5f);
        }

        //Add a force to the magic going forward form your current position.
        magicProjectile.GetComponent<Rigidbody>().AddForce(dir * speed, ForceMode.Impulse);
    }
}
