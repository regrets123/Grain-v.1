using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class ClimbableScript : MonoBehaviour, IInteractable {

    string interactText = "PRESS SPACE TO CLIMB";

    [SerializeField]
    bool superClimb;

    [SerializeField]
    Transform finalClimbingPosition;

    public bool SuperClimb
    {
        get { return this.superClimb; }
    }

    public string GetText()
    {
        return this.interactText;
    }

    public Transform FinalClimbingPosition
    {
        get { return this.finalClimbingPosition; }
    }

    public void Interact(PlayerInteractions player)
    {
        return;
    }    
}
