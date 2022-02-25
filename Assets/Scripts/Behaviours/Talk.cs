using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talk : Behaviour {
    public override bool HandleImpulse(ImpulseInfo info, Impulse impulse, bool is_source_impulse = false) {
        // We filter out unimportant events, because they don't matter.
        return impulse.importance < 2;
    }

    public override void Stop() {
        GameState.DisengageConversation();
    }

    public override void Run(TalkativeNpc self) {
        var wanted_angle = Quaternion.LookRotation(Player.Instance.transform.position - self.transform.position, Vector3.up).eulerAngles.y;
        self.target_angle = wanted_angle;
    }

    public override bool Yield() {
        return false;
    }
}
