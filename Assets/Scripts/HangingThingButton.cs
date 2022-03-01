using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingThingButton : MonoBehaviour, SUPERCharacter.IInteractable, InteractSound {
    public string interactionName { get => "Release"; }
    public float grabTime { get => 5f; }

    public Rigidbody controlling;
    public BoxCollider kill_region;
    bool used = false;

    public AudioClip sound;
    public AudioClip i_sound { get => sound; }


    public bool CanInteract() => !used;
    public bool Interact() {
        controlling.constraints = RigidbodyConstraints.None;

        var bounds = kill_region.bounds;

        Debug.Log(kill_region.bounds.center);
        Debug.Log(kill_region.bounds.extents);
        var overlaps = Physics.OverlapBox(bounds.center, bounds.extents, kill_region.transform.rotation, (1 << 0));

        foreach (var overlap in overlaps) {
            var npc = overlap.GetComponent<TalkativeNpc>();
            if (!npc) continue;

            npc.Shot(100f, overlap.transform.position, new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized);
        }
        used = true;

        // TODO: Should we really make guards sus immediately?
        // GameState.Instance.globalGuardSusness = 1.5f;
        // Answer: No. Breaks the game, you get caught immediately.

        return true;
    }
}
