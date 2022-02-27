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

    bool has_key = false;

    public string interactionName { get => is_locked ? (has_key ? "Unlock" : "") : (open ? "Close" : "Open"); }
    public float grabTime { get => is_locked && has_key ? 2f : -1f; }

    public bool open = false;

    float interactedTime = -100f;
    const float timeDelay = 0.75f;

    public AudioSource lockedSound;
    public AudioSource openingSound;

    void Awake() {
        doorAnim = GetComponent<Animator>();
    }

    public bool CanInteract() {
        if (Time.time - interactedTime < timeDelay) return false;
        if (!is_locked) return true;

        var (_, thing) = InventoryHandler.FindItemByPredicate(item => item is Key key && key.color == color);
        if (!thing) GameState.Instance.PutMessage("Door is locked", 0.2f);
        has_key = thing;
        return true;
    }

    public bool Interact() {
        if (is_locked) {
            var (inventory_button, thing) = InventoryHandler.FindItemByPredicate(item => item is Key key && key.color == color);
            if (thing == null) {
                if (lockedSound) lockedSound.Play();
                return false;
            }

            if ((thing as Key).singleUse) {
                GameState.Instance.PutMessage("The key snapped!", 1.5f);
                Destroy(inventory_button.gameObject);
            }

            is_locked = false;
        }

        interactedTime = Time.time;
        open = !open;
        doorAnim.Play(open ? openAnimationName : closeAnimationName);
        if (openingSound) openingSound.Play();

        return true;
    }
}
