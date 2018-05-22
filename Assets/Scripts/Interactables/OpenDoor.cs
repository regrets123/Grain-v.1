using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson && Andreas Nilsson*/

public class OpenDoor : MonoBehaviour, IInteractable
{
    [SerializeField]
    GameObject doorToOpen, leverPullPos;

    [SerializeField]
    float movePlayerSmoother;

    [SerializeField]
    AudioClip openGate;

    float leverPullPosX;

    Animator anim, animDoor;

    string interactText = "PRESS E TO INTERACT";

    PlayerInteractions playerToMove;

    MovementType previousMovement;


    void Start()
    {
        anim = this.GetComponent<Animator>();
        animDoor = doorToOpen.gameObject.GetComponent<Animator>();
    }

    public string GetText()
    {
        return this.interactText;
    }

    public void Interact(PlayerInteractions player)     //Spelar upp en animation medan spelaren drar i en spak för att öppna en dörr
    {
        playerToMove = player;
        StartCoroutine("MovePlayerToInteract");
        //player.InteractTime = 3f;
        anim.Play("LeverSwitch", 0);
        playerToMove.Anim.Play("Open Gate", 0);
        StartCoroutine("OpenSesame");
    }

    IEnumerator OpenSesame()        //Öppnar den relevanta dörren
    {
        yield return new WaitForSeconds(5.0f);
        SoundManager.instance.PlaySingle(openGate);
        playerToMove.GetComponent<PlayerMovement>().ChangeMovement("Previous");
        animDoor.SetTrigger("OpenDoor");
    }

    IEnumerator MovePlayerToInteract()      //Flyttar spelaren till rätt position medan animationen spelas
    {
        playerToMove.GetComponent<PlayerMovement>().ChangeMovement("None");
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / movePlayerSmoother;
        playerToMove.Anim.SetFloat("Speed", 0.5f);

            playerToMove.gameObject.transform.position = Vector3.Lerp(playerToMove.gameObject.transform.position, leverPullPos.gameObject.transform.position, t);
            playerToMove.gameObject.transform.rotation = Quaternion.Lerp(playerToMove.gameObject.transform.rotation, leverPullPos.gameObject.transform.rotation, t);

            yield return null;
        }

    }
}
