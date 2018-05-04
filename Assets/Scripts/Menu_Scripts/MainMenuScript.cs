using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public enum ActiveMenu
{
    None, NewGame, LoadGame, Credits, Settings
}

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    Button[] mainMenuButtons, newGameButtons, loadGameButtons, goBackButtons;

    [SerializeField]
    Button applySettingsButton;

    [SerializeField]
    float mainMenuCooldown;

    [SerializeField]
    Slider[] settingsSliders;

    Button[] currentButtons;

    Button selectedButton;

    bool coolingDown = false;

    int currentIndex = 0;

    ActiveMenu activeMenu;

    AudioSource buttonSound;

    void Start()
    {
        currentButtons = mainMenuButtons;
        SelectNewButton(0);
        buttonSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!coolingDown)
        {
            if (Input.GetButtonDown("GoBack"))
            {
                foreach (Button goBackButton in goBackButtons)
                {
                    if (goBackButton.gameObject.activeInHierarchy)
                    {
                        goBackButton.onClick.Invoke();      //Går tillbaka om spelaren trycker på escape eller "back" på en handkontroll
                        break;
                    }
                }
            }
            else if (Input.GetButtonDown("SelectItem"))
            {
                selectedButton.onClick.Invoke();
            }
            else if ((Input.GetAxis("NextItem") != 0f || Input.GetAxis("NextInventoryRow") != 0f) && !coolingDown)      //Låter spelaren navigera i huvudmenyn via handkontroller
            {
                if (currentButtons != null || Input.GetAxis("NextInventoryRow") != 0f)
                    StartCoroutine("MenuCooldown");
                if (Input.GetAxis("NextItem") > 0f)
                {
                    if (activeMenu != ActiveMenu.Settings)
                        SelectNewButton((currentIndex + 1) % currentButtons.Length);
                    else
                    {
                        settingsSliders[currentIndex].value = Mathf.Clamp(settingsSliders[currentIndex].value + ((Mathf.Abs(settingsSliders[currentIndex].minValue) + Mathf.Abs(settingsSliders[currentIndex].maxValue)) / 50f), settingsSliders[currentIndex].minValue, settingsSliders[currentIndex].maxValue);
                    }
                }
                else if (Input.GetAxis("NextItem") < 0f)
                {
                    if (activeMenu != ActiveMenu.Settings)
                        SelectNewButton(currentIndex == 0 ? currentButtons.Length - 1 : currentIndex - 1);
                    else
                    {
                        settingsSliders[currentIndex].value = Mathf.Clamp(settingsSliders[currentIndex].value - ((Mathf.Abs(settingsSliders[currentIndex].minValue) + Mathf.Abs(settingsSliders[currentIndex].maxValue)) / 50f), settingsSliders[currentIndex].minValue, settingsSliders[currentIndex].maxValue);
                    }
                }
                else if (Input.GetAxis("NextInventoryRow") > 0f)
                {
                    if (activeMenu == ActiveMenu.None)
                        SelectNewButton(currentIndex < 2 ? currentButtons.Length - (currentIndex + 1) : currentIndex - 2);
                    else
                        currentIndex = currentIndex == 0 ? settingsSliders.Length - 1 : currentIndex - 1;
                }
                else if (Input.GetAxis("NextInventoryRow") < 0f)
                {
                    if (activeMenu == ActiveMenu.None)
                        SelectNewButton(currentIndex > 2 ? 4 - currentIndex : currentIndex + 2);
                    else
                        currentIndex = (currentIndex + 1) % settingsSliders.Length;
                }
            }
        }
    }

    public void NewMenu(int newIndex)       //Avgör vilken meny som ska visas och navigeras i
    {
        buttonSound.Play();
        currentIndex = 0;
        ActiveMenu newActiveMenu = (ActiveMenu)newIndex;
        this.activeMenu = (activeMenu == newActiveMenu ? ActiveMenu.None : newActiveMenu);
        switch (this.activeMenu)
        {
            case ActiveMenu.None:
                currentButtons = mainMenuButtons;
                break;

            case ActiveMenu.NewGame:
                currentButtons = newGameButtons;
                break;

            case ActiveMenu.LoadGame:
                currentButtons = loadGameButtons;
                break;

            case ActiveMenu.Settings:
                currentButtons = null;
                selectedButton.transform.localScale = new Vector3(1f, 1f, 1f);
                selectedButton = applySettingsButton;
                break;
        }
    }

    public void SelectNewButton(int newIndex)       //Avgör vilken knapp som är vald
    {
        if (activeMenu == ActiveMenu.Credits)
            return;
        Button newSelectedButton = currentButtons[newIndex];
        if (selectedButton != null)
            selectedButton.transform.localScale = new Vector3(1f, 1f, 1f);
        selectedButton = newSelectedButton;
        currentIndex = newIndex;
        selectedButton.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    IEnumerator MenuCooldown()      //Tillåter spelaren att navigera smidigt i huvudmenyn med handkontroll
    {   
        coolingDown = true;
        yield return new WaitForSecondsRealtime(mainMenuCooldown);
        coolingDown = false;
    }
}
