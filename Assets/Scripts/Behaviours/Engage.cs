using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engage : Behaviour {
    public float speed = 8f;
    public float distanceForCapture = 2f;

    public Vector3 target_position;

    // 0 is walking, 1 is aiming, 2 is waiting for a after shooting.
    int state = 0;
    float stateTimer = 0f;
    public float walkTime = 1f;
    public float aimTime = 1f;
    public float waitTime = 1f;
    public float shot_position = 0f;

    float cantSeePlayerTime = 0f;
    public float max_time = 8f;

    public AudioClip loadingSound;
    public AudioClip fireSound;

    public override bool HandleImpulse(ImpulseInfo info, Impulse impulse, bool is_source_impulse = false) {
        // Engaging the player is probably one of the most hard-core thing the npc can do, there is no interrupting this.
        return true;
    }

    public override void Stop() {
        cantSeePlayerTime = 0f;
        state = 0;
        stateTimer = 0f;
    }

    public override void Run(TalkativeNpc self) {
        if (self.canSeePlayer) {
            target_position = Player.Instance.transform.position;
            cantSeePlayerTime = 0f;
        } else {
            cantSeePlayerTime += Time.fixedDeltaTime;
            if (cantSeePlayerTime > max_time) {
                wantsToExit = true;
            }
        }

        stateTimer += Time.fixedDeltaTime;

        switch (state) {
            case 0: {
                // Walking
                var delta = target_position - self.transform.position;

                var wanted_angle = Quaternion.LookRotation(delta, Vector3.up).eulerAngles.y;
                self.target_angle = wanted_angle;

                self.rb.AddForce(delta.normalized * speed);

                if (stateTimer > walkTime && self.canSeePlayer) {
                    state = 1;
                    stateTimer = 0f;

                    self.PlayAudio(loadingSound);
                }
            } break;

            case 1: {
                // Aiming
                var delta = target_position - self.transform.position;
                var wanted_angle = Quaternion.LookRotation(delta, Vector3.up).eulerAngles.y;
                self.target_angle = wanted_angle;

                if (stateTimer > aimTime) {
                    if (!self.canSeePlayer) {
                        state = 0;
                        stateTimer = 0f;
                    } else {
                        self.PlayAudio(fireSound);
                        GameState.CapturePlayer();
                        state = 2;
                        stateTimer = 0f;
                    }
                }
            } break;

            case 2: {
                // Waiting a bit after shooting.
                if (stateTimer > waitTime) {
                    state = 0;
                    stateTimer = 0f;
                }
            } break;
        }
    }

    public override bool Yield() {
        return false;
    }
}