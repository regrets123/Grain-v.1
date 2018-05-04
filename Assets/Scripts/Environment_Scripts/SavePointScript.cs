using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class SavePointScript : MonoBehaviour, IInteractable
{
    public void Reskin(Material newMat)                 //Byter material på savepointen för att visa att den använts
    {
        GetComponent<Renderer>().material = newMat;
    }

    public void Interact(PlayerControls player)         //Sparar spelet då spelaren interagerar med scriptet
    {
        player.RestoreHealth(1000);
        player.Stamina = 1000;
        SaveManager saver = FindObjectOfType<SaveManager>();
        saver.SaveGame(this.gameObject);
    }
}
