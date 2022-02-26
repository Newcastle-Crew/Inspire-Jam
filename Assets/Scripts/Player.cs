using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SUPERCharacter.SUPERCharacterAIO))]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public GenericSound fart_impulse;

    public bool inside_exit_point = false;
    public AudioSource susSoundTemp;
    SUPERCharacter.SUPERCharacterAIO cam;

    public enum ItemKind { None, Gun }
    
    public ItemKind holding = ItemKind.None;

    bool lost = false;

    void Awake() {
        Instance = this;
        cam = GetComponent<SUPERCharacter.SUPERCharacterAIO>();
    }

    public static void Equip(ItemKind holding) {
        Instance.holding = holding;
        Debug.Log("Equipped");
    }

    void Update() {
        if (lost) return;

        if(Input.GetKeyDown(KeyCode.K)) {
            // Do sussy sound.
            GameState.CreateImpulse(transform.position, fart_impulse);
            // GameState.SusSound(transform.position, 7f, 14f, Sussy.ActionKind.Fart);
            // GameState.SusAction(Sussy.ActionKind.Fart);
            if(susSoundTemp) susSoundTemp.Play();
        }

        if(Input.GetMouseButtonDown(0)) {
            switch (holding) {
                case ItemKind.Gun: {
                    // Shooting!
                    var pos = cam.eyePosition;
                    var rotation = cam.eyeRotation;
                    var delta = rotation * Vector3.forward;
                    Debug.Log(delta);

                    // Walls, Heads and Bodies
                    int mask = (1 << 3) | (1 << 6);
                    if (Physics.Raycast(pos, delta, out var hit_info, 100f, mask)) {
                        var hit_point = hit_info.point;
                        var layer = hit_info.transform.gameObject.layer;
                        var npc = hit_info.transform.GetComponentInParent<TalkativeNpc>();
                        if (npc) {
                            Debug.Log(hit_point.y);
                            Debug.Log(npc.transform.position.y);
                            if (hit_point.y > npc.transform.position.y + 1.4) {
                                // Hit head
                                Debug.Log("Headshot!");
                                npc.Shot(1f, hit_point, delta.normalized);
                            } else {
                                // Hit body
                                Debug.Log("Body shot!");
                                npc.Shot(0.2f, hit_point, delta.normalized);
                            }
                        } else {
                            // It might not have the npc component if you're shooting on an already dead corpse...
                        }
                    }
                } break;
            }
        }
    }

    int message_id;
    void OnTriggerExit(Collider other) {
        var exit_door = other.GetComponent<ExitDoor>();
        if (!exit_door) return;

        inside_exit_point = false;

        GameState.Instance.CloseMessage(message_id);
    }

    void OnTriggerEnter(Collider other) {
        var exit_door = other.GetComponent<ExitDoor>();
        if (!exit_door) return;

        inside_exit_point = true;

        if (!GameState.Instance.winnable) {
            message_id = GameState.Instance.PutMessage("Kill target before exiting");
            return;
        }

        WinGame();
    }

    bool game_ended = false;
    IEnumerator LoseGameCountdown() {
        GameState.Instance.PutMessage("Captured.", 5f);

        yield return new WaitForSeconds(2f);

        SUPERCharacter.SUPERCharacterAIO.Instance.EnterGUIMode();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoseGame() {
        if (game_ended) return;
        game_ended = true;
        lost = true;
        StartCoroutine("LoseGameCountdown");
    }

    IEnumerator WinGameCountdown() {
        GameState.Instance.PutMessage("Good work!", 5f);

        yield return new WaitForSeconds(1f);

        SUPERCharacter.SUPERCharacterAIO.Instance.EnterGUIMode();
        SceneManager.LoadScene("MainMenu");
    }

    public void WinGame() {
        // If you're already winning, don't win again!
        if (game_ended) return;
        game_ended = true;
        StartCoroutine("WinGameCountdown");
    }
}
