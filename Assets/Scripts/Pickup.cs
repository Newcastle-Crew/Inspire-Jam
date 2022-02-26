using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, SUPERCharacter.IInteractable {
    public string interactionName { get => "Pick up"; }
    public float grabTime { get => 2f; }

    public Item item;

    public bool Interact() {
        InventoryHandler.AddItem(item);

        // Urgh, c#!!!! I don't know how else to do this other than to add an extra item component to the thing, but that means that when we destroy the gameobject the item gets destroyed
        // too, and there's as far as I can tell no way to prevent that. Maybe a workaround would be to put the item stuff itself into another class that isn't the gameobject....

        gameObject.SetActive(false);

        return true;
    }
}