using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public class PlayerAbilities : MonoBehaviour, IPausable
{

    #region Serialized Variables

    [SerializeField]
    int maxLifeForce;

    [SerializeField]
    float abilityCooldown;

    [SerializeField]
    SpriteRenderer currentRune;

    [SerializeField]
    Transform abilityPos;

    #endregion

    #region Non-Serialized Variables

    BaseAbilityScript currentAbility;

    BaseItemScript currentItem;

    Slider lifeForceBar;

    int lifeForce;

    bool paused = true;

    InventoryManager inventory;

    Animator anim;

    InputManager inputManager;

    #endregion

    #region Properties

    public int LifeForce
    {
        get { return this.lifeForce; }
        set { this.lifeForce = value; lifeForceBar.value = lifeForce; }
    }

    public Slider LifeForceBar
    {
        get { return this.LifeForceBar; }
    }

    public Animator Anim
    {
        get { return this.anim; }
    }

    #endregion

    #region Main Methods

    private void Awake()
    {
        lifeForceBar = GameObject.Find("LifeForceSlider").GetComponent<Slider>();
        lifeForceBar.maxValue = maxLifeForce;
        lifeForceBar.value = lifeForce;
        FindObjectOfType<PauseManager>().Pausables.Add(this);
        inventory = GetComponent<InventoryManager>();
        this.anim = GetComponent<Animator>();
        inputManager = FindObjectOfType<InputManager>();
    }

    void Update()
    {
        if (inputManager.CurrentInputMode != InputMode.Playing)
            return;
        if ((Input.GetButtonDown("Ability") || Input.GetAxis("Ability") < 0f) && currentAbility != null && !BaseAbilityScript.CoolingDown)
        {
            print("ability pressed");
            currentAbility.UseAbility();
        }
        else if (Input.GetButtonDown("UseItem") && currentItem != null && !BaseItemScript.CoolingDown && !paused)
        {
            currentItem.UseItem();
        }
    }

    #endregion

    #region Public Methods

    public void ReceiveLifeForce(int value)         //Låter spelaren få lifeforce
    {
        this.lifeForce = Mathf.Clamp(this.lifeForce + value, 0, 100);
        lifeForceBar.value = lifeForce;
    }

    public void PauseMe(bool pausing)       //Ser till att spelaren
    {
        paused = !pausing;
    }

    public void EquipAbility(GameObject newAbility)
    {
        if (currentAbility != null)
            Destroy(currentAbility.gameObject);
        print("ability equipped");
        currentAbility = Instantiate(newAbility, abilityPos).GetComponent<BaseEquippableObject>() as BaseAbilityScript;
        currentRune.sprite = newAbility.GetComponent<BaseAbilityScript>().MyRune;
    }

    public void EquipItem(GameObject newItem)
    {
        if (currentItem != null)
            Destroy(currentAbility.gameObject);
        currentItem = Instantiate(newItem, abilityPos).GetComponent<BaseItemScript>();
        inventory.EquippedItemImage.sprite = newItem.GetComponent<BaseItemScript>().InventoryIcon;
    }

    #endregion

    #region Coroutines

    public IEnumerator AbilityCooldown()                //Hindrar spelaren från att använda abilities under en tid efter att en ability använts
    {
        BaseAbilityScript.CoolingDown = true;
        yield return new WaitForSeconds(abilityCooldown);
        BaseAbilityScript.CoolingDown = false;
    }

    #endregion
}
