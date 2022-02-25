using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour, SUPERCharacter.IInteractable {
    public string title = "タイトルがない";
    public string text = "text goes here";

    public float grabTime { get => 3f; }

    public float susness = 0.3f;

    public string interactionName { get => "Interact"; }

    public bool Interact() {
        GameState.OpenNote(this);
        return true;
    }
}
