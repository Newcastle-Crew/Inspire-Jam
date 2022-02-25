using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour {
    public struct ClosestPointResult {
        public Vector3 position;
        public int index;
    }

    public ClosestPointResult? ClosestPoint(Vector3 point, bool forward) {
        int childCount = transform.childCount;
        if (childCount < 1) {
            Debug.LogError("Cannot have a PatrolPath with less than one point.");
            throw new System.Exception();
        }

        // Get the closest end point to start with.
        var closest = point;
        var closestSqr = 1000000000f;
        int index = 0;
        bool hasAny = false;

        void TryAddingPoint(Vector3 newPoint, int newIndex) {
            var distance_sqr = (newPoint - point).sqrMagnitude;
            if (distance_sqr < closestSqr) {
                // Make sure there's a successful raycast
                if (!Physics.Raycast(newPoint, point - newPoint, (point - newPoint).magnitude, (1 << 3))) {
                    closestSqr = distance_sqr;
                    closest = newPoint;
                    index = newIndex;
                    hasAny = true;
                }
            }
        }

        for (int i=0; i<childCount; i++) {
            var newPoint = transform.GetChild(i).position;
            var next = i + (forward ? 1 : -1);
            TryAddingPoint(newPoint, i);
        }

        for (int i=1; i<childCount; i++) {
            var a = transform.GetChild(i - 1).position;
            var b = transform.GetChild(i    ).position;

            var (linePoint, t) = ClosestLinePoint(a, b, point);
            if (t < 0f || t >= 1f) continue;

            TryAddingPoint(linePoint, forward ? i-1 : i);
        }

        if (!hasAny) return null;

        var result = new ClosestPointResult();
        result.position = closest;
        result.index = index;
        return result;
    }

    public static (Vector3, float) ClosestLinePoint(Vector3 a, Vector3 b, Vector3 point) {
        var line_diff = b - a;
        var line_length = line_diff.magnitude;
        var dir = line_diff / line_length;
        var dot = Vector3.Dot(point - a, dir);
        var linePoint = a + dir * dot;

        return (linePoint, dot / line_length);
    }

    void OnDrawGizmos() {
        int childCount = transform.childCount;
        Gizmos.color = Color.blue;
        for (int i=1; i<childCount; i++) {
            var a = transform.GetChild(i - 1).position;
            var b = transform.GetChild(i    ).position;
            Gizmos.DrawLine(a, b);
        }
    }
}
