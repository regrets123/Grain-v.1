using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class TrapDamageTrigger : MonoBehaviour
{
    [SerializeField]
    protected int damage;

    [SerializeField]
    DamageType dmgType;

    public virtual void DealDamage(IKillable target)
    {
        target.TakeDamage(damage, dmgType);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            DealDamage(other.gameObject.GetComponent<IKillable>());
        }
    }
}