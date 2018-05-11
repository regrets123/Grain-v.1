﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*By Björn Andersson*/

public class PlayerInteractions : MonoBehaviour,IPausable {

    #region Non-Serialized Variables

    IInteractable currentInteractable;

    bool paused = false;

    Text interactText;

    float interactTime;

    InventoryManager inventory;

    Rigidbody rb;

    PlayerMovement movement;

    #endregion

    #region Properties

    public float InteractTime
    {
        set { this.interactTime = value; }
    }

    public InventoryManager Inventory
    {
        get { return this.inventory; }
    }

    #endregion

    #region Main Methods

    // Use this for initialization
    void Start () {
        //interactText = GameObject.Find();
        inventory = GetComponent<InventoryManager>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Interact") && currentInteractable != null && !paused)
        {
            currentInteractable.Interact(this);
            this.currentInteractable = null;
            rb.velocity = Vector3.zero;
            StartCoroutine("NonMovingInteract");
        }
	}

    #endregion

    #region Public Methods

    public void PauseMe(bool pausing)
    {
        paused = pausing;
    }

    #endregion

        #region Colliders
    void OnTriggerEnter(Collider other)         //Avgör vilken IIinteractable spelaren kan interagera med
    {
        if (other.gameObject.GetComponent<IInteractable>() != null)
            currentInteractable = other.gameObject.GetComponent<IInteractable>();
    }

    void OnTriggerExit(Collider other)
    {
        IInteractable otherInteractable = other.gameObject.GetComponent<IInteractable>();
        if (otherInteractable != null && currentInteractable == otherInteractable)
            currentInteractable = null;
    }

    #endregion

    #region Coroutines

    IEnumerator NonMovingInteract()             //Hindrar spelaren från att röra sig medan denne interagerar med något
    {
        movement.Interacting = true;
        yield return new WaitForSeconds(interactTime);
        movement.Interacting = false;
    }

    #endregion
}
