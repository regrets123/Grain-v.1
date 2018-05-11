using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class BaseEquippableObject : MonoBehaviour {     //Script som alla föremål som kan ligga i inventoryt ärver från

    [SerializeField]
    protected Sprite inventoryIcon;

    [SerializeField]
    protected string objectName;

    [SerializeField]
    protected EquipableType myType;

    protected bool equipped = false;

    protected PlayerCombat combat;

    protected PlayerMovement movement;

    protected PlayerAbilities abilities;

    public string ObjectName
    {
        get { return this.objectName; }
    }

    public EquipableType MyType
    {
        get { return this.myType; }
    }

    public Sprite InventoryIcon
    {
        get { return this.inventoryIcon; }
    }

    protected virtual void Start()
    {
        combat = FindObjectOfType<PlayerCombat>();
        movement = FindObjectOfType<PlayerMovement>();
        abilities = FindObjectOfType<PlayerAbilities>();
    }
}