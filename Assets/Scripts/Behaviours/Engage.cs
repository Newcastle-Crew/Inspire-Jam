using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engage : Behaviour {
    public float speed = 8f;
    public float distanceForCapture = 2f;

    public Vector3 target_position;

    public override bool HandleImpulse(ImpulseInfo info, Impulse impulse, bool is_source_impulse = false) {
        // Engaging the player is probably one of the most hard-core thing the npc can do, there is no interrupting this.
        return true;
    }

    public override void Run(TalkativeNpc self) {
        if (self.canSeePlayer) {
            target_position = Player.Instance.transform.position;
        }

        var delta = target_position - self.transform.position;

        var wanted_angle = Quaternion.LookRotation(delta, Vector3.up).eulerAngles.y;
        self.target_angle = wanted_angle;

        self.rb.AddForce(delta.normalized * speed);

        if (delta.sqrMagnitude < distanceForCapture * distanceForCapture) {
            if (self.canSeePlayer) {
                // Capture the player!
                GameState.CapturePlayer();
            } else {
                wantsToExit = true;
            }
        }
    }

    public override bool Yield() {
        return false;
    }
}