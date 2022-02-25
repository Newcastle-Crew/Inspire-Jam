using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brain {
    public float susLevel = 0f;
    public float stressLevel = 0f;
}

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class TalkativeNpc : MonoBehaviour, SUPERCharacter.IInteractable {
    public Transform idleMovePointsParent;
    public Rigidbody rb;
    public Brain brain;
    public float health = 1f;
    public float rotation_speed = 1f;
    public float target_angle = 0f;
    public float fov = 60f;
    public bool canSeePlayer = false;
    public Vector3 eye_offset = Vector2.up * 0.6f;
    public string interactionName { get => "Talk"; }
    public string[] conversation;

    public Behaviour defaultBehaviour;
    public Behaviour stressBehaviour;
    public Behaviour defaultImpulseHandler;
    public Behaviour susBehaviour;

    AudioSource audio_source;
    List<Behaviour> behaviourStack;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        audio_source = GetComponent<AudioSource>();

        target_angle = transform.rotation.eulerAngles.y;

        behaviourStack = new List<Behaviour>();
        brain = new Brain();
    }

    void Start() {
        var game_state = GameState.Instance;
        game_state.npcs.Add(this);
    }

    public Vector3 EarPosition() {
        return transform.position;
    }

    public Vector3 EyePosition() {
        return transform.position + eye_offset;
    }

    public void SetSusLevel(float susLevel) {
        this.brain.susLevel = Mathf.Max(this.brain.susLevel, susLevel);
    }

    public void Shot(float damage, Vector3 point, Vector3 dir) {
        var state = GameState.Instance;
        var player = Player.Instance;

        health -= damage;
        rb.AddForceAtPosition(dir * 20.3f + Vector3.up * 6f, point, ForceMode.Impulse);

        var impulse = new GenericSound();
        if (health <= 0f) {
            impulse.is_auditorial = false;

            if (state.hitmanTarget == this) {
                state.winnable = true;

                if (player.inside_exit_point) {
                    player.WinGame();
                }
            }

            rb.constraints = RigidbodyConstraints.None;
            GameState.Instance.npcs.Remove(this);
            Destroy(this);
        }

        impulse.susLevel = 4f;
        impulse.audio_radius = 40;
        GameState.CreateImpulse(transform.position, impulse);
    }

    public void SetBehaviour(Behaviour behaviour) {
        behaviour.wantsToExit = false;

        if (behaviourStack.Count > 0) {
            var previously_running = behaviourStack[behaviourStack.Count - 1];
            var continue_after_yield = previously_running.Yield();
            if (!continue_after_yield) {
                previously_running.Stop();
                behaviourStack.RemoveAt(behaviourStack.Count - 1);
            }
        }

        behaviourStack.Add(behaviour);
    }

    public void GiveImpulse(ImpulseInfo info, Impulse impulse) {
        Debug.Log("Got impulse");

        for (int i = this.behaviourStack.Count - 1; i >= 0; i--) {
            var behaviour = this.behaviourStack[i];
            if (behaviour.HandleImpulse(info, impulse)) {
                // If we handle the behaviour, we want to cut the rest of the behaviours.
                for (int i2 = this.behaviourStack.Count - 1; i2 > i; i2--) {
                    this.behaviourStack[i2].Stop();
                    this.behaviourStack.RemoveAt(i2);
                }

                return;
            }
        }

        // If it's not handled by any existing behaviour, add a new one
        {
            Debug.Log("Adding default handler");
            var behaviour = this.defaultImpulseHandler;
            var result = behaviour.HandleImpulse(info, impulse, is_source_impulse: true);
            Debug.Assert(result, "The default behaviour handler should handle any impulse");

            SetBehaviour(behaviour);
        }
    }

    void FixedUpdate() {
        var player_cam = SUPERCharacter.SUPERCharacterAIO.Instance;
        var player_pos = player_cam.eyePosition;
        var player = Player.Instance;

        if (brain.stressLevel > 0f) {
            brain.stressLevel -= Time.fixedDeltaTime * 0.2f;
        }

        canSeePlayer = CanSee(player_pos, !player_cam.isCrouching);
        if (canSeePlayer) {
            if (player.holding == Player.ItemKind.Gun) {
                SetSusLevel(4f);
            }

            // If they're suspicious, increase stress level!
            var oldStressLevel = brain.stressLevel;
            brain.stressLevel = Mathf.Max(brain.susLevel, brain.stressLevel);

            if (oldStressLevel < brain.stressLevel + 1f) {
                // TODO: Exclaim!
            }
        }

        if (behaviourStack.Count == 0) {
            if (susBehaviour && brain.susLevel > 1f && canSeePlayer) {
                SetBehaviour(susBehaviour);
            } else if (stressBehaviour && brain.stressLevel > 1f) {
                stressBehaviour.Run(this);
            } else if (defaultBehaviour) {
                defaultBehaviour.Run(this);
            }
        } else {
            var currentBehaviour = behaviourStack[behaviourStack.Count - 1];
            currentBehaviour.Run(this);
            if (currentBehaviour.wantsToExit) {
                behaviourStack.RemoveAt(behaviourStack.Count - 1);
            }
        }

        // TODO: What to do about this?
        var angular_error = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target_angle);
        const float MAX_ANGULAR_ERROR = 10f;
        if (Mathf.Abs(angular_error) > MAX_ANGULAR_ERROR) {
            rb.AddTorque(0f, Mathf.Sign(angular_error) * rotation_speed, 0f);
        } else {
            rb.AddTorque(0f, angular_error / MAX_ANGULAR_ERROR * rotation_speed, 0f);
        }
    }

    public bool CanSee(Vector3 point, bool big = false) {
        var eyePos = EyePosition();

        int mask = 1 << 3;
        const float SIGHT_SIZE = 0.25f;
        
        var look_rotation = Quaternion.LookRotation(point - eyePos, Vector3.up);
        var delta_angle = Mathf.DeltaAngle(look_rotation.eulerAngles.y, transform.rotation.eulerAngles.y);

        if (Mathf.Abs(delta_angle) > fov) return false;

        if (big) {
            // We try three different raycasts when you're standing up, so we don't miss the player if we should be seeing them
            for (var i=0; i<3; i++) {
                var delta = (point + Vector3.up * (float)i * 0.3f) - eyePos;
                if(!Physics.SphereCast(eyePos, SIGHT_SIZE, delta, out var _hit_info, delta.magnitude, mask)) {
                    return true;
                }
            }
        } else {
            var delta = point - eyePos;
            if(!Physics.SphereCast(eyePos, SIGHT_SIZE, delta, out var _hit_info, delta.magnitude, mask)) {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos() {

        var eyePos = EyePosition();
        var player = SUPERCharacter.SUPERCharacterAIO.Instance;
        if (!player) return;

        var player_pos = player.eyePosition;

        if (canSeePlayer) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(player_pos, eyePos);
        }
    }

    public bool Interact() {
        GameState.EngageConversation(this, conversation, Vector3.up * 0.6f /* Hardcoded face position! */);
        return true;
    }
}
