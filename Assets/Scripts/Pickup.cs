using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, SUPERCharacter.IInteractable, InteractSound 
{
    public string interactionName { get => "Pick up"; }
    public float grabTime { get => 2f; }

    public AudioClip sound;
    public AudioClip i_sound { get => sound; }

    public Item item;

    public bool CanInteract() => true;

    public bool Interact() {
        if (item is NoteItem note) {
            GameState.Instance.notesPickedUp += 1;

            if (GameState.Instance.notesPickedUp >= GameState.Instance.numNotesNeededForObjective) {
                // Don't update objective if already killed target
                if (!GameState.Instance.winnable) {
                    GameState.Instance.objectiveMessage.text = GameState.Instance.pickedUpNotesObjectiveMessage;
                }
            }
        }

        InventoryHandler.AddItem(item);

        // Urgh, c#!!!! I don't know how else to do this other than to add an extra item component to the thing, but that means that when we destroy the gameobject the item gets destroyed
        // too, and there's as far as I can tell no way to prevent that. Maybe a workaround would be to put the item stuff itself into another class that isn't the gameobject....

        gameObject.SetActive(false);

        GameState.Instance.PutMessage("Got " + item.name, 1.5f); // The UI will have a 'TAB to open inventory' prompt, so I removed the line that told players to press tab.

        return true;
    }
}
