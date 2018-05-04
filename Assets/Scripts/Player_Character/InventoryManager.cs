using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public enum EquipableType       //Indikerar vilken typ av föremål något är så att det kan läggas i rätt kollektion i inventoryt och att olika saker kan göras med föremålet, såsom att endast vapen kan uppgraderas
{
    Weapon, Ability, Consumable, ItemUpgrade
}

public class InventoryManager : MonoBehaviour
{
    GameObject inventoryMenu, upgradeOptions, equipButton, upgradeButton, favoriteButton, applyUpgradeButton, closeUpgradesButton;

    Image equippedWeaponImage, equippedAbilityImage, currentEquipableImage, currentUpgradeImage;

    List<GameObject> equippableWeapons, equippableAbilities, consumables, favoriteItems, itemUpgrades;

    List<GameObject>[] playerInventory = new List<GameObject>[4];

    PlayerControls player;

    InputManager inputManager;

    Button[] inventoryButtons = new Button[12];

    Button[] categoryButtons = new Button[4];

    Button[] upgradeButtons = new Button[8];

    PauseManager pM;

    int displayCollection = 0, collectionIndex = 0, upgradeIndex = 0;

    Sprite defaultIcon;

    Button currentChoice, currentCategory, currentUpgrade;

    MenuManager menuManager;

    Text equippableName, upgradeName, upgradeInfo;

    bool coolingDown = false, itemSelected = false, upgrading = false, equippingFavorite = false, upgradeSelected = false;

    public List<GameObject>[] PlayerInventory
    {
        get { return this.playerInventory; }
    }

    public bool Upgrading
    {
        get { return this.upgrading; }
    }

    public Button[] UpgradeButtons
    {
        get { return this.upgradeButtons; }
    }

    public Button CurrentUpgrade
    {
        get { return this.currentUpgrade; }
        set { this.currentUpgrade = value; }
    }

    public int UpgradeIndex
    {
        set { this.upgradeIndex = value; }
    }

    public bool ItemSelected
    {
        get { return this.itemSelected; }
    }

    public GameObject InventoryMenu
    {
        get { return this.inventoryMenu; }
    }

    public List<GameObject> EquippableAbilities
    {
        get { return this.equippableAbilities; }
    }

    public Button[] InventoryButtons
    {
        get { return this.inventoryButtons; }
    }

    public List<GameObject> EquippableWeapons
    {
        get { return this.equippableWeapons; }
    }

    public List<GameObject> Consumable
    {
        get { return this.consumables; }
    }

    public List<GameObject> ItemUpgrades
    {
        get { return this.itemUpgrades; }
    }

    public Button CurrentChoice
    {
        get { return this.currentChoice; }
        set { this.currentChoice = value; }
    }

    public int CollectionIndex
    {
        set { this.collectionIndex = value; }
    }

    public Image EquippedAbilityImage
    {
        get { return this.equippedAbilityImage; }
    }

    public Image EquippedWeaponImage
    {
        get { return this.equippedWeaponImage; }
    }

    private void Awake()        //Hittar alla objekt relevanta för inventoryt då det skapas. Eftersom spelaren instanssierar inventoryt och det inte ligger i scenen från början kan dessa värden inte serialiseras.
    {
        if (FindObjectOfType<PlayerControls>().Inventory != null)
        {
            Destroy(this);
        }
        menuManager = FindObjectOfType<MenuManager>();
        defaultIcon = Resources.Load<Sprite>("EmptySlot");
        this.player = FindObjectOfType<PlayerControls>();
        pM = FindObjectOfType<PauseManager>();
        inputManager = FindObjectOfType<InputManager>();
        playerInventory[0] = new List<GameObject>();
        playerInventory[1] = new List<GameObject>();
        playerInventory[2] = new List<GameObject>();
        playerInventory[3] = new List<GameObject>();
        itemUpgrades = new List<GameObject>();
        equippableWeapons = playerInventory[0];
        equippableAbilities = playerInventory[1];
        consumables = playerInventory[2];
        favoriteItems = playerInventory[3];
        inventoryMenu = GameObject.Find("InventoryMenu");
        upgradeOptions = GameObject.Find("UpgradeOptions");
        upgradeButton = GameObject.Find("UpgradeButton");
        closeUpgradesButton = GameObject.Find("CloseUpgradesButton");
        equipButton = GameObject.Find("EquipButton");
        favoriteButton = GameObject.Find("FavoriteButton");
        applyUpgradeButton = GameObject.Find("ApplyUpgradeButton");
        equippedAbilityImage = GameObject.Find("EquippedAbilityImage").GetComponent<Image>();
        equippedWeaponImage = GameObject.Find("EquippedWeaponImage").GetComponent<Image>();
        currentEquipableImage = GameObject.Find("Equipable Image").GetComponent<Image>();
        currentUpgradeImage = GameObject.Find("CurrentUpgradeImage").GetComponent<Image>();
        equippableName = GameObject.Find("Equipable Name").GetComponent<Text>();
        upgradeName = GameObject.Find("UpgradeName").GetComponent<Text>();
        upgradeInfo = GameObject.Find("UpgradeInfo").GetComponent<Text>();
        equippedAbilityImage.sprite = defaultIcon;
        equippedWeaponImage.sprite = defaultIcon;
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            inventoryButtons[i] = GameObject.Find("Slot " + (i + 1).ToString()).GetComponent<Button>();
        }
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i] = GameObject.Find("UpgradeSlot " + (i + 1).ToString()).GetComponent<Button>();
        }
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            categoryButtons[i] = GameObject.Find("Category " + (i + 1).ToString()).GetComponent<Button>();
        }
        currentCategory = categoryButtons[0];
        currentCategory.GetComponent<Outline>().enabled = true;
        closeUpgradesButton.SetActive(false);
        upgradeOptions.SetActive(false);
        inventoryMenu.SetActive(false);
        upgradeButton.SetActive(false);
        favoriteButton.SetActive(false);
        equipButton.SetActive(false);
        applyUpgradeButton.SetActive(false);
    }

    void Update() //se till att rätt saker händer när rätt knappar trycks på               
    {
        if (Input.GetButtonDown("Inventory") && inputManager.CurrentInputMode != InputMode.Paused)
        {
            if (inventoryMenu.activeSelf)           //Visar och döljer inventoryt
            {
                HideInventory();
            }
            else
            {
                ShowInventory();
            }
        }
        else if (inventoryMenu.activeSelf && inputManager.CurrentInputMode == InputMode.Inventory && !coolingDown && !upgrading)
        {
            if (!itemSelected)      //Låter spelaren navigera i inventoryt via handkontroll
            {
                if (Input.GetAxis("NextInventoryRow") < 0f)
                {
                    ChangeInventoryRow(true);
                }
                else if (Input.GetAxis("NextInventoryRow") > 0f)
                {
                    ChangeInventoryRow(false);
                }
                else if (Input.GetAxisRaw("NextItem") < 0f)
                {
                    HighlightNextItem(false);
                }
                else if (Input.GetAxisRaw("NextItem") > 0f)
                {
                    HighlightNextItem(true);
                }
                if (Input.GetButtonDown("PreviousInventoryCategory"))
                {
                    if (displayCollection == 0)
                    {
                        DisplayNewCollection(playerInventory.Length - 1);
                    }
                    else
                        DisplayNewCollection(displayCollection - 1);
                }
                else if (Input.GetButtonDown("NextInventoryCategory"))
                {
                    DisplayNewCollection((displayCollection + 1) % playerInventory.Length);
                }
            }
            if (itemSelected)
            {
                if (Input.GetButtonDown("Favorite"))
                {
                    favoriteButton.GetComponent<Button>().onClick.Invoke();
                }
                else if (Input.GetButtonDown("Upgrade"))
                {
                    upgradeButton.GetComponent<Button>().onClick.Invoke();
                }
                else if (Input.GetButtonDown("SelectItem"))
                {
                    equipButton.GetComponent<Button>().onClick.Invoke();
                }
                else if (Input.GetButtonDown("GoBack"))
                {
                    ShowUpgradeOptions(false);
                    upgradeButton.SetActive(false);
                    favoriteButton.SetActive(false);
                    equipButton.SetActive(false);
                    currentEquipableImage.sprite = defaultIcon;
                    equippableName.text = "";
                }
            }
            else if (Input.GetButtonDown("SelectItem") && !itemSelected)
            {
                currentChoice.onClick.Invoke();
            }
        }
        else if (upgrading && !coolingDown)
        {
            if (Input.GetButtonDown("SelectItem"))
            {
                if (!upgradeSelected)
                {
                    currentUpgrade.onClick.Invoke();
                    upgradeSelected = true;
                }
                else
                {
                    applyUpgradeButton.GetComponent<Button>().onClick.Invoke();
                    upgradeSelected = false;
                }
            }
            else if (Input.GetAxisRaw("NextItem") > 0f)
            {
                upgradeSelected = false;
                menuManager.NoGlow(currentUpgrade.GetComponent<Outline>());
                StartCoroutine("MenuCooldown");
                upgradeIndex = (upgradeIndex + 1) % upgradeButtons.Length;
            }
            else if (Input.GetAxisRaw("NextItem") < 0f)
            {
                upgradeSelected = false;
                menuManager.NoGlow(currentUpgrade.GetComponent<Outline>());
                StartCoroutine("MenuCooldown");
                if (upgradeIndex == 0)
                {
                    upgradeIndex = upgradeButtons.Length - 1;
                }
                else
                {
                    upgradeIndex--;
                }
            }
            else if (Input.GetAxisRaw("NextInventoryRow") != 0f)
            {
                upgradeSelected = false;
                menuManager.NoGlow(currentUpgrade.GetComponent<Outline>());
                StartCoroutine("MenuCooldown");
                int nextUpgradeIndex = 2;
                if (Input.GetAxisRaw("NextInventoryRow") > 0f)
                {
                    nextUpgradeIndex *= -1;
                }
                if (upgradeIndex + nextUpgradeIndex < 0)
                {
                    upgradeIndex = (upgradeButtons.Length) + (upgradeIndex + nextUpgradeIndex);
                }
                else if (upgradeIndex + nextUpgradeIndex > upgradeButtons.Length - 1)
                {
                    upgradeIndex = (upgradeIndex + nextUpgradeIndex) - (upgradeButtons.Length);
                }
                else
                {
                    upgradeIndex += nextUpgradeIndex;
                }
            }
            currentUpgrade = upgradeButtons[upgradeIndex];
            if (Input.GetButtonDown("GoBack"))
            {
                ShowUpgradeOptions(false);
            }
            UpdateSprites();
        }
        else if (!inventoryMenu.activeSelf && inputManager.CurrentInputMode == InputMode.Playing && !coolingDown)
        {
            if (!equippingFavorite)
            {
                bool controllerInput = Input.GetAxisRaw("NextInventoryRow") == 0f ? false : true;
                if (!controllerInput)
                    controllerInput = Input.GetAxisRaw("NextItem") == 0f ? false : true;
                if (Input.GetAxisRaw("NextInventoryRow") > 0f || Input.GetKeyDown(KeyCode.Alpha1))
                {
                    EquipFavorite(0, controllerInput);
                }
                else if (Input.GetAxisRaw("NextInventoryRow") < 0f || Input.GetKeyDown(KeyCode.Alpha3))
                {
                    EquipFavorite(2, controllerInput);
                }
                else if (Input.GetAxisRaw("NextItem") > 0f || Input.GetKeyDown(KeyCode.Alpha2))
                {
                    EquipFavorite(1, controllerInput);
                }
                else if (Input.GetAxisRaw("NextItem") < 0f || Input.GetKeyDown(KeyCode.Alpha4))
                {
                    EquipFavorite(3, controllerInput);
                }
            }
            if (Input.GetButtonDown("UnEquip"))
            {
                UnEquipWeapon();
            }
        }
        if (!coolingDown && (Input.GetKeyDown("r") || Input.GetAxis("Fire2") == -1f))      //Låter spelaren dra och stoppa undan det senast equippade vapnet
        {
            StartCoroutine(MenuCooldown());
            if (player.CurrentWeapon != null)
                player.Equip(null);
            else if (player.LastEquippedWeapon != null)
                player.Equip(player.LastEquippedWeapon);
        }
    }

    public string[] ReportAvailableUpgrades()
    {
        string[] upgradeNames = new string[itemUpgrades.Count];
        for(int i = 0; i < itemUpgrades.Count; i++)
        {
            upgradeNames[i] = itemUpgrades[i].GetComponent<UpgradeScript>().ObjectName;
        }
        return upgradeNames;
    }

    void UnEquipWeapon()        //Stoppar undan ett vapen
    {
        player.UnEquipWeapon();
    }

    public string[] ReportFavorites()       //Meddelar SaveManagern vilka föremål som finns bland spelarens favoriter
    {
        string[] allFavorites = new string[playerInventory[3].Count];
        for (int i = 0; i < playerInventory[3].Count; i++)
        {
            allFavorites[i] = playerInventory[3][i].GetComponent<BaseEquippableObject>().ObjectName;
        }
        return allFavorites;
    }

    void EquipFavorite(int favoriteIndex, bool controllerInput)     //Equippar ett av spelarens favoritföremål utan att behöva gå in i inventoryt
    {
        if (playerInventory[3] == null || playerInventory[3].Count <= favoriteIndex || playerInventory[3][favoriteIndex] == null)
            return;
        player.Equip(playerInventory[3][favoriteIndex]);
        StartCoroutine(DisplayEquippedFavorite(favoriteIndex));
        if (controllerInput)
            StartCoroutine("HighlightControllerInput");
    }

    IEnumerator DisplayEquippedFavorite(int favoriteIndex)
    {
        equippingFavorite = true;
        yield return new WaitForSeconds(2); //Lerpa bild in & ut
        equippingFavorite = false;
    }

    void UpgradeWeapon(bool upgrading)          //Visar och döljer alternativ för att uppgradera vapen
    {
        this.upgrading = upgrading;
        upgradeOptions.SetActive(upgrading);
        if (upgrading)
        {
            currentUpgrade = upgradeButtons[0];
        }
        else
        {
            currentChoice = inventoryButtons[collectionIndex];
        }
    }

    void ChangeInventoryRow(bool next)      //Går upp eller ner en rad i inventoryt då spelaren använder handkontroll
    {
        StartCoroutine("MenuCooldown");
        int nextRow = 4;
        if (!next)
        {
            nextRow *= -1;
        }
        if (collectionIndex + nextRow < 0)
        {
            collectionIndex = inventoryButtons.Length + (collectionIndex + nextRow);
        }
        else if (collectionIndex + nextRow >= inventoryButtons.Length)
        {
            collectionIndex = (collectionIndex + nextRow) - inventoryButtons.Length;
        }
        else
        {
            collectionIndex += nextRow;
        }
        menuManager.Glow(inventoryButtons[collectionIndex].GetComponent<Outline>());
    }

    void HighlightNextItem(bool next)       //Går ett steg åt höger eller vänster i inventoryt då spelaren använder handkontroll
    {
        StartCoroutine("MenuCooldown");
        if (next)
            collectionIndex = (collectionIndex + 1) % inventoryButtons.Length;
        else
        {
            if (collectionIndex == 0)
                collectionIndex = inventoryButtons.Length - 1;
            else
                collectionIndex--;
        }
        menuManager.Glow(inventoryButtons[collectionIndex].GetComponent<Outline>());
    }
    
    IEnumerator HighlightControllerInput()      //Postproduktion
    {
        yield return null;
    }

    IEnumerator MenuCooldown()          //Då handkontrollers D-pad använder sig av axlar snarare än knappar läggs denna cooldown in för att underlätta spelarens navigering i menyer med handkontroll
    {
        coolingDown = true;
        yield return new WaitForSecondsRealtime(0.15f);
        coolingDown = false;
    }

    void ShowInventory()                //Visar spelarens inventory
    {
        if (currentChoice == null)
        {
            currentChoice = inventoryButtons[0];
        }
        upgradeSelected = false;
        inventoryMenu.SetActive(true);
        equippableName.text = "";
        upgradeInfo.text = "";
        upgradeName.text = "";
        currentEquipableImage.sprite = defaultIcon;
        currentUpgradeImage.sprite = defaultIcon;
        pM.PauseAndUnpause(true);
        UpdateSprites();
        menuManager.Glow(currentChoice.GetComponent<Outline>());
    }

    public string[] ReportItems()           //Meddelar SaveManagern alla föremål som finns i spelarens inventory
    {
        string[] items = new string[playerInventory[0].Count + playerInventory[1].Count + playerInventory[2].Count];
        print(items.Length + " Items");
        int index = 0;
        for (int i = 0; i < playerInventory[0].Count; i++)
        {
            items[index] = playerInventory[0][i].GetComponent<BaseEquippableObject>().ObjectName;
            index++;
        }
        for (int i = 0; i < playerInventory[1].Count; i++)
        {
            items[index] = playerInventory[1][i].GetComponent<BaseEquippableObject>().ObjectName;
            index++;
        }
        for (int i = 0; i < playerInventory[2].Count; i++)
        {
            items[index] = playerInventory[2][i].GetComponent<BaseEquippableObject>().ObjectName;
            index++;
        }
        return items;
    }

    public string[] ReportWeaponNames()     //Meddelar SaveManagern alla vapen i spelarens inventory
    {
        string[] weaponNames = new string[equippableWeapons.Count];
        for (int i = 0; i < equippableWeapons.Count; i++)
        {
            weaponNames[i] = equippableWeapons[i].GetComponent<BaseEquippableObject>().ObjectName;
        }
        return weaponNames;
    }

    public string[][] ReportWeaponUpgrades()    //Meddelar SaveManagern vilka vapen som uppgraderats, vilken typ av uppgradering det är och vilken nivå uppgraderingen har
    {
        string[][] weaponUpgrades = new string[equippableWeapons.Count][];
        for (int i = 0; i < equippableWeapons.Count; i++)
        {
            weaponUpgrades[i] = new string[2];
            weaponUpgrades[i][0] = equippableWeapons[i].GetComponent<BaseEquippableObject>().ObjectName;
            weaponUpgrades[i][1] = equippableWeapons[i].GetComponent<BaseWeaponScript>().CurrentUpgrade.ToString() + equippableWeapons[i].GetComponent<BaseWeaponScript>().UpgradeLevel;
        }
        return weaponUpgrades;
    }

    public void DisplayNewCollection(int displayCollection)         //Väljer vilken av inventoryts kollektioner som ska visas
    {
        this.displayCollection = displayCollection;
        currentCategory.GetComponent<Outline>().enabled = false;
        currentCategory = categoryButtons[displayCollection];
        currentCategory.GetComponent<Outline>().enabled = true;
        if (itemSelected)
        {
            itemSelected = false;
            ShowItemOptions(false);
        }
        UpdateSprites();
    }

    void ShowItemOptions(bool show)         //Ger spelaren möjlighet att att equippa föremål, lägga till de bland favoriter och uppgradera dem om de är vapen
    {
        if (!show || playerInventory[displayCollection][collectionIndex].GetComponent<BaseEquippableObject>() is BaseWeaponScript)
        {
            upgradeButton.SetActive(show);
        }
        equipButton.SetActive(show);
        favoriteButton.SetActive(show);
    }

    void UpdateSprites()   //Uppdaterar visuellt menyn av föremål och förmågor som spelaren kan välja mellan
    {
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            if (i < playerInventory[displayCollection].Count && playerInventory[displayCollection][i] != null)
            {
                inventoryButtons[i].image.sprite = playerInventory[displayCollection][i].GetComponent<BaseEquippableObject>().InventoryIcon;
            }
            else
            {
                inventoryButtons[i].image.sprite = defaultIcon;
            }
        }
        if (upgradeOptions.activeSelf)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (itemUpgrades.Count > i && itemUpgrades[i] != null)
                {
                    upgradeButtons[i].image.sprite = itemUpgrades[i].GetComponent<BaseEquippableObject>().InventoryIcon;
                }
                else
                {
                    upgradeButtons[i].image.sprite = defaultIcon;
                }
            }
            menuManager.Glow(currentUpgrade.GetComponent<Outline>());
        }
    }

    public void ShowUpgradeOptions(bool show)       //Väljer om spelaren ska navigera mellan föremål i inventoryt eller tillgängliga uppgraderingar
    {
        itemSelected = show;
        upgradeOptions.SetActive(show);
        closeUpgradesButton.SetActive(show);
        this.upgrading = show;
        menuManager.NoGlow(currentChoice.GetComponent<Outline>());
        if (show)
        {
            currentUpgrade = upgradeButtons[0];
            menuManager.Glow(currentUpgrade.GetComponent<Outline>());
        }
        else
        {
            currentChoice = inventoryButtons[0];
            menuManager.Glow(currentChoice.GetComponent<Outline>());
        }
        UpdateSprites();
    }

    public void SelectItem(int index)       //Väljer ett föremål att equippa, uppgradera eller lägga till bland favoriter
    {
        if (playerInventory[displayCollection] == null || index >= playerInventory[displayCollection].Count || playerInventory[displayCollection] == null)
            return;
        collectionIndex = index;
        itemSelected = true;
        ShowItemOptions(true);
        equippableName.text = playerInventory[displayCollection][index].GetComponent<BaseEquippableObject>().ObjectName;
        currentEquipableImage.sprite = playerInventory[displayCollection][index].GetComponent<BaseEquippableObject>().InventoryIcon;
    }

    public void ApplyUpgrade()              //Uppgraderar ett valt vapen
    {
        playerInventory[0][collectionIndex].GetComponent<BaseWeaponScript>().ApplyUpgrade(itemUpgrades[upgradeIndex].GetComponent<UpgradeScript>().MyUpgrade/*, false*/);
        itemUpgrades.Remove(itemUpgrades[upgradeIndex]);
        UpdateSprites();
    }

    public void SelectUpgrade(int upgradeIndex)         //Väljer en uppgradering att lägga på ett vapen
    {
        this.upgradeIndex = upgradeIndex;
        equipButton.SetActive(false);
        favoriteButton.SetActive(false);
        applyUpgradeButton.SetActive(true);
    }


    public void Equip()    //Equippar ett föremål som finns i spelarens inventory
    {
        if (playerInventory[displayCollection] == null || collectionIndex > playerInventory[displayCollection].Count - 1 || playerInventory[displayCollection][collectionIndex] == null)
        {
            return;
        }
        player.Equip(playerInventory[displayCollection][collectionIndex]);
    }

    public void AddFavorite()   //Lägger till ett föremål bland spelarens favoriter
    {
        if (favoriteItems != null && favoriteItems.Count < 4)
        {
            foreach (GameObject favorite in favoriteItems)
            {
                if (favorite == playerInventory[displayCollection][collectionIndex])
                    return;
            }
            playerInventory[3].Add(playerInventory[displayCollection][collectionIndex]);
        }
    }

    public void AddFavorite(GameObject newFav)  //Samma som ovan, men tar emot ett föremål som argument
    {
        if (favoriteItems != null && favoriteItems.Count < 4)
        {
            foreach (GameObject favorite in favoriteItems)
            {
                if (favorite == playerInventory[displayCollection][collectionIndex])
                    return;
            }
            playerInventory[3].Add(newFav);
        }
    }

    public void AddUpgrade(GameObject newUpgrade)      //Lägger till en tillgänglig uppgradering i spelarens inventory
    {
        itemUpgrades.Add(newUpgrade);
    }

    public void HideInventory()    //Gömmer inventoryt
    {
        upgradeSelected = false;
        itemSelected = false;
        favoriteButton.SetActive(false);
        applyUpgradeButton.SetActive(false);
        ShowItemOptions(false);
        currentEquipableImage.sprite = defaultIcon;
        if (upgradeOptions.activeSelf)
        {
            upgradeOptions.SetActive(false);
        }
        pM.PauseAndUnpause(false);
        ShowUpgradeOptions(false);
        inventoryMenu.SetActive(false);
    }

    public void NewEquippable(GameObject equippable)    //Lägger till nya föremål i spelarens inventory
    {
        switch (equippable.GetComponent<BaseEquippableObject>().MyType)
        {
            case EquipableType.Weapon:
                AddEquippable(equippable, 0);
                break;

            case EquipableType.Ability:
                AddEquippable(equippable, 1);
                break;

            case EquipableType.Consumable:
                AddEquippable(equippable, 2);
                break;

            case EquipableType.ItemUpgrade:
                AddUpgrade(equippable);
                break;

            default:
                Debug.Log("trying to add nonspecified equippable, gör om gör rätt");
                break;
        }
    }

    public void SetWeaponUpgrade(string weaponName, string upgradeName, int upgradeLevel)       //Laddar in vapens uppgraderingar då ett sparat spel laddas
    {
        foreach (GameObject weapon in playerInventory[0])
        {
            BaseWeaponScript weaponScript = weapon.GetComponent<BaseWeaponScript>();
            if (weaponScript.ObjectName == weaponName)
            {
                Upgrade upgrade = Upgrade.DamageUpgrade;
                switch (upgradeName)
                {
                    case "LeechUpgrade":
                        upgrade = Upgrade.LeechUpgrade;
                        break;

                    case "FrostUpgrade":
                        upgrade = Upgrade.FireUpgrade;
                        break;

                    case "FireUpgrade":
                        upgrade = Upgrade.FireUpgrade;
                        break;
                }
                print(upgradeLevel);
                for (int i = 0; i < upgradeLevel; i++)
                {
                    weaponScript.ApplyUpgrade(upgrade);
                }
            }
        }
    }

    void AddEquippable(GameObject equippable, int collection)    //Lägger till equippable i rätt collection
    {
        playerInventory[collection].Add(equippable);
    }
}