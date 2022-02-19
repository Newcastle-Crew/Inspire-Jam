using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TalkativeNpc : MonoBehaviour, SUPERCharacter.IInteractable {
    public Transform idleMovePointsParent;
    Rigidbody rb;

    public enum States { Still, IdleMoving, Talking }

    public States current = States.Still;

    public List<States> state_stack;

    // States.IdleMoving | States.Talking
    public float rotation_speed = 1f;
    public float target_angle = 0f;

    // States.Still
    float still_timer = 1.3f;

    // States.IdleMoving
    public float idle_moving_speed = 1.0f;
    public float idle_moving_acceptable_error = 2f;
    Transform idle_moving_target;

    public int susLevel = Sussy.NONE;
    public bool knowsSusIdentity = false;
    public float susTimer = 0f;
    public float susTimeScalar = 1f;

    public Vector3 eye_offset = Vector2.up * 0.6f;

    public string interactionName { get => "話す"; }
    public string[] conversation;

    const float SIGHT_DISTANCE = 20f;
    const float SIGHT_SIZE = 0.3f;

    const float GLOBAL_SUS_TIME_SCALAR = 2f;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        state_stack = new List<States>();

        target_angle = transform.rotation.eulerAngles.y;
    }

    void Start() {
        var game_state = GameState.Instance;
        game_state.npcs.Add(this);
    }

    public Vector3 EarPosition() {
        return transform.position;
    }

    public void HearSusSound(int susLevel) {
        var newSusTime = (float)(susLevel *susLevel) * susTimeScalar * GLOBAL_SUS_TIME_SCALAR;
        if (this.susLevel > susLevel) {
            if (newSusTime > susTimer) susTimer = newSusTime;
            return;
        }

        this.susLevel = susLevel;
        if (susLevel == Sussy.MURDER) {
            susTimer = 1_000_000_000f;
        } else {
            susTimer = (float)(susLevel *susLevel) * susTimeScalar * GLOBAL_SUS_TIME_SCALAR;
        }
    }

    void FixedUpdate() {
        var eyePos = transform.position + eye_offset;
        bool useTargetAngle = false;

        // 
        // - Sus level things
        //
        {
            if (susLevel > Sussy.NONE) {
                susTimer -= Time.fixedDeltaTime;
                if (susTimer < 0f) {
                    susLevel = Sussy.NONE;
                }
            }
        }

        //
        // States
        //
        switch (current) {
            case States.Still: {
                still_timer -= Time.fixedDeltaTime;

                if (still_timer <= 0f) {
                    // TODO: This mask should only include static objects.
                    int mask = 1 << 0;

                    Transform FindGrazeTarget() {
                        if (idleMovePointsParent == null) return null;

                        var targets = new List<Transform>();
                        int childCount = idleMovePointsParent.childCount;
                        for (var i=0; i<childCount; i++) {
                            var child = idleMovePointsParent.GetChild(i);
                            if (!child.gameObject.activeSelf) continue;

                            var targetPos = child.position;
                            if(!Physics.SphereCast(eyePos, SIGHT_SIZE, targetPos - eyePos, out var _unused, (targetPos - eyePos).magnitude, mask)) {
                                targets.Add(child);
                            }
                        }

                        if (targets.Count == 0) return null;

                        var moving_towards_i = Random.Range(0, targets.Count);
                        return targets[moving_towards_i];
                    }

                    var moving_towards = FindGrazeTarget();
                    if (moving_towards != null) {
                        idle_moving_target = moving_towards;
                        moving_towards.gameObject.SetActive(false);
                        current = States.IdleMoving;
                    } else {
                        still_timer = Random.Range(2f, 4f);
                    }
                }
            } break;
            case States.Talking: {
                useTargetAngle = true;
            } break;
            case States.IdleMoving: {
                Debug.Log("Moving");
                var error = idle_moving_target.position - transform.position;
                rb.AddForce(error.normalized * idle_moving_speed);

                var look_rotation = Quaternion.LookRotation(error, Vector3.up);
                target_angle = look_rotation.eulerAngles.y;
                useTargetAngle = true;

                if (error.x*error.x+error.z*error.z < idle_moving_acceptable_error*idle_moving_acceptable_error) {
                    Debug.Log("Stopped moving due to acceptable error");
                    still_timer = Random.Range(1f, 5f);
                    idle_moving_target.gameObject.SetActive(true);
                    current = States.Still;
                }
            } break;
        }

        if (useTargetAngle) {
            // Debug.Log(target_angle);
            // transform.rotation = Quaternion.Euler(0f, target_angle, 0f);
            var angular_error = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target_angle);
            const float MAX_ANGULAR_ERROR = 10f;
            if (Mathf.Abs(angular_error) > MAX_ANGULAR_ERROR) {
                rb.AddTorque(0f, Mathf.Sign(angular_error) * rotation_speed, 0f);
            } else {
                rb.AddTorque(0f, angular_error / MAX_ANGULAR_ERROR * rotation_speed, 0f);
            }
        }
    }

    public bool Interact() {
        GameState.EngageConversation(this, conversation, Vector3.up * 0.6f /* Hardcoded face position! */);
        return true;
    }
}
