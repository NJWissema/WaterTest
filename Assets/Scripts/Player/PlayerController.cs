using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //reference the transform
    Transform playerTransform;

    public static bool inWater;
    public static bool isSwimming;
    // if not in water, walk
    // if in water and not swimming, float
    // if in water and swimming, swim
    public LayerMask waterMask;

    [Header("Player Rotation")]
    public float sensitivity = 1f;

    //Clamp Variables
    public float rotationMin;
    public float rotationMax;

    //mouse input
    float rotationX;
    float rotationY;

    [Header("Player Movement")]
    public float walkSpeed = 1;
    public float swimSpeed = 1;
    float moveX;
    float moveY;
    float moveZ;

    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        SwimmingOrFloating();
        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        SwitchMovement();
    }
    private void OnTriggerExit(Collider other)
    {
        SwitchMovement();
    }

    void SwimmingOrFloating()
    {
        bool swimCheck = false;
        if(inWater){
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(playerTransform.position.x, playerTransform.position.y + 0.5f, playerTransform.position.z), Vector3.down, out hit, Mathf.Infinity, waterMask))
            {
                if (hit.distance < 0.1f)
                {
                    swimCheck = true;
                }
            }
            else{
                swimCheck = true;
            }
        }
        isSwimming = swimCheck;
        Debug.Log("isSwiming: " + isSwimming);
    }
    

    // Update is called once per frame
    void Update()
    {
        LookAround();

        //Debug function
        if(Input.GetKey(KeyCode.Escape)){
            Cursor.lockState = CursorLockMode.None;
        }
        
    }

    void SwitchMovement(){
        inWater = !inWater;
        rb.useGravity = !rb.useGravity;
    }

    void LookAround()
    {
        //get the mouse input
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;

        //Clamp the values of x and y
        rotationY = Mathf.Clamp(rotationY, rotationMin, rotationMax);
        
        //setting the rotation value every update
        playerTransform.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);
    }

    void Move()
    {
        //get the movement input
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Forward");
        moveY = Input.GetAxis("Vertical");

        //check if the player is standing still
        if (inWater) //If in water, velocity = 0
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            if (moveX == 0 && moveZ == 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if(!inWater)
        {
            // Move the player (Land)
            playerTransform.Translate(new Quaternion(0,playerTransform.rotation.y, 0, playerTransform.rotation.w) * new Vector3(moveX, 0, moveZ) * Time.deltaTime * walkSpeed, Space.World);
        }
        else
        {
            // CHeck if player is floating above water or swimming
            if(!isSwimming)
            {
                // Move the player (floating)
                // Clamp the MoveY value, so that it cannot use space or ctrl to move up
                moveY = Mathf.Min(moveY,0);

                // convert the local direction vector into a worldSpace vector
                Vector3 clampedDirection = playerTransform.TransformDirection(new Vector3(moveX, moveY, moveZ));

                // clamp the values of this worldspace vector
                clampedDirection = new Vector3(clampedDirection.x, Mathf.Min(clampedDirection.y, 0), clampedDirection.z);

                playerTransform.Translate(clampedDirection * Time.deltaTime * swimSpeed, Space.World);
            }
            else
            {
                // Move the player (swimming)
                playerTransform.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * swimSpeed);
                playerTransform.Translate(new Vector3(0, moveY, 0) * Time.deltaTime * swimSpeed, Space.World);
            }
        }

        
    }
}
