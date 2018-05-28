using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*By Johanna Pettersson*/

public class MenuManager : MonoBehaviour
{
    InventoryManager iM;
    MainMenuScript mMS;

    [SerializeField]
    Color inactiveColor, activeColor;

    [SerializeField]
    Image inputImage;

    public void Start()
    {
        mMS = this.gameObject.GetComponent<MainMenuScript>();
    }
    
    public void ToggleMenu(GameObject menuToToggle)     //Visar och döljer menyer
    {
        menuToToggle.SetActive(!menuToToggle.activeSelf);
    }
    
    public void LoadScene(string sceneName)             //Laddar en vald scen
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void ExitApplication()                       //Avslutar spelet
    {
        Application.Quit();
    }

    public void Glow(Outline o)         //Får knappar att lysa
    {
        if (iM == null)
            iM = FindObjectOfType<InventoryManager>();
        if (iM.CurrentChoice != null)
            NoGlow(iM.CurrentChoice.GetComponent<Outline>());
        if (iM.CurrentUpgrade != null)
            NoGlow(iM.CurrentUpgrade.GetComponent<Outline>());
        o.enabled = true;
        if (iM.ItemSelected)
            return;
        if (iM.Upgrading)
        {
            iM.CurrentUpgrade = o.GetComponent<Button>();
            iM.UpgradeIndex = Array.IndexOf(iM.UpgradeButtons, o.GetComponent<Button>());
        }
        else
        {
            iM.CurrentChoice = o.GetComponent<Button>();
            iM.CollectionIndex = Array.IndexOf(iM.InventoryButtons, o.GetComponent<Button>());
        }
    }

    public void NoGlow(Outline o)       //Får knappar att sluta lysa
    {
        o.enabled = false;
    }

    public void ToggleColor(Text textToChange)      //Byter färg på textelement

    {
        if (textToChange.color == activeColor)
        {
            textToChange.color = inactiveColor;
        }
        else
        {
            textToChange.color = activeColor;
        }
    }

    public void CheckInput()
    {
        if (Input.GetJoystickNames().Length > 0)
        {
            print(Input.GetJoystickNames()[0]);
            inputImage.sprite = Resources.Load<Sprite>("Controller-Description-UI");
        }
        else
        {
            print("Hello Keyboard!");
            inputImage.sprite = Resources.Load<Sprite>("Keyboard-Controller-Descriptions");
        }
    }
    
}
