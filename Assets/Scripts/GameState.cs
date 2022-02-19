using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    enum States { Playing, InNote, Conversing }

    States current = States.Playing;

    static GameState Instance = null;

    SUPERCharacter.SUPERCharacterAIO character;

    // States.Conversing
    public Text conversation_text;
    string[] conversation;
    int conversation_index;

    bool singleFrameLock = false;

    void Awake() {
        Instance = this;
    }

    void Update() {
        if (singleFrameLock) {
            singleFrameLock = false;
            return;
        }

        if (current == States.Conversing) {
            if (Input.GetKeyDown(KeyCode.E)) {
                conversation_index++;
                ContinueConversation();
            }
        }

        if (current == States.InNote) {
            if (Input.GetKeyDown(KeyCode.E)) {
                CloseNote();
            }
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

        state.current = States.Playing;
    }

    public static void OpenNote(Note note) {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        var noteGui = NoteGUI.Instance;

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
        state.singleFrameLock = true;
    }

    public static void CloseNote() {
        var state = Instance;
        var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
        var noteGui = NoteGUI.Instance;

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
