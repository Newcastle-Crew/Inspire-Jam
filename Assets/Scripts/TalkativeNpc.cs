using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class TalkativeNpc : MonoBehaviour, SUPERCharacter.IInteractable {
    public Transform idleMovePointsParent;
    Rigidbody rb;
    AudioSource audio_source;

    public enum States { Still, IdleMoving, Talking, InvestigatingSus }

    public States current = States.Still;

    public List<States> state_stack;

    public AudioClip[] repeated_sound_rude;
    public AudioClip[] repeated_sound_murder;

    public AudioClip[] fallback_attention_grab;
    public AudioClip[] fallback_rude;
    public AudioClip[] fallback_murder;

    public AudioClip[] hear_fart;
    public AudioClip[] seen_running;
    public AudioClip[] seen_reading;
    public AudioClip[] realize_culprit;

    public float susLevelSpeedIncrease = 0.4f;

    // States.InvestigatingSus
    public float target_moving_acceptable_error = 2f;
    public float target_moving_speed = 8f;

    // States.IdleMoving | States.Talking
    public float rotation_speed = 1f;
    public float target_angle = 0f;

    public float normalFov = 60f;
    public float engagedFov = 80f;

    // States.Still
    float still_timer = 1.3f;

    // States.IdleMoving
    public float idle_moving_speed = 1.0f;
    public float idle_moving_acceptable_error = 2f;
    Transform idle_moving_target;

    Vector3 susSoundPosition;

    public int susLevel = Sussy.NONE;
    public bool knowsSusIdentity = false;
    public float susTimer = 0f;
    public float susTimeScalar = 1f;
    float timeBeingSus = 0f;
    float timeSinceExclamation = 0f;
    float exclamationTimingRandom = 0f;

    public bool canSeePlayer = false;

    const float TIME_BEING_SUS_UNTIL_EXCLAMATION = 1.0f;

    public Vector3 eye_offset = Vector2.up * 0.6f;

    public string interactionName { get => "話す"; }
    public string[] conversation;

    const float SIGHT_DISTANCE = 20f;
    const float SIGHT_SIZE = 1.0f;

    const float GLOBAL_SUS_TIME_SCALAR = 2f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        audio_source = GetComponent<AudioSource>();

        state_stack = new List<States>();

        target_angle = transform.rotation.eulerAngles.y;
    }

    void Start() {
        var game_state = GameState.Instance;
        game_state.npcs.Add(this);
        exclamationTimingRandom = Random.value;
    }

    public Vector3 EarPosition() {
        return transform.position;
    }

    public Vector3 EyePosition() {
        return transform.position + eye_offset;
    }

    public void HearSusSound(Vector3 position, Sussy.ActionKind action) {
        int susLevel = Sussy.ActionToSus(action);

        var newSusTime = (float)(susLevel * susLevel) * susTimeScalar * GLOBAL_SUS_TIME_SCALAR;
        if (this.susLevel > susLevel) {
            UpdateSusness(newSusTime, knowsSusIdentity, this.susLevel, action);
            return;
        }

        // Gets distracted from what the player did, since it heard something worse.
        var new_knows_sus_identity = false;
        if (knowsSusIdentity && susLevel == this.susLevel) {
            new_knows_sus_identity = knowsSusIdentity;
        }

        susSoundPosition = position;
        UpdateSusness(newSusTime, new_knows_sus_identity, susLevel, action);
    }

    public void SeeSusAction(Sussy.ActionKind action) {
        int susLevel = Sussy.ActionToSus(action);
        var newSusTime = (float)(susLevel *susLevel) * susTimeScalar * GLOBAL_SUS_TIME_SCALAR;

        if (this.susLevel > susLevel) {
            UpdateSusness(newSusTime, knowsSusIdentity, this.susLevel, action);
            return;
        }

        UpdateSusness(newSusTime, true, susLevel, action);
    }

    void Exclaim(AudioClip[] clips) {
        if (clips.Length > 0) {
            var clip = clips[Random.Range(0, clips.Length)];
            audio_source.PlayOneShot(clip);
            timeSinceExclamation = 0f;
            exclamationTimingRandom = Random.value;
        } else {
            Debug.LogWarning("No sound clips");
        }
    }

    void UpdateSusness(float newSusTimer, bool newKnowsSusIdentity, int newSusLevel, Sussy.ActionKind action) {
        var increase = newSusLevel - susLevel;

        if (newSusLevel > Sussy.ATTENTION_GRAB) {
            current = States.InvestigatingSus;
        }

        susTimer = newSusTimer;
        susLevel = newSusLevel;

        bool big_surprise = increase > 2;
        if (!big_surprise && !knowsSusIdentity && newKnowsSusIdentity && timeBeingSus > TIME_BEING_SUS_UNTIL_EXCLAMATION) {
            Exclaim(realize_culprit);
        }
        knowsSusIdentity = newKnowsSusIdentity;

        if (big_surprise) {
            BigSurprise(action);
        } else if (increase > 0) {
            SmallSurprise(action);
        }
    }

    AudioClip[] GetActionSoundQueues(Sussy.ActionKind action) {
        AudioClip[] clips = new AudioClip[0];
        switch (action) {
            case Sussy.ActionKind.Run: {
                clips = seen_running;
            } break;
            case Sussy.ActionKind.Fart: {
                clips = hear_fart;
            } break;
            case Sussy.ActionKind.Reading: {
                clips = seen_reading;
            } break;
        }

        // Fallbacks, in case no sound was defined.
        switch (Sussy.ActionToSus(action)) {
            case Sussy.MURDER:
                if(clips.Length > 0) break;
                clips = fallback_murder;
                goto case Sussy.RUDE;
            case Sussy.RUDE:
                if(clips.Length > 0) break;
                clips = fallback_rude;
                goto case Sussy.ATTENTION_GRAB;
            case Sussy.ATTENTION_GRAB:
                if(clips.Length > 0) break;
                clips = fallback_attention_grab;
                break;
        }

        return clips;
    }

    void BigSurprise(Sussy.ActionKind action) {
        var sounds = GetActionSoundQueues(action);
        Exclaim(sounds);
    }

    void SmallSurprise(Sussy.ActionKind action) {
        if (timeSinceExclamation > 1.0f) {
            var sounds = GetActionSoundQueues(action);
            Exclaim(sounds);
        }
    }

    void FixedUpdate() {
        var state = GameState.Instance;
        var eyePos = EyePosition();
        var player = SUPERCharacter.SUPERCharacterAIO.Instance;
        var player_pos = player.eyePosition;
        bool useTargetAngle = false;

        timeSinceExclamation += Time.fixedDeltaTime;

        // Visibility
        {
            var delta = eyePos - player_pos;
            var magSqr = delta.sqrMagnitude;

            canSeePlayer = false;

            const float maxRadius = 50f;
            if(magSqr < maxRadius*maxRadius) {
                int mask = 1 << 3;
                const float SIGHT_SIZE = 0.25f;
                
                var look_rotation = Quaternion.LookRotation(delta, Vector3.up);
                var delta_angle = Mathf.DeltaAngle(look_rotation.eulerAngles.y, 180f + transform.rotation.eulerAngles.y);

                var fov = susLevel > Sussy.NONE ? engagedFov : normalFov;

                if (Mathf.Abs(delta_angle) <= fov) {
                    if (!player.isCrouching) {
                        // We try three different raycasts when you're standing up, so we don't miss the player if we should be seeing them
                        for (var i=0; i<3&&!canSeePlayer; i++) {
                            if(!Physics.SphereCast(player_pos + Vector3.up * (float)i * 0.3f, SIGHT_SIZE, delta, out var _hit_info, delta.magnitude, mask)) {
                                canSeePlayer = true;
                                Debug.Log("Can see player!!!!");
                            }
                        }
                    } else {
                        if(!Physics.SphereCast(player_pos, SIGHT_SIZE, delta, out var _hit_info, delta.magnitude, mask)) {
                            canSeePlayer = true;
                            Debug.Log("Can see player!!!!");
                        }
                    }
                }
            }
        }

        // 
        // - Sus level things
        //
        {
            if (susLevel > Sussy.NONE) {
                susTimer -= Time.fixedDeltaTime;
                timeBeingSus += Time.fixedDeltaTime;
                if (susTimer < 0f) {
                    susLevel = Sussy.NONE;
                }
            } else {
                timeBeingSus = 0f;
            }

            if (susLevel >= Sussy.RUDE) {
                if (timeSinceExclamation > exclamationTimingRandom * 2f + 1.4f) {
                    var sounds = new AudioClip[0];
                    if (susLevel == Sussy.MURDER) {
                        sounds = repeated_sound_murder;
                    }
                    if (sounds.Length == 0) sounds = repeated_sound_rude;
                    Exclaim(sounds);
                }
            }

            if (canSeePlayer && player.isSprinting) {
                SeeSusAction(Sussy.ActionKind.Run);
            }

            if (canSeePlayer && state.current == GameState.States.InNote) {
                SeeSusAction(Sussy.ActionKind.Reading);
            }
        }

        //
        // States
        //
        switch (current) {
            case States.InvestigatingSus: {
                if (susLevel == Sussy.NONE) {
                    current = States.Still;
                    still_timer = 0f;
                    break;
                }

                Vector3 sus_pos;
                if (knowsSusIdentity) sus_pos = player_pos;
                else sus_pos = susSoundPosition;

                var error = sus_pos - transform.position;
                var wanted_angle = Quaternion.LookRotation(error, Vector3.up);
                target_angle = wanted_angle.y;
                useTargetAngle = true;

                if (error.x*error.x+error.z*error.z > target_moving_acceptable_error*target_moving_acceptable_error) {
                    rb.AddForce(error.normalized * target_moving_speed);
                }
            } break;
            case States.Still: {
                still_timer -= Time.fixedDeltaTime * (1f + (float)susLevel);

                if (still_timer <= 0f) {
                    int mask = (1 << 3) | (1 << 0);

                    Transform FindGrazeTarget() {
                        if (idleMovePointsParent == null) return null;

                        var targets = new List<(float, Transform)>();
                        var probSum = 0f;
                        int childCount = idleMovePointsParent.childCount;
                        for (var i=0; i<childCount; i++) {
                            var child = idleMovePointsParent.GetChild(i);
                            if (!child.gameObject.activeSelf) continue;

                            var targetPos = child.position;
                            if(!Physics.SphereCast(eyePos, SIGHT_SIZE, targetPos - eyePos, out var _unused, (targetPos - eyePos).magnitude, mask)) {
                                float prob;
                                prob = 1f;
                                probSum += prob;

                                targets.Add((prob, child));
                            }
                        }

                        if (targets.Count == 0) return null;

                        var moving_towards_pick = Random.Range(0f, probSum);
                        foreach (var (prob, target) in targets) {
                            moving_towards_pick -= prob;
                            if (moving_towards_pick < 0f) return target;
                        }
                        
                        return null;
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
                var error = idle_moving_target.position - transform.position;
                rb.AddForce(error.normalized * idle_moving_speed);

                var look_rotation = Quaternion.LookRotation(error, Vector3.up);
                target_angle = look_rotation.eulerAngles.y;
                useTargetAngle = true;

                if (error.x*error.x+error.z*error.z < idle_moving_acceptable_error*idle_moving_acceptable_error) {
                    still_timer = Random.Range(1f, 5f);
                    idle_moving_target.gameObject.SetActive(true);
                    current = States.Still;
                }
            } break;
        }

        if (useTargetAngle) {
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
