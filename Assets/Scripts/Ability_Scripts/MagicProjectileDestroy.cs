using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class MagicProjectileDestroy : MonoBehaviour
{
    protected IKillable targetToHit;

    [SerializeField]
    protected int damage;

    [SerializeField]
    DamageType dmgType;

    void Start()
    {
        Destroy(transform.parent.gameObject, 8);
    }

    public virtual void DealDamage(IKillable target)
    {
        target.TakeDamage(damage, dmgType);
    }

    void OnTriggerEnter(Collider other)         //Gör skada på en träffad fiende
    {
        targetToHit = other.gameObject.GetComponent<IKillable>();

        if (targetToHit != null && other.gameObject.tag != "Player")
        {
            DealDamage(targetToHit);
            Destroy(transform.parent.gameObject);
        }
        else if (other.gameObject.layer == 8)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
