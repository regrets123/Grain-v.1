using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public class PlayerAbilities : MonoBehaviour, IPausable {

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

    Slider lifeForceBar;

    int lifeForce;

    bool paused = true;

    InventoryManager inventory;

    Animator anim;

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
    }

    void Update () {
		if (Input.GetButtonDown("Ability") && currentAbility != null && !BaseAbilityScript.CoolingDown && paused)
        {
            currentAbility.UseAbility();
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
        currentAbility = Instantiate(newAbility, abilityPos).GetComponent<BaseEquippableObject>() as BaseAbilityScript;
        currentRune.sprite = newAbility.GetComponent<BaseAbilityScript>().MyRune;
        inventory.EquippedAbilityImage.sprite = newAbility.GetComponent<BaseAbilityScript>().InventoryIcon;
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
