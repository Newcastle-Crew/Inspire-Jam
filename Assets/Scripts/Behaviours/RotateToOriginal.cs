using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToOriginal : Behaviour {
    float sourceAngle;

    void Awake() {
        sourceAngle = transform.rotation.eulerAngles.y;
    }

    public override void Run(TalkativeNpc self) {
        self.target_angle = sourceAngle;
    }
}
