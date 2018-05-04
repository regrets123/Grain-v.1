using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableScript : MonoBehaviour {

    [SerializeField]
    Transform finalClimbingPosition;

    [SerializeField]
    AnimationClip myAnim;

    public Transform FinalClimbingPosition
    {
        get { return this.finalClimbingPosition; }
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerControls player = other.gameObject.GetComponent<PlayerControls>();
        if (player != null)
        {
            player.CurrentClimbable = this;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        PlayerControls player = other.gameObject.GetComponent<PlayerControls>();
        if (player != null)
        {
            player.CurrentClimbable = null;
        }
    }

    public void Climb(PlayerControls player)
    {
        player.StartCoroutine("Climb",  myAnim);
        //player.gameObject.transform.position = finalClimbingPosition.position;
    }
}
