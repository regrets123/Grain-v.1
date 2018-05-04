using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public interface IInteractable
{
    void Interact(PlayerControls player);
}

public class PickUpable : MonoBehaviour, IInteractable
{
    [SerializeField]
    GameObject item;

    public void Interact(PlayerControls player)     //Låter spelaren plocka upp ett föremål och lägga det i inventoryt
    {
        player.InteractTime = 2f;
        player.Anim.SetTrigger("PickUp");
        StartCoroutine("DetachItem");
        player.Inventory.NewEquippable(item);
        Destroy(this.gameObject, 2);
    }

    IEnumerator DetachItem()            //Tar bort föremålet från sin parent för att kunna ta bort föremålet från världen utan att hindra spelaren från att läggadet i sitt inventory
    {
        yield return new WaitForSeconds(1.9f);
        item.transform.parent = null;
        item.transform.position = Vector3.zero;
        item.transform.rotation = new Quaternion(0f, 0f, 0f, item.transform.rotation.w);
    }
}
