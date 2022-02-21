using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : Behaviour
{
    enum States { Still, Walking }

    States current = States.Still;
    float still_timer = 1f;

    Transform target;
    public Transform targetPoints;
    public float speed = 8f;

    const float SIGHT_SIZE = 0.3f;
    const float ACCEPTABLE_ERROR = 0.7f;

    void Awake() {
        still_timer = Random.Range(1f, 5f);
    }

    public override void Run(TalkativeNpc self) {
        var eyePos = self.EyePosition();

        switch (current) {
            case States.Still: {
                still_timer -= Time.fixedDeltaTime;

                if (still_timer <= 0f) {
                    int mask = (1 << 3);

                    Transform FindGrazeTarget() {
                        if (targetPoints == null) return null;

                        var targets = new List<(float, Transform)>();
                        var probSum = 0f;
                        int childCount = targetPoints.childCount;
                        for (var i=0; i<childCount; i++) {
                            var child = targetPoints.GetChild(i);
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
                        target = moving_towards;
                        target.gameObject.SetActive(false);
                        current = States.Walking;
                    } else {
                        still_timer = Random.Range(2f, 4f);
                    }
                }
            } break;
            case States.Walking: {
                var error = target.position - transform.position;
                self.rb.AddForce(error.normalized * speed);

                // TODO: Target angle is a bit weird, what do we do with that?
                var look_rotation = Quaternion.LookRotation(error, Vector3.up);
                self.target_angle = look_rotation.eulerAngles.y;

                if (error.x*error.x+error.z*error.z < ACCEPTABLE_ERROR*ACCEPTABLE_ERROR) {
                    still_timer = Random.Range(1f, 5f);
                    target.gameObject.SetActive(true);
                    current = States.Still;
                }
            } break;
        }
    }
}
