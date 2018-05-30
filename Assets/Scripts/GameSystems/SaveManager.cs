using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using UnityEngine.SceneManagement;

/*By Björn Andersson*/

public class SavedGame  //Referensklass som håller koll på ett sparat spels och dess menysprites sökvägar
{
    string spritePath, savePath;

    public string SpritePath
    {
        get { return this.spritePath; }
    }

    public string SavePath
    {
        get { return this.savePath; }
    }

    public SavedGame(string spritePath, string savePath)
    {
        this.spritePath = spritePath;
        this.savePath = savePath;
    }
}

public class SaveManager : MonoBehaviour
{
    public bool testing;

    [SerializeField]
    int maxSaves;
        
    [SerializeField]
    GameObject camBase, playerPrefab, healthPotPrefab;

    [SerializeField]
    GameObject[] allItems;

    [SerializeField]
    Material saveMat;

    [SerializeField]
    GameObject[] savePoints;

    SavedGame currentSave;

    List<int> usedSavePoints = new List<int>();

    XmlDocument currentGame;

    XPathNavigator xNav;

    PlayerCombat combat;

    PlayerAbilities abilities;

    PlayerMovement movement;

    InventoryManager inventoryManager;

    InputManager inputManager;

    LoadingManager lM;

    GameObject myCam, player;

    private void Start()  //Startar spelet på olika sätt beroende på om det är ett nytt eller sparat spel
    {
        lM = FindObjectOfType<LoadingManager>();
        inputManager = GetComponent<InputManager>();
        currentGame = new XmlDocument();
        StartGame();
    }

    private void StartGame()
    {
        if (testing)
            return;
        if (!File.Exists(Application.dataPath + "/SaveToLoad.xml"))
        {
            TextAsset newGameText = Resources.Load("SavedStateXML") as TextAsset;
            currentGame.LoadXml(newGameText.text);
        }
        if (xNav == null)
            xNav = currentGame.CreateNavigator();
        LoadSavedScenes();
    }

    private void Update()           //Temporary AF Fuckers
    {
        if (inputManager.CurrentInputMode != InputMode.Playing)
            return;
        if (Input.GetKeyDown(KeyCode.H))
        {
            SaveGame(savePoints[0]);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            ReloadGame();
        }
    }

    //Metoder som används för att spara ett spel
    #region SaveMethods
    public void SaveGame(GameObject savePoint)      //Sparar spelet och byter material på den savepoint som använts för att indikera att den används
    {
        string spritePath = Screenshot();
        usedSavePoints.Add(Array.IndexOf(savePoints, savePoint));
        savePoint.GetComponent<SavePointScript>().Reskin(saveMat);
        GetInfoToSave();   //Matar in all info som ska sparas i den virtuella XML-filen
        if (currentSave == null)
        {
            string savePath = "";
            int saveNumber = 0;
            while (saveNumber < maxSaves)
            {
                if (!File.Exists(Application.dataPath + "/SavedGame_" + saveNumber.ToString() + ".xml"))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.CloseOutput = true;
                    // Save the document to a file and auto-indent the output.
                    savePath = Application.dataPath + "/SavedGame_" + saveNumber.ToString() + ".xml";
                    using (XmlWriter writer = XmlWriter.Create(savePath, settings))
                    {
                        currentGame.Save(writer);
                    }
                    XmlDocument referenceXML = new XmlDocument();
                    TextAsset saveReference = Resources.Load("ReferenceXML") as TextAsset;
                    referenceXML.LoadXml(saveReference.text);
                    XPathNavigator refNav = referenceXML.CreateNavigator();
                    refNav.SelectSingleNode("/ReferenceXML/SavedGame/@SpritePath").SetValue(spritePath);
                    refNav.SelectSingleNode("/ReferenceXML/SavedGame/@SavePath").SetValue(savePath);
                    using (XmlWriter refWriter = XmlWriter.Create(Application.dataPath + "/SaveRef_" + saveNumber + ".xml", settings))
                    {
                        referenceXML.Save(refWriter);
                    }
                    break;
                }
                saveNumber++;
            }
            currentSave = new SavedGame(spritePath, savePath);
        }
        else
        {
            currentGame.Save(currentSave.SavePath);
        }
    }

    void GetInfoToSave()        //Lagrar all relevant info i currentGame
    {
        SavePlayerTransform();
        SavePlayerResources();
        SaveCamTransform();
        SaveUsedSavePoints();
        SaveLoadedScenes();
        SaveInventory();
    }

    void SaveLoadedScenes()         //Sparar alla öppna scener i XML så dessa kan laddas in när ett spel laddas
    {
        XPathNavigator scenesNode = xNav.SelectSingleNode("/SavedState/LoadedScenes");
        XmlNodeList oldScenes = currentGame.SelectNodes("//Scene");
        if (oldScenes.Count > 0)
        {
            for (int i = oldScenes.Count - 1; i > -1; i--)
            {
                oldScenes[i].ParentNode.RemoveChild(oldScenes[i]);
            }
        }
        for (int i = 2; i < SceneManager.sceneCount; i++)       //De båda masterscenerna är alltid laddade, så dessa behöver inte sparas och där för börjar i på 2
        {
            scenesNode.AppendChild("<Scene Name=\"" + SceneManager.GetSceneAt(i).name + "\"/>");
        }
    }

    void SaveUsedSavePoints()       //Sparar alla använda savepoints så deras material kan bytas när ett sparat spel laddas
    {
        XPathNavigator savePointsNode = xNav.SelectSingleNode("/SavedState/UsedSavePoints");
        XmlNodeList oldSavePoints = currentGame.SelectNodes("//SavePoint");
        if (oldSavePoints.Count > 0)
            for (int i = oldSavePoints.Count - 1; i > -1; i--)
            {
                oldSavePoints[i].ParentNode.RemoveChild(oldSavePoints[i]);
            }
        foreach (int index in usedSavePoints)
        {
            savePointsNode.AppendChild("<SavePoint Index=\"" + index + "\"/>");
        }
    }

    void SavePlayerResources()      //Sparar spelarens resurser till XML
    {
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Resources/@Stamina").SetValue(movement.Stamina.ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Resources/@HP").SetValue(combat.Health.ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Resources/@LifeForce").SetValue(abilities.LifeForce.ToString());
    }

    void SaveCamTransform()         //Sparar kamerans position & rotation till XML
    {
        xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@X").SetValue(Math.Round(myCam.transform.position.x, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Y").SetValue(Math.Round(myCam.transform.position.y, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Z").SetValue(Math.Round(myCam.transform.position.z, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@X").SetValue(Math.Round(myCam.transform.rotation.x, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Y").SetValue(Math.Round(myCam.transform.rotation.y, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Z").SetValue(Math.Round(myCam.transform.rotation.z, 4).ToString());
        xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@W").SetValue(Math.Round(myCam.transform.rotation.w, 4).ToString());
    }

    void SaveInventory()        //Sparar alla föremål i spelarens inventory till XML
    {
        XPathNodeIterator nodes = xNav.Select("/SavedState/PlayerInfo/Inventory//Item/@Name");
        XPathNavigator inventory = xNav.SelectSingleNode("//Inventory");
        inventory.SelectSingleNode("//HealthPotions/@Number").SetValue(HealthPotion.MaxCharges.ToString());
        HealthPotion.RefillPots();
        foreach (string itemName in inventoryManager.ReportItems())
        {
            if (nodes.Count == 0)
            {
                inventory.AppendChild("<Item Name=\"" + itemName + "\"/>");
                continue;
            }
            foreach (XPathNavigator node in nodes)
            {
                if (itemName == node.Value)
                {
                    break;
                }
                else if (nodes.CurrentPosition == nodes.Count)
                {
                    inventory.AppendChild("<Item Name=\"" + itemName + "\"/>");
                }
            }
        }
        XPathNavigator favoritesNode = xNav.SelectSingleNode("//Favorites");
        XPathNodeIterator oldFavorites = favoritesNode.SelectChildren(XPathNodeType.All);
        XmlNodeList allOldFavorites = currentGame.SelectNodes("//Favorite");
        if (oldFavorites.Count > 0)
        {
            for (int i = allOldFavorites.Count - 1; i > -1; i--)
            {
                allOldFavorites[i].ParentNode.RemoveChild(allOldFavorites[i]);
            }
        }
        foreach (string favName in inventoryManager.ReportFavorites())
        {
            favoritesNode.AppendChild("<Favorite Name =\"" + favName + "\"/>");
        }
        nodes = xNav.Select("/SavedState/PlayerInfo/Inventory//Item/@Name");
        string[] weaponNames = inventoryManager.ReportWeaponNames();
        XPathNavigator upgradesNode = xNav.SelectSingleNode("//AppliedUpgrades");
        XmlNodeList oldAppliedUpgrades = currentGame.SelectNodes("//AppliedUpgrade");
        if (oldAppliedUpgrades.Count > 0)
        {
            for (int i = oldAppliedUpgrades.Count - 1; i > -1; i--)
            {
                oldAppliedUpgrades[i].ParentNode.RemoveChild(oldAppliedUpgrades[i]);    //Tar bort alla sparade upgrades från XML för att sedan kunna lägga in alla befintliga, för att undvika att spara dubletter eller redan använda upgrades
            }
        }
        upgradesNode = xNav.SelectSingleNode("//AppliedUpgrades");
        string[][] newUpgrades = inventoryManager.ReportWeaponUpgrades();
        int index = 0;
        foreach (string[] upgradeInfo in newUpgrades)
        {
            char upgradeLevel = upgradeInfo[1][upgradeInfo[1].Length - 1];
            string newName = upgradeInfo[1].Remove(upgradeInfo[1].Length - 1, 1);
            for (int i = index; i < weaponNames.Length; i++)
            {
                if (weaponNames[index] == newUpgrades[index][0])
                {
                    index++;
                    print("Saving upgrade level " + upgradeLevel);
                    upgradesNode.AppendChild("<AppliedUpgrade Weapon=\"" + newUpgrades[i][0] + "\" Name=\"" + newName + "\" Level=\"" + upgradeLevel + "\"/>");
                    break;
                }
            }
        }
        upgradesNode = xNav.SelectSingleNode("//AvailableUpgrades");
        XmlNodeList oldAvailableUpgrades = currentGame.SelectNodes("//AvailablUpgrade");
        if (oldAvailableUpgrades.Count > 0)
        {
            for (int i = oldAvailableUpgrades.Count - 1; i > -1; i--)
            {
                oldAvailableUpgrades[i].ParentNode.RemoveChild(oldAvailableUpgrades[i]);
            }
        }
        foreach (string upgradeName in inventoryManager.ReportAvailableUpgrades())
        {
            upgradesNode.AppendChild("<AvailableUpgrade Name=\"" + upgradeName + "\"/>");
        }
    }

    void SavePlayerTransform() //Sparar spelarens transform i XML
    {
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@X").SetValue(Math.Round(player.transform.position.x, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Y").SetValue(Math.Round(player.transform.position.y, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Z").SetValue(Math.Round(player.transform.position.z, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@X").SetValue(Math.Round(player.transform.rotation.x, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Y").SetValue(Math.Round(player.transform.rotation.y, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Z").SetValue(Math.Round(player.transform.rotation.z, 4).ToString());
        xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@W").SetValue(Math.Round(player.transform.rotation.w, 4).ToString());
    }

    string Screenshot() //Tar ett screenshot som sedan används som en sprite för sparfilen i loadmenyn
    {
        string path = Application.dataPath;
        if (currentSave != null)
        {
            path = currentSave.SpritePath;
        }
        else
        {
            int saveNumber = 0;
            while (saveNumber < maxSaves)
            {
                if (!File.Exists(Application.dataPath + "/SaveSprite_" + saveNumber.ToString() + ".png"))
                {
                    path += "/SaveSprite_" + saveNumber.ToString() + ".png";
                    break;
                }
                saveNumber++;
            }
        }
        ScreenCapture.CaptureScreenshot(path);
        return path;
    }
    #endregion

    //Metoder som används för att ladda ett sparat spel
    #region LoadMethods
    void LoadGame()    //Laddar ett sparat spel
    {
        if (currentSave == null)
        {
            XmlDocument pathHolder = new XmlDocument();
            pathHolder.Load(Application.dataPath + "/SaveToLoad.xml");
            this.currentSave = new SavedGame(pathHolder.SelectSingleNode("/ReferenceXML/SavedGame/@SpritePath").Value, pathHolder.SelectSingleNode("/ReferenceXML/SavedGame/@SavePath").Value);
            File.Delete(Application.dataPath + "/SaveToLoad.xml");
        }
        this.currentGame.Load(currentSave.SavePath);
        this.xNav = currentGame.CreateNavigator();
    }

    void LoadSavedScenes()          //Laddar in sparade scener
    {
        XPathNodeIterator savedScenes = xNav.Select("/SavedState/LoadedScenes/Scene/@Name");
        StartCoroutine(LoadingScenes(savedScenes));
    }

    IEnumerator LoadingScenes(XPathNodeIterator savedScenes)
    {
        bool loadGame = false;
        if (File.Exists(Application.dataPath + "/SaveToLoad.xml"))
        {
            loadGame = true;
            LoadGame();
        }
        foreach (XPathNavigator scene in savedScenes)
        {
            lM.LoadScene(scene.Value);
            yield return new WaitUntil(() => SceneLoaded() == true);
        }
        Vector3 newPos = new Vector3(float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Z").Value));
        Quaternion newRot = new Quaternion(float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Z").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@W").Value));
        Vector3 newCameraPos = new Vector3(float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Z").Value));
        Quaternion newCameraRot = new Quaternion(float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Z").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@W").Value));
        player = Instantiate(playerPrefab, newPos, newRot);
        myCam = Instantiate(camBase, newCameraPos, newCameraRot);
        abilities = player.GetComponent<PlayerAbilities>();
        movement = player.GetComponent<PlayerMovement>();
        combat = player.GetComponent<PlayerCombat>();
        inventoryManager = player.GetComponent<InventoryManager>();
        inputManager.PlayerInventory = inventoryManager;
        if (loadGame)
        {
            ReskinSavePoints();
            LoadInventory();
        }
        lM.LoadingScreen.SetActive(false);
        if (File.Exists(Application.dataPath + "/Settings.xml"))
        {
            XmlDocument settingsDoc = new XmlDocument();
            settingsDoc.Load(Application.dataPath + "/Settings.xml");
            SettingsMenuScript settings = FindObjectOfType<SettingsMenuScript>();
            settings.SetCam(myCam);
            settings.SetCamSensitivity(float.Parse(settingsDoc.SelectSingleNode("/Settings/Camera/@Sensitivity").Value));
        }
    }

    bool SceneLoaded()
    {
        return lM.Loaded;
    }

    void ReskinSavePoints()     //Ger använda checkpoints ett nytt material för att indikera att de använts
    {
        XPathNodeIterator nodes = xNav.Select("/SavedState/UsedSavePoints//SavePoint/@Index");
        foreach (XPathNavigator node in nodes)
        {
            savePoints[int.Parse(node.Value)].GetComponent<SavePointScript>().Reskin(saveMat);
        }
    }

    void LoadInventory()    //Laddar in spelarens sparade inventory
    {
        print("inventory loading");
        XPathNodeIterator nodes = xNav.Select("/SavedState/PlayerInfo/Inventory//Item/@Name");
        XPathNodeIterator favorites = xNav.Select("/SavedState/PlayerInfo/Inventory/Favorites//Favorite/@Name");
        XPathNodeIterator availableUpgrades = xNav.Select("/SavedState/PlayerInfo/Inventory/Upgrades/AvailableUpgrades//AvailableUpgrade/@Name");
        XPathNavigator pots = xNav.SelectSingleNode("//HealthPotions/@Number");
        if (pots.Value != "0")
        {
            inventoryManager.NewEquippable(healthPotPrefab);
            HealthPotion.SetNumberOfPots(int.Parse(pots.Value));
        }
        foreach (XPathNavigator node in nodes)
        {
            foreach (GameObject item in allItems)
            {
                if (node.Value == item.GetComponent<BaseEquippableObject>().ObjectName)
                {
                    print("item added");
                    GameObject newItem = Instantiate(item);
                    inventoryManager.NewEquippable(newItem);
                    while (favorites.MoveNext())
                    {
                        if (favorites.Current.Value == newItem.GetComponent<BaseEquippableObject>().ObjectName)
                        {
                            inventoryManager.AddFavorite(newItem);
                            break;
                        }
                    }
                }
            }
        }
        foreach (XPathNavigator upgrade in availableUpgrades)
        {
            foreach (GameObject upgradePrefab in allItems)
            {
                if (upgrade.Value == upgradePrefab.GetComponent<BaseEquippableObject>().ObjectName)
                {
                    inventoryManager.AddUpgrade(upgradePrefab);
                }
            }
        }
    }

    public void CheckIfUpgraded(BaseWeaponScript spawnedWeapon)
    {
        XPathNodeIterator appliedUpgrades = xNav.Select("/SavedState/PlayerInfo/Inventory/Upgrades/AppliedUpgrades//AppliedUpgrade");
        while (appliedUpgrades.MoveNext())
        {
            XPathNavigator thisUpgrade = appliedUpgrades.Current.CreateNavigator();
            string weaponToUpgrade = thisUpgrade.GetAttribute("Weapon", "");
            if (spawnedWeapon.ObjectName == weaponToUpgrade)
            {
                Upgrade newUpgrade = Upgrade.DamageUpgrade;
                switch (thisUpgrade.GetAttribute("Name", ""))
                {
                    case "FireUpgrade":
                        newUpgrade = Upgrade.FireUpgrade;
                        break;

                    case "FrostUpgrade":
                        newUpgrade = Upgrade.FrostUpgrade;
                        break;

                    case "LeechUpgrade":
                        newUpgrade = Upgrade.LeechUpgrade;
                        break;
                }
                int upgradeLvl = int.Parse(thisUpgrade.GetAttribute("Level", ""));
                for (int i = 0; i < upgradeLvl; i++)
                {
                    combat.CurrentWeapon.ApplyUpgrade(newUpgrade);
                }
                break;
            }
        }

    }

    public void ReloadGame()        //Laddar spelet från där det senast sparades; om spelet inte sparats får spelaren börja om från början. Används framförallt då spelaren dör.
    {
        if (currentSave != null)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            TextAsset xmlText = Resources.Load("ReferenceXML") as TextAsset;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText.text);
            XPathNavigator saveNav = doc.CreateNavigator();
            using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/SaveToLoad.xml", settings))
            {
                saveNav.SelectSingleNode("/ReferenceXML/SavedGame/@SpritePath").SetValue(currentSave.SpritePath);
                saveNav.SelectSingleNode("/ReferenceXML/SavedGame/@SavePath").SetValue(currentSave.SavePath);
                doc.Save(writer);
            }
        }
        inputManager.SetInputMode(InputMode.Playing);
        SceneManager.LoadScene("Master Scene 1 - Managers");
    }


    void MoveCamera()       //Flyttar kameran till en sparad position
    {
        Vector3 newCameraPos = new Vector3(float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Position/@Z").Value));
        Quaternion newCameraRot = new Quaternion(float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@Z").Value), float.Parse(xNav.SelectSingleNode("/SavedState/CameraTransform/Rotation/@W").Value));
        myCam.transform.position = newCameraPos;
        myCam.transform.rotation = newCameraRot;
    }

    IEnumerator MovePlayerWait() //Flyttar spelaren efter en viss tid för att ge banan tid att ladda in så spelaren inte faller genom marken innan marken existerar
    {
        yield return new WaitForSeconds(3);
        Vector3 newPos = new Vector3(float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Position/@Z").Value));
        Quaternion newRot = new Quaternion(float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@X").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Y").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@Z").Value), float.Parse(xNav.SelectSingleNode("/SavedState/PlayerInfo/Transform/Rotation/@W").Value));
        player.transform.position = newPos;
        player.transform.rotation = newRot;
    }
    #endregion
}