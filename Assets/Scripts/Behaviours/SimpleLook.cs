using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLook : Behaviour {
    float turningTowardsAngle;
    
    public float lookTime = 0f;

    ImpulseInfo info;
    Impulse impulse;

    public AudioClip susSound;
    AudioClip normalSound;

    void Awake() {
        normalSound = this.stateChangeSound;
    }

    public override bool HandleImpulse(ImpulseInfo info, Impulse impulse, bool is_source_impulse = false) {
        if (!is_source_impulse) return false;

        this.stateChangeSound = impulse.susLevel > 1f ? susSound : normalSound;

        this.info = info;
        this.impulse = impulse;

        var diff = info.pos - transform.position;
        this.turningTowardsAngle = Quaternion.LookRotation(diff, Vector3.up).eulerAngles.y;
        this.wantsToExit = false;
        this.lookTime = 0f;
        return true;
    }

    public override void Run(TalkativeNpc self) {
        if (self.canSeePlayer && !info.visible) {
            const float MAX_DISTANCE_FOR_RELATION = 6f;
            if ((Player.Instance.transform.position - info.pos).sqrMagnitude < MAX_DISTANCE_FOR_RELATION*MAX_DISTANCE_FOR_RELATION) {
                self.SetSusLevel(impulse.susLevel);
            }
            info.visible = true;
        }

        var currentAngle = transform.rotation.eulerAngles.y;

        // TODO: I feel like this comparison could be made a lot simpler, maybe by comparing the quaternion components
        // directly somehow?
        if (Mathf.Abs(currentAngle - turningTowardsAngle) < 5.0f) {
            this.lookTime += Time.fixedDeltaTime;

            if (this.lookTime > 1f) this.wantsToExit = true;
        }

        self.target_angle = turningTowardsAngle;
    }

    public override bool Yield() {
        return false;
    }
}
