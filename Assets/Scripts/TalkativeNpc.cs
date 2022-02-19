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
    Vector3 idle_moving_target;

    public float alertnessLevel = 0f;
    public float susLevel = 0f;
    public float susLowering = 0.1f;

    public Vector3 eye_offset = Vector2.up * 0.6f;

    public string interactionName { get => "話す"; }
    public string[] conversation;

    const float SIGHT_DISTANCE = 20f;
    const float SIGHT_SIZE = 0.3f;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        state_stack = new List<States>();

        target_angle = transform.rotation.eulerAngles.y;
    }

    void FixedUpdate() {
        var eyePos = transform.position + eye_offset;

        // 
        // - Sus level things
        //
        {
            susLevel -= susLowering * Time.fixedDeltaTime * susLowering;
            if (susLevel < 0f) susLevel = 0f;

            var state = GameState.Instance;

            var playerCamera = SUPERCharacter.SUPERCharacterAIO.Instance;
            Debug.Assert(playerCamera != null);
            var targetPos = playerCamera.transform.position;

            // It's a spherecast to make the seeing less precise
            int mask = 1 << 0;
            if(Physics.SphereCast(eyePos, SIGHT_SIZE, targetPos - eyePos, out var hit_info, SIGHT_DISTANCE, mask)) {
                var player = hit_info.transform.GetComponent<SUPERCharacter.SUPERCharacterAIO>();
                if (player != null) {
                    // We can see the player. Are they doing something sus?
                    if (player.isSprinting) susLevel += state.sprint_susness * Time.fixedDeltaTime;
                    if (state.current == GameState.States.InNote) {
                        susLevel += state.reading.susness * state.note_susness * Time.fixedDeltaTime;
                    }
                }
            }

            if (susLevel > 0.25f) {
                transform.rotation = Quaternion.LookRotation(targetPos - eyePos, Vector3.up);
            }
        }

        //
        // States
        //
        switch (current) {
            case States.Still: {
                still_timer -= Time.fixedDeltaTime;

                if (still_timer <= 0f) {
                    // Pick a random target point.
                    if (idleMovePointsParent != null && idleMovePointsParent.childCount > 0) {
                        Debug.Log("Trying to find a point to move towards");
                        var moving_towards = idleMovePointsParent.GetChild(Random.Range(0, idleMovePointsParent.childCount));

                        // Make sure we can see the point
                        var targetPos = moving_towards.transform.position;
                        // TODO: This mask should only include static objects
                        int mask = 1 << 0;
                        if(!Physics.SphereCast(eyePos, SIGHT_SIZE, targetPos - eyePos, out var _unused, (targetPos - eyePos).magnitude, mask)) {
                            Debug.Log("Found poitn to move towards");
                            // We can go here
                            idle_moving_target = targetPos;
                            current = States.IdleMoving;
                        } else {
                            Debug.Log("Point isn't visible");
                            still_timer = Random.Range(0.3f, 1f);
                        }
                    } else {
                        Debug.Log("No valid point to move towards found");
                        still_timer = Random.Range(1f, 5f);
                    }
                }
            } break;
            case States.IdleMoving: {
                Debug.Log("Moving");
                var error = idle_moving_target - transform.position;
                rb.AddForce(error.normalized * idle_moving_speed);

                target_angle = Mathf.Atan2(error.z, error.x);

                if (error.sqrMagnitude < idle_moving_acceptable_error*idle_moving_acceptable_error) {
                    Debug.Log("Stopped moving due to acceptable error");
                    still_timer = Random.Range(1f, 5f);
                    current = States.Still;
                }
            } break;
        }

        if (current == States.IdleMoving || current == States.Talking) {
            var angular_error = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target_angle);
            if (Mathf.Abs(angular_error) > 0.1f) {
                rb.AddTorque(0f, Mathf.Sign(angular_error) * rotation_speed, 0f);
            }
        }
    }

    public bool Interact() {
        GameState.EngageConversation(this, conversation, Vector3.up * 0.6f /* Hardcoded face position! */);
        return true;
    }
}
