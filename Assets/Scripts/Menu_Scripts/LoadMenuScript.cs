using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*By Björn Andersson*/

public class LoadMenuScript : MonoBehaviour
{
    [SerializeField]
    Button[] loadButtons;

    Sprite[] loadSprites;

    void Awake()
    {
        loadSprites = new Sprite[loadButtons.Length];
        for (int i = 0; i < loadSprites.Length; i++)                //Laddar in sprites för alla sparade spel så spelaren kan se vilket spel den vill ladda
        {
            if (File.Exists(Application.dataPath + "/SaveRef_" + i.ToString() + ".xml"))
            {
                XmlDocument refDoc = new XmlDocument();
                refDoc.Load(Application.dataPath + "/SaveRef_" + i.ToString() + ".xml");
                XPathNavigator xNav = refDoc.CreateNavigator();
                byte[] imageBytes = File.ReadAllBytes(xNav.SelectSingleNode("/ReferenceXML/SavedGame/@SpritePath").Value);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                tex.LoadImage(imageBytes);
                loadSprites[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.0f));
                loadButtons[i].GetComponentInChildren<Text>().text = "Load Saved Game " + (i + 1).ToString();
                loadButtons[i].GetComponent<Image>().sprite = loadSprites[i];
            }
            else
            {
                loadButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void LoadGame(int savedGame)  //Skapar en referensXML som indikerar att ett sparat spel ska laddas då spelscenen öppnas
    {
        TextAsset refText = Resources.Load("ReferenceXML") as TextAsset;
        XmlDocument refXML = new XmlDocument();
        refXML.LoadXml(refText.text);
        XPathNavigator xNav = refXML.CreateNavigator();
        xNav.SelectSingleNode("/ReferenceXML/SavedGame/@SpritePath").SetValue(Application.dataPath + "/SaveSprite_" + savedGame + ".png");
        xNav.SelectSingleNode("/ReferenceXML/SavedGame/@SavePath").SetValue(Application.dataPath + "/SavedGame_" + savedGame + ".xml");
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter writer = XmlWriter.Create(Application.dataPath + "/SaveToLoad.xml", settings);
        refXML.Save(writer);
        SceneManager.LoadScene("Master Scene 1 - Managers");
    }
}