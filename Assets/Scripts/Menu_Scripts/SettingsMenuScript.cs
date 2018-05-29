using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Xml;
using System.Xml.XPath;
using System.IO;

/*By Johanna Pettersson and Björn Andersson*/

public class SettingsMenuScript : MonoBehaviour
{
    [SerializeField]
    AudioMixer mainMixer;

    [SerializeField]
    Slider environmentalSlider, musicSlider, fxSlider, sensitivitySlider;

    [SerializeField]
    GameObject settingsMenu;

    CameraFollow camFollow;

    XmlDocument settingsXML;

    XPathNavigator xNav;

    float musicVolume, SFXVolume, environmentalVolume, camSensitivity, startingEnvironmental, startingMusic, startingFX, startingBrightness, startingSense;

    public void SetCam(GameObject cam)        //Ställer in alla settings från värden sparade i XML
    {
        camFollow = cam.GetComponent<CameraFollow>();
        if (File.Exists(Application.dataPath + "/Settings.xml"))
        {
            settingsXML = new XmlDocument();
            settingsXML.Load(Application.dataPath + "/Settings.xml");
            xNav = settingsXML.CreateNavigator();
            SetMusicVolume(float.Parse(xNav.SelectSingleNode("/Settings/Volumes/@Music").Value));
            startingMusic = musicVolume;
            musicSlider.value = musicVolume;
            SetEnvironmentalVolume(float.Parse(xNav.SelectSingleNode("/Settings/Volumes/@Environmental").Value));
            startingEnvironmental = environmentalVolume;
            environmentalSlider.value = environmentalVolume;
            SetSFXVolume(float.Parse(xNav.SelectSingleNode("/Settings/Volumes/@FX").Value));
            startingFX = SFXVolume;
            fxSlider.value = SFXVolume;
            sensitivitySlider.value = float.Parse(xNav.SelectSingleNode("/Settings/Camera/@Sensitivity").Value);
            camSensitivity = sensitivitySlider.value;
            startingSense = sensitivitySlider.value;
        }
        else        //Om inga värden sparats i XML skapas istället en virtuell XML att spara värden i
        {
            TextAsset newSettings = Resources.Load("SettingsXML") as TextAsset;
            settingsXML = new XmlDocument();
            settingsXML.LoadXml(newSettings.text);
            xNav = settingsXML.CreateNavigator();
        }
    }

    /* När vi ändrar ljudvolymen använder vi oss av mainmixern. */
    public void SetMusicVolume(float musicVolume)       //Ställer in musikvolymen
    {
        mainMixer.SetFloat("Music", musicVolume);
        this.musicVolume = musicVolume;
    }

    public void SetEnvironmentalVolume(float environmentalVolume)       //Ställer in volymen på alla miljörelaterade ljud
    {
        mainMixer.SetFloat("Environmental", environmentalVolume);
        this.environmentalVolume = environmentalVolume;
    }

    public void SetSFXVolume(float SFXVolume)
    {
        mainMixer.SetFloat("SFX", SFXVolume);
        this.SFXVolume = SFXVolume;
    }

    public void SetCamSensitivity(float sense)          //Ställer in hur snabbt kameran kan röra sig
    {
        camSensitivity = sense;
        if (camFollow == null)
            camFollow = FindObjectOfType<CameraFollow>();
        if (camFollow != null)
            camFollow.InputSensitivity = camSensitivity;
    }

    public void ApplySettings()                         //Bekräftar och sparar alla nya värden
    {
        GetComponent<AudioSource>().Play();
        startingSense = sensitivitySlider.value;
        startingMusic = musicVolume;
        startingFX = SFXVolume;
        startingEnvironmental = environmentalVolume;
        if (xNav == null)
        {
            settingsXML = new XmlDocument();
            if (File.Exists(Application.dataPath + "/Settings.xml"))
                settingsXML.Load(Application.dataPath + "/Settings.xml");
            else
            {
                TextAsset settingsText = Resources.Load("SettingsXML") as TextAsset;
                settingsXML.LoadXml(settingsText.text);
            }

            xNav = settingsXML.CreateNavigator();
        }
        xNav.SelectSingleNode("/Settings/Volumes/@Music").SetValue(musicVolume.ToString());
        xNav.SelectSingleNode("/Settings/Volumes/@Environmental").SetValue(environmentalVolume.ToString());
        xNav.SelectSingleNode("/Settings/Volumes/@FX").SetValue(SFXVolume.ToString());
        xNav.SelectSingleNode("/Settings/Camera/@Sensitivity").SetValue(sensitivitySlider.value.ToString());
        XmlWriterSettings writerSettings = new XmlWriterSettings();
        writerSettings.Indent = true;
        SetCamSensitivity(sensitivitySlider.value);
        using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/Settings.xml", writerSettings))
            settingsXML.Save(writer);
    }

    public void GoBack()                                //Avbryter alla temporära settingsförändringar och återställer dem till deras tidigare värden
    {
        SetSFXVolume(startingFX);
        SetMusicVolume(startingMusic);
        SetEnvironmentalVolume(startingEnvironmental);
        sensitivitySlider.value = startingSense;
        settingsMenu.SetActive(false);
    }
}
