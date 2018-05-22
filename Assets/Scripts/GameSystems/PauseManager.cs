using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*By Björn Andersson*/

//Interface som implementeras av allt som ska kunna pausas
public interface IPausable
{
    void PauseMe(bool pausing);
}

public class PauseManager : MonoBehaviour
{
    [SerializeField]
    GameObject pauseMenu, settingsMenu, deathScreen, confirmQuitMenu;

    [SerializeField]
    Button[] goBackButtons;

    InputManager iM;

    bool paused = false;

    //Lista av allt som kan pausas
    List<IPausable> pausables = new List<IPausable>();

    InputMode previousInputMode = InputMode.None;

    InventoryManager playerInventory;

    public List<IPausable> Pausables
    {
        get { return pausables; }
    }

    private void Awake()
    {
        iM = GetComponent<InputManager>();
        iM.SetInputMode(InputMode.Playing);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))      //Gör olika saker då spelaren trycker ner escape eller "back" på en handkontroll beroende på vilken meny som är aktiv
        {
            if (playerInventory == null)
            {
                playerInventory = FindObjectOfType<InventoryManager>();
            }
            foreach (Button goBackButton in goBackButtons)
            {
                if (goBackButton.gameObject.activeInHierarchy)
                {
                    goBackButton.onClick.Invoke();
                    return;
                }
            }
            PauseAndUnpause(false);
        }
    }

    public void PauseAndUnpause(bool inventory)    //Pausar/unpausar spelet och tar fram/döljer pausmenyn
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<InventoryManager>();
        if (iM == null)
            iM = GetComponent<InputManager>();
        paused = !paused;
        if (paused)
        {
            Time.timeScale = 0f;
            previousInputMode = iM.CurrentInputMode;
            if (!inventory)
                iM.SetInputMode(InputMode.Paused);
            else
                iM.SetInputMode(InputMode.Inventory);
        }
        else
        {
            Time.timeScale = 1f;
            iM.SetInputMode(previousInputMode);
        }
        if (!inventory)
        {
            pauseMenu.SetActive(paused);
        }
        else if (!paused)
        {
            playerInventory.HideInventory();
        }
        foreach (IPausable pauseMe in pausables)
        {
            if (pauseMe != null)
                pauseMe.PauseMe(paused);
        }
    }

    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }
}
