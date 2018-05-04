using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public enum InputMode           //Håller koll på om spelet körs som vanligt, är pausat eller om spelaren navigerar i inventoryt för att kunna göra olika saker med samma inputs
{
    None, Playing, Paused, Inventory
}

public class InputManager : MonoBehaviour
{
    InputMode currentInputMode = InputMode.Playing;

    InventoryManager playerInventory;

    public InputMode CurrentInputMode
    {
        get { return currentInputMode; }
    }

    public InventoryManager PlayerInventory
    {
        set { if (this.playerInventory == null) this.playerInventory = value; }
    }

    private void Start()
    {
        //Locks cursor in the middle of the screen and hides it out of the way when game starts
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HideUpgrades()
    {
        playerInventory.ShowUpgradeOptions(false);
    }

    public void SetInputMode(InputMode newMode)
    {
        if (newMode != currentInputMode)
        {
            //If a menu of sorts is brought up like pause screen or inventory the cursor is visable and unlocked again. It's locked and becomes invisable if its the other way around
            currentInputMode = newMode;
            if (currentInputMode == InputMode.None || currentInputMode == InputMode.Paused || currentInputMode == InputMode.Inventory)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    public void ApplyUpgrade()      //Uppgraderar ett valt vapen med en vald uppgradering
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.ApplyUpgrade();
    }

    public void AddFavorite()       //Lägger till ett föremål bland spelarens favoriter för att kunna nå det via hotkeys
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.AddFavorite();
    }

    public void SelectUpgrade(int upgradeIndex)     //Väljer en uppgradering att lägga på ett vapen
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.SelectUpgrade(upgradeIndex);
    }

    public void SelectEquipable(int index)          //Väljer ett föremål i inventoryt för att sedan kunna equippa eller uppgradera det
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.SelectItem(index);
    }

    public void ViewInventoryCollection(int displayCollection)      //Väljer om spelarens vapen, abilities, favoriter eller övriga föremål ska visas i inventoryt
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.DisplayNewCollection(displayCollection);
    }

    public void Equip()             //Equippar ett vapen eller en ability
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.Equip();
    }

    public void ShowUpgradeOptions(bool show)       //Visar tillgängliga uppgraderingar för ett valt vapen
    {
        if (playerInventory == null)
            this.playerInventory = FindObjectOfType<InventoryManager>();
        playerInventory.ShowUpgradeOptions(show);
    }

    public void GoToMenu()              //Gör muspekaren tillgänglig då spelaren går till huvudmenyn
    {
        SetInputMode(InputMode.None);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
