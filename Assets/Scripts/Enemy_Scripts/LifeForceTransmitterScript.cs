using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class LifeForceTransmitterScript : MonoBehaviour
{
    PlayerControls target;

    int lifeForceAmount;

    Vector3 startPos;

    float soulanimTime = 2f;

    bool soulAnimDone = false;

    Animator anim;

    float speed = 2f;

    public void StartMe(PlayerControls target, int lifeForceAmount, BaseEnemyScript spawner)     //Då det blev problem att hindra scriptet från att ärva från MonoBehaviour och använda en konstruktor agerar denna metod som en improviserad konstruktor
    {
        this.startPos = transform.position;
        this.target = target;
        this.lifeForceAmount = lifeForceAmount;
        this.transform.position = spawner.transform.position;
        anim = GetComponentInChildren<Animator>();
        anim.SetTrigger("SoulDrain");
        StartCoroutine("SoulAnim");
    }

    void Update() //Får fiendens själ att röra sig mot spelaren och ge spelaren lifeforce då den kommer fram
    {
        if (soulAnimDone)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, target.transform.position + new Vector3(0f, 0.5f, 0f), speed * Time.deltaTime);
            if (Vector3.Distance(gameObject.transform.position, target.transform.position + new Vector3(0f, 0.5f, 0f)) <= 0.1f)
            {
                this.target.ReceiveLifeForce(lifeForceAmount);
                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator SoulAnim()      //Spelar upp en animation då själen instantieras
    {
        yield return new WaitForSeconds(soulanimTime);
        soulAnimDone = true;
    }
}
