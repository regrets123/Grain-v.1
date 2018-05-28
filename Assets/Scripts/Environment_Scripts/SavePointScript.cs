using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class SavePointScript : MonoBehaviour, IInteractable
{
    string interactText = "PRESS E TO SAVE GAME";

    string controllerInteractText = "PRESS A TO SAVE GAME";

    public void Reskin(Material newMat)                 //Byter material på savepointen för att visa att den använts
    {
        GetComponent<Renderer>().material = newMat;
    }

    public void Interact(PlayerInteractions player)         //Sparar spelet då spelaren interagerar med scriptet
    {
        player.GetComponent<PlayerCombat>().RestoreHealth(1000);
        player.GetComponent<PlayerMovement>().Stamina = 1000;
        SaveManager saver = FindObjectOfType<SaveManager>();
        saver.SaveGame(this.gameObject);
    }

    public string GetText()
    {
        return FindObjectOfType<MenuManager>().CheckInput() ? controllerInteractText : interactText;
    }
}
