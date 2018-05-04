using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*By Johanna Pettersson*/

public class CreditsText : MonoBehaviour {

    public Image title;
    public bool isEndScreen;
    public GameObject mainMenu;
    public Canvas canvas;
    
    RectTransform canvasTransform;
    Vector3 textPosition;

    float scrollTime = 0.5f;

    private bool timeToScroll;

    void Awake()
    {
        canvasTransform = canvas.GetComponent<RectTransform>();
        textPosition = this.gameObject.transform.position;
    }

	// Update is called once per frame
	void Update () {
		
        if(this.gameObject.activeSelf == true)
        {
            StartCoroutine(FadeInText(title, 2f));
        }

        if(timeToScroll == true )
        {
            this.gameObject.transform.Translate(Vector3.up * scrollTime);
            if(this.gameObject.transform.position.y >= canvasTransform.rect.height /2 + 200)
            {
                Reset();
            }
        }

    }

    /* Resettar texten och tar tillbaka den till sin usprungliga position. Vi kontrollerar om det är endscreen eller inte för att veta var vi ska återvända. */

    public void Reset()
    {
        timeToScroll = false;
        this.gameObject.transform.position = textPosition;
        this.gameObject.transform.parent.gameObject.SetActive(false);
        if (isEndScreen)
        {
            SceneManager.LoadScene(0);
        }
        else if (!isEndScreen)
        {
        mainMenu.SetActive(true);
        }
    }

    /* Tonar in titeln och börjar därefter rulla upp texten. */

    public IEnumerator FadeInText(Image title, float t)
    {
        title.color = new Color(title.color.r, title.color.g, title.color.b, 0);
        while (title.color.a < 1.0f)
        {
            title.color = new Color(title.color.r, title.color.g, title.color.b, title.color.a + (Time.deltaTime / t));
            yield return null;
        }

        timeToScroll = true;
    }
}
