using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkativeNpc : MonoBehaviour, SUPERCharacter.IInteractable {
    public string interactionName { get => "話す"; }

    public string[] conversation;

    public bool Interact() {
        GameState.EngageConversation(this, conversation, Vector3.up * 0.6f /* Hardcoded face position! */);
        return true;
    }
}
