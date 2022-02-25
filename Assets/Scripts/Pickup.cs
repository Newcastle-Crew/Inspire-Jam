using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, SUPERCharacter.IInteractable {
    public string interactionName { get => "Pick up"; }
    public float grabTime { get => 2f; }

    public bool Interact() {
        return true;
    }
}
