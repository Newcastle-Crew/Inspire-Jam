using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol :  Behaviour {
    public PatrolPath path;

    int pathIndex = 0;

    public bool forward = true;

    const float MAX_ERROR_FOR_FOLLOW = 1.2f;
    const float MAX_ERROR_FOR_RECALCULATE = 5f;
    const float FAR_RECALCULATE_TIME = 2f;
    public bool forceRecalculate = true;

    public float speed = 4f;

    float recalculated_time = -10000f;

    public Vector3 GetDirection(Vector3 from_pos) {
        if (forceRecalculate) {
            forceRecalculate = false;

            var point = path.ClosestPoint(from_pos, forward);
            pathIndex = point.index;
            Debug.Log(pathIndex);
        }

        int childCount = path.transform.childCount;
        var a = path.transform.GetChild(pathIndex).position;
        var b = path.transform.GetChild(pathIndex + (forward ? 1 : -1)).position;

        var (closestPos, t) = PatrolPath.ClosestLinePoint(a, b, from_pos);
        var error = closestPos - from_pos;
        error = new Vector3(error.x, 0f, error.z);
        if (error.sqrMagnitude >= MAX_ERROR_FOR_FOLLOW * MAX_ERROR_FOR_FOLLOW) {
            if ((Time.fixedTime - recalculated_time) > FAR_RECALCULATE_TIME) {
                recalculated_time = Time.fixedTime;
                forceRecalculate = true;
            }

            return error.normalized;
        } else {
            if (t >= 1f) {
                if (forward) {
                    pathIndex += 1;
                    if (pathIndex + 1 >= childCount) {
                        pathIndex = childCount - 1;
                        forward = false;
                    }
                } else if (!forward) {
                    pathIndex -= 1;
                    if (pathIndex <= 0) {
                        forward = true;
                        pathIndex = 0;
                    }
                }
            }

            return (b - a).normalized;
        }
    }

    public override void Run(TalkativeNpc self) {
        var dir = GetDirection(self.EyePosition());
        self.rb.AddForce(dir * speed);

        self.target_angle = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
    }
}