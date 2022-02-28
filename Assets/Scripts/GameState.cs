using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public enum States { Playing, InNote, Conversing, InInventory, Captured }

    public States current = States.Playing;

    public static GameState Instance = null;

    public float globalGuardSusness = 0f;

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
    public NoteItem reading;
    public NoteGUI note_gui;

    // States.Conversing
    public Text conversation_text;
    string[] conversation;
    int conversation_index;
    TalkativeNpc talking_to;
    public Talk talk_behaviour;

    // Messages
    float messageTimer = 0f;
    int currentMessageId = 0;
    public Text message;
    public GameObject messageBase;

    public GameObject interactBar;
    public Transform interactCompletionBar;

    public Text objectiveMessage;
    public int notesPickedUp = 0;
    public int numNotesNeededForObjective = 1;
    public string pickedUpNotesObjectiveMessage = "Kill target";
    public string killedTargetObjectiveMessage = "Escape";

    public GameObject interactIconBase;
    public Text interactNameText;

    bool singleFrameLock = false;

    void Awake() {
        Instance = this;
        npcs = new List<TalkativeNpc>();

        if (!hitmanTarget) {
            Debug.LogError("Please set hitmanTarget to something");
            winnable = true;
            objectiveMessage.text = killedTargetObjectiveMessage;
        }
    }

    void Update() {
        if (singleFrameLock) {
            singleFrameLock = false;
            return;
        }

        if (messageTimer > 0f) {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) {
                CloseMessage(currentMessageId);
            }
        }

        switch (current) {
            case States.Conversing: {
                if (Input.GetKeyDown(KeyCode.E)) {
                    conversation_index++;
                    ContinueConversation();
                }
            } break;

            case States.InNote: {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    CloseNote();
                }
            } break;

            case States.InInventory: {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    CloseInventory();
                }
            } break;

            case States.Playing: {
                if (Input.GetKeyDown(KeyCode.Tab)) {
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
            DisengageConversation();
        }
    }

    public static void CreateImpulse(Vector3 pos, Impulse impulse) {
        var state = Instance;

        foreach(var npc in state.npcs) {
            var info = new ImpulseInfo();
            info.pos = pos;
            info.audible = impulse.is_auditorial && (npc.EarPosition() - pos).sqrMagnitude < impulse.audio_radius * impulse.audio_radius;
            info.visible = impulse.is_visual && npc.CanSee(pos);

            if (!info.audible && !info.visible) continue;

            npc.GiveImpulse(info, impulse);
        }
    }

    public int PutMessage(string message, float time = -1f) {
        var state = Instance;
        state.message.text = message;
        state.messageBase.SetActive(true);
        state.messageTimer = time;

        return ++state.currentMessageId;
    }

    public void CloseMessage(int id) {
        var state = Instance;
        if (state.currentMessageId != id) return;

        state.messageBase.SetActive(false);
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

        npc.target_angle = 180f + look_towards.eulerAngles.y;

        state.talk_behaviour.wantsToExit = false;
        npc.SetBehaviour(state.talk_behaviour);

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

        state.conversation_text.gameObject.SetActive(false);
        state.talk_behaviour.wantsToExit = true;

        if(state.current != States.Conversing) {
            Debug.Log("Disengaged conversation while not in one.");
            return;
        }

        playerCamera.ExitGUIMode();

        state.talking_to = null;
        state.current = States.Playing;
    }

    public static void CapturePlayer() {
        if (Instance.current != States.Captured) {
            Player.Instance.LoseGame();
        }

        Instance.current = States.Captured;
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

    public static void OpenNote(NoteItem note) {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        var noteGui = state.note_gui;

        Debug.Assert(state != null);
        Debug.Assert(noteGui != null);
        Debug.Assert(playerCamera != null);
        
        if (state.current != States.InInventory) {
            Debug.LogError("Cannot open note while not in the inventory state");
            return;
        }

        noteGui.title.text = note.title;
        noteGui.body.text = string.Join("\n", note.bodyText);
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

        state.current = States.InInventory;
    }
}
