using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* By Johanna Pettersson */

public class TutorialScript : MonoBehaviour {
    
    public Text textTutorial;
    public string computerText;
    public string controllerText;

    // On trigger enter avslutar vi eventuellt föregående coroutine och startar den nya
    void OnTriggerEnter()
    {
        StopCoroutine(TextCountdown());
        StartCoroutine(TextCountdown());
        Destroy(this.gameObject, 6f);
    }

    // Aktiverar texten och väntar fem sec innan den inaktiveras
    public IEnumerator TextCountdown()
    { 
        if(Input.GetJoystickNames().Length > 0)
        {
            textTutorial.text = controllerText;
        }
        else
        {
        textTutorial.text = computerText;
        }
        textTutorial.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        textTutorial.gameObject.SetActive(false);
    }
}
