using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarAccelerator : MonoBehaviour {
    public Rigidbody rb;

    public float speed = 5f;
    
    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (enabled) {
            rb.AddForce(Vector3.forward * speed);
        }
    }
}
