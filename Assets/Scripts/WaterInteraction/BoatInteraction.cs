using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInteraction : MonoBehaviour
{

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        SwitchMovement();
    }
    private void OnTriggerExit(Collider other)
    {
        SwitchMovement();
    }

    void SwitchMovement(){
        rb.useGravity = !rb.useGravity;
        rb.velocity = new Vector2(0, 0);
    }
}
