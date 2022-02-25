using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol :  Behaviour {
    public PatrolPath path;

    public Behaviour fallbackBehaviour;

    int? pathIndex = 0;

    public bool forward = true;

    float next_node_timer = 0f;
    const float NODE_WAIT_TIME = 1.0f;

    const float MAX_ERROR_FOR_FOLLOW = 1.2f;
    const float MAX_ERROR_FOR_RECALCULATE = 5f;
    const float FAR_RECALCULATE_TIME = 2f;
    public bool forceRecalculate = true;

    public float speed = 4f;

    float recalculated_time = -10000f;

    public Vector3? GetDirection(Vector3 from_pos) {
        if (forceRecalculate) {
            forceRecalculate = false;

            var point = path.ClosestPoint(from_pos, forward);

            pathIndex = point?.index;
        }

        if (!pathIndex.HasValue) {
            if ((Time.fixedTime - recalculated_time) > FAR_RECALCULATE_TIME) {
                recalculated_time = Time.fixedTime;
                forceRecalculate = true;
            }

            return null;
        }

        int childCount = path.transform.childCount;
        var a = path.transform.GetChild(pathIndex.Value).position;
        var b = path.transform.GetChild(pathIndex.Value + (forward ? 1 : -1)).position;

        var (closestPos, t) = PatrolPath.ClosestLinePoint(a, b, from_pos);
        var error = closestPos - from_pos;
        error = new Vector3(error.x, 0f, error.z);
        if (error.sqrMagnitude >= MAX_ERROR_FOR_FOLLOW * MAX_ERROR_FOR_FOLLOW) {
            if ((Time.fixedTime - recalculated_time) > FAR_RECALCULATE_TIME) {
                recalculated_time = Time.fixedTime;
                forceRecalculate = true;
            }

            return error.normalized;
        }

        if (t >= 1f) {
            if (forward) {
                pathIndex += 1;
                if (pathIndex + 1 >= childCount) {
                    pathIndex = childCount - 1;
                    forward = false;
                }

                next_node_timer = NODE_WAIT_TIME;
            } else if (!forward) {
                pathIndex -= 1;
                if (pathIndex <= 0) {
                    forward = true;
                    pathIndex = 0;
                }

                next_node_timer = NODE_WAIT_TIME;
            }
        }

        return (b - a).normalized;
    }

    public override void Run(TalkativeNpc self) {
        if (next_node_timer > 0f) {
            next_node_timer -= Time.fixedDeltaTime;
            return;
        }

        var qDir = GetDirection(self.EyePosition());
        
        if (qDir is Vector3 dir) {
            self.rb.AddForce(dir * speed);

            self.target_angle = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
        } else {
            if (fallbackBehaviour) {
                fallbackBehaviour.Run(self);
            }
        }
    }
}