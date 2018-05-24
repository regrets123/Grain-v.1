using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class ConsoleScript : MonoBehaviour
{
    //Made awesomely by Björn Andersson && Andreas Nilsson

    #region Serialized

    [SerializeField]
    GameObject console;

    [SerializeField]
    AudioSource dejaVuMusic;

    [SerializeField]
    InputField consoleField;
    
    #endregion

    #region Non-Serialized

    private bool active = false, dejaVuActive = false, bigHead = false;

    PauseManager pauseManager;

    InputManager inputManager;

    GameObject head;

    VideoPlayer speedLines;

    UnityEvent sprint = new UnityEvent();

    #endregion

    #region Properties

    public UnityEvent Sprint
    {
        get { return this.sprint; }
    }

    #endregion

    #region Main Methods

    // Use this for initialization
    void Start()
    {
        pauseManager = GetComponent<PauseManager>();
        inputManager = GetComponent<InputManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ActivateConsole();
        }
        if (inputManager.CurrentInputMode == InputMode.Console)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                CheckCode();
            }
        }
        if (Input.GetButtonUp("Sprint") && dejaVuActive)
        {
            StopDejaVu();
        }
    }

    #endregion

    #region DejaVu

    void PlayDejaVu()
    {
        if (dejaVuMusic.isPlaying)
        {
            return;
        }
        dejaVuMusic.Play();
        speedLines.Play();
    }

    public void StopDejaVu()
    {
        if (!dejaVuActive)
            return;
        dejaVuMusic.Stop();
        speedLines.Stop();
    }

    #endregion

    void CheckCode()        //Fuskkoder. Shhh!!
    {
        switch (consoleField.text.ToUpper())
        {
            case "DEJAVU":
                speedLines = GameObject.Find("Speedlines").GetComponent<VideoPlayer>();
                dejaVuActive = !dejaVuActive;
                if (dejaVuActive)
                    sprint.AddListener(PlayDejaVu);
                else
                    sprint.RemoveListener(PlayDejaVu);
                break;

            case "KANYE":
                head = GameObject.FindGameObjectWithTag("Head");
                bigHead = !bigHead;
                if(bigHead)
                    head.transform.localScale = new Vector3(3, 3, 3);
                else
                    head.transform.localScale = new Vector3(1, 1, 1);
                break;

            case "CRAPMETAL":

                break;

            case "FREEFALLING":

                break;
        }
        consoleField.text = "";
        ActivateConsole();
    }

    void ActivateConsole()
    {
        if (inputManager.CurrentInputMode != InputMode.Playing && inputManager.CurrentInputMode != InputMode.Console)
            return;
        active = !active;
        console.SetActive(active);
        if (active)
            consoleField.ActivateInputField();
        else
            consoleField.DeactivateInputField();
        pauseManager.ActivateConsole(active);
    }


}
