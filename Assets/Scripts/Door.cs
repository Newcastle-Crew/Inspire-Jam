using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour, SUPERCharacter.IInteractable {
    Animator doorAnim;
    public string openAnimationName = "DoorOpen";
    public string closeAnimationName = "DoorClose";

    public bool is_locked = true;
    public Key.Color color = Key.Color.Red;

    public string interactionName { get => is_locked ? "Unlock" : (open ? "Close" : "Open"); }
    public float grabTime { get => is_locked ? 2f : -1f; }

    public bool open = false;

    float interactedTime = -100f;
    const float timeDelay = 0.75f;

    void Awake() {
        doorAnim = GetComponent<Animator>();
    }

    public bool CanInteract() {
        if (Time.time - interactedTime < timeDelay) return false;
        if (!is_locked) return true;

        var (_, thing) = InventoryHandler.FindItemByPredicate(item => item is Key key && key.color == color);
        if (!thing) GameState.Instance.PutMessage("Door is locked", 0.2f);
        return thing != null;
    }

    public bool Interact() {
        if (is_locked) {
            var (inventory_button, thing) = InventoryHandler.FindItemByPredicate(item => item is Key key && key.color == color);
            if (thing == null) return false;
            Destroy(inventory_button.gameObject);

            is_locked = false;
        }

        interactedTime = Time.time;
        open = !open;
        doorAnim.Play(open ? openAnimationName : closeAnimationName);

        return true;
    }
}
