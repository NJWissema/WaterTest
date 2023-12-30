using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public Rigidbody rigidBody;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;
    public int floaterCount = 1;

    // Drag
    public float waterDrag = 0.99f;
    public float waterAngularDrag = 0.5f;

    private void FixedUpdate()
    {
        rigidBody.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);
        // float waveHeight = WaveManager.instance.GetWaveHeight(transform.position.x);

        float waveHeight = 0;
        if(transform.position.y < waveHeight){
            float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;
            rigidBody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
            rigidBody.AddForce(displacementMultiplier * Time.fixedDeltaTime * waterDrag * -rigidBody.velocity, ForceMode.VelocityChange);
            rigidBody.AddTorque(displacementMultiplier * Time.fixedDeltaTime * waterAngularDrag * -rigidBody.angularVelocity, ForceMode.VelocityChange);
            
        }
    }
}
