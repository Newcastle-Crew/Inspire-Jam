using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SUPERCharacter.SUPERCharacterAIO))]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public AudioSource susSoundTemp;
    SUPERCharacter.SUPERCharacterAIO cam;

    public enum ItemKind { None, Gun }

    public ItemKind holding = ItemKind.None;

    void Awake() {
        Instance = this;
        cam = GetComponent<SUPERCharacter.SUPERCharacterAIO>();
    }

    public static void Equip(ItemKind holding) {
        Instance.holding = holding;
        Debug.Log("Equipped");
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.K)) {
            // Do sussy sound.
            GameState.SusSound(transform.position, 7f, 14f, Sussy.ActionKind.Fart);
            GameState.SusAction(Sussy.ActionKind.Fart);
            if(susSoundTemp) susSoundTemp.Play();
        }

        if(Input.GetMouseButtonDown(0)) {
            switch (holding) {
                case ItemKind.None: {
                    // Try to interact with something
                    if (cam.interactHoveringOver != null)
                        cam.interactHoveringOver.Interact();
                } break;

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
                            Debug.Assert(layer == 3, "Nothing but an npc should have a head/body component");
                        }
                    }
                } break;
            }
        }
    }
}
