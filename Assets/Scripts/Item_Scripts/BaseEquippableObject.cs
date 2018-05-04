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

    protected PlayerControls player;

    protected bool equipped = false;

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
        this.player = FindObjectOfType<PlayerControls>();
    }
}