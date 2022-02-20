using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public enum States { Playing, InNote, Conversing, InInventory }

    public States current = States.Playing;

    public static GameState Instance = null;

    // sus's per second
    public float sprint_susness = 0.2f;
    public float note_susness = 0.3f;

    public bool winnable = false;
    public TalkativeNpc hitmanTarget = null;

    SUPERCharacter.SUPERCharacterAIO character;

    public List<TalkativeNpc> npcs;

    // States.InInventory
    public GameObject inventory_gui;

    // States.InNote
    public Note reading;
    public NoteGUI note_gui;

    // States.Conversing
    public Text conversation_text;
    string[] conversation;
    int conversation_index;
    TalkativeNpc talking_to;

    bool singleFrameLock = false;

    void Awake() {
        Instance = this;
        npcs = new List<TalkativeNpc>();

        if (!hitmanTarget) {
            Debug.LogError("Please set hitmanTarget to something");
            winnable = true;
        }
    }

    void Update() {
        if (singleFrameLock) {
            singleFrameLock = false;
            return;
        }

        switch (current) {
            case States.Conversing: {
                if (Input.GetKeyDown(KeyCode.E)) {
                    conversation_index++;
                    ContinueConversation();
                }
            } break;

            case States.InNote: {
                if (Input.GetKeyDown(KeyCode.E)) {
                    CloseNote();
                }
            } break;

            case States.InInventory: {
                if (Input.GetKeyDown(KeyCode.I)) {
                    CloseInventory();
                }
            } break;

            case States.Playing: {
                if (Input.GetKeyDown(KeyCode.I)) {
                    OpenInventory();
                }
            } break;
        }
    }

    void ContinueConversation() {
        if (conversation_index < conversation.Length) {
            conversation_text.text = conversation[conversation_index];
            conversation_text.gameObject.SetActive(true);
        } else {
            conversation_text.gameObject.SetActive(false);
            DisengageConversation();
        }
    }

    public static void SusSound(Vector3 pos, float distinctRadius, float indistinctRadius, Sussy.ActionKind action) {
        var state = Instance;

        // Find all the NPC:s within the radius that may hear the sound. Will scale with the npc:s hearing factor.
        foreach(var npc in state.npcs) {
            var earPos = npc.EarPosition();
            var delta = earPos - pos;
            var magSqr = delta.sqrMagnitude;

            if(magSqr < distinctRadius*distinctRadius) {
                // Audible
                npc.HearSusSound(pos, action);
            } else if(delta.sqrMagnitude < indistinctRadius*indistinctRadius) {
                npc.HearSusSound(pos, action);
            }
        }
    }

    public static void SusAction(Sussy.ActionKind action) {
        var pos = SUPERCharacter.SUPERCharacterAIO.Instance.eyePosition;
        var state = Instance;
        foreach(var npc in state.npcs) {
            if (npc.canSeePlayer) {
                npc.SeeSusAction(action);
            }
        }
    }

    public static void EngageConversation(TalkativeNpc npc, string[] conversation, Vector3 face_offset) {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;

        Debug.Assert(state != null);
        Debug.Assert(playerCamera != null);

        if(state.current != States.Playing) {
            Debug.LogError("Cannot engage a conversation in any other state than the Playing state");
            return;
        }

        playerCamera.EnterGUIMode();

        var position_delta = (npc.transform.position + face_offset) - playerCamera.transform.position;
        var look_towards = Quaternion.LookRotation(position_delta, Vector3.up);
        playerCamera.RotateView(look_towards.eulerAngles, true);

        state.current = States.Conversing;
        state.singleFrameLock = true;

        npc.state_stack.Add(npc.current);
        npc.current = TalkativeNpc.States.Talking;
        npc.target_angle = 180f + look_towards.eulerAngles.y;

        state.talking_to = npc;
        state.conversation = conversation;
        state.conversation_index = 0;
        state.ContinueConversation();
    }

    public static void DisengageConversation() {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;

        Debug.Assert(state != null);
        Debug.Assert(playerCamera != null);

        if(state.current != States.Conversing) {
            Debug.LogError("Cannot disengage conversation while not in a conversation");
            return;
        }

        playerCamera.ExitGUIMode();

        state.talking_to.current = state.talking_to.state_stack[state.talking_to.state_stack.Count - 1];
        state.talking_to.state_stack.RemoveAt(state.talking_to.state_stack.Count - 1);
        state.talking_to = null;
        state.current = States.Playing;
    }

    public static void OpenInventory() {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        
        if (state.current != States.Playing) {
            Debug.LogError("Cannot open inventory while not in the playing state");
            return;
        }

        playerCamera.EnterGUIMode();

        state.inventory_gui.SetActive(true);

        state.current = States.InInventory;
        state.singleFrameLock = true;
    }

    public static void CloseInventory() {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        
        if (state.current != States.InInventory) {
            Debug.LogError("Cannot close inventory while not in the InInventory state");
            return;
        }

        playerCamera.ExitGUIMode();

        state.inventory_gui.SetActive(false);

        state.current = States.Playing;
    }

    public static void OpenNote(Note note) {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        var noteGui = state.note_gui;

        Debug.Assert(state != null);
        Debug.Assert(noteGui != null);
        Debug.Assert(playerCamera != null);
        
        if (state.current != States.Playing) {
            Debug.LogError("Cannot open note while not in the playing state");
            return;
        }

        playerCamera.EnterGUIMode();

        var position_delta = note.transform.position - playerCamera.transform.position;
        var look_towards = Quaternion.LookRotation(position_delta, Vector3.up);
        playerCamera.RotateView(look_towards.eulerAngles, true);

        noteGui.title.text = note.title;
        noteGui.body.text = note.text;
        noteGui.gameObject.SetActive(true);
        
        state.current = States.InNote;
        state.reading = note;
        state.singleFrameLock = true;
    }

    public static void CloseNote() {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        var noteGui = state.note_gui;

        Debug.Assert(state != null);
        Debug.Assert(noteGui != null);
        Debug.Assert(playerCamera != null);
        if (state.current != States.InNote) {
            Debug.LogError("Cannot close note when note having a note opened");
            return;
        }

        noteGui.gameObject.SetActive(false);
        playerCamera.ExitGUIMode();

        state.current = States.Playing;
    }
}
