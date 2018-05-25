using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Björn Andersson*/

public class BaseEquippableObject : MonoBehaviour {     //Script som alla föremål som kan ligga i inventoryt ärver från

    [SerializeField]
    protected Sprite inventoryIcon;

    [SerializeField]
    protected string objectName, inventoryInfo;

    [SerializeField]
    protected EquipableType myType;

    protected bool equipped = false;

    protected PlayerCombat combat;

    protected PlayerMovement movement;

    protected PlayerAbilities abilities;

    protected Camera cam;

    protected CameraFollow camFollow;

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

    public virtual string InventoryInfo
    {
        get { return this.inventoryInfo; }
    }

    protected virtual void Start()
    {
        combat = FindObjectOfType<PlayerCombat>();
        movement = FindObjectOfType<PlayerMovement>();
        abilities = FindObjectOfType<PlayerAbilities>();
        cam = FindObjectOfType<Camera>();
        camFollow = FindObjectOfType<CameraFollow>();
    }
}