using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMvmt : MonoBehaviour
{
    public Rigidbody rb;
    public float termGroundVel = 7f;
    public float termAirVel = 1f;
    public float accelerationCoeff = 12f;

    public float friction = 7f;
    public float bhopTolerance = 0.20f; // Tolerance to make another successful bhop
    public float landCooldown = 0.30f; // Cooldown after landing a jump

    public float jumpHeight = 1f; // relative control player has in air
    // public float acrobatics = 1.0f; // relative control player has in air

    // private int tick = 0;

    public float sensitivity = 100f;

    public GameObject cameraObj;
    public Transform weapon;

    private bool isGrounded = false;
    private RaycastHit ground;
    public float groundCastDist = 1.00f;
    public float groundCastRad = 0.33f;

    private float lastGrounding;
    private bool canJump = false;

    private float xRotation = 0f;

    private Vector3 prevVelocity = Vector3.zero;

    public Animator animator;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }


    void checkGrounded()
    {
        int layerMask = 1 << 10; // mask to only hit ground surfaces

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, groundCastDist, layerMask))
        if (Physics.SphereCast(
            transform.position, //origin
            groundCastRad, // radius of sphere (~radius of player)
            Vector3.down, // direction
            out hit, // store hit data
            groundCastDist, // distance from origin to cast (half player height)
            layerMask // only collide w/ ground
            ))
        {
            // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            float slope = Mathf.Acos(Vector3.Dot(hit.normal.normalized, Vector3.up.normalized)) * 180f / 3.14f;
            if (slope < 45) {
                ground = hit;
                if (!isGrounded) {
                    lastGrounding = Time.time;
                    isGrounded = true;
                    canJump = true;
                }
            } else {
                isGrounded = false;
            }
            
        } else {
            isGrounded = false;
        }
        animator.SetBool("InAir", !isGrounded);
    }

    Vector3 adjustForSlope(Vector3 mvmtVec) {
        float mag = mvmtVec.magnitude;
        Vector3 normal = ground.normal.normalized;
        return Vector3.ProjectOnPlane(mvmtVec, normal).normalized * mag;
    }

    void Update()
    {
        checkGrounded();
        Aim();
        Movement();
    }

    void Movement() {
        if (isGrounded) {
            rb.useGravity = false;
        } else {
            rb.useGravity = true;
        }
        prevVelocity = rb.velocity;

        float dTime = Time.deltaTime;
        float timeOnGround = Time.time - lastGrounding;

        Vector3 velocityModifier = Vector3.zero; 


        // default to no jump
        Vector3 jumpAdj = Vector3.zero;

        // get wasd inputs
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 lateralVec = transform.right * x;
        Vector3 forwardVec = transform.forward * z;
        Vector3 mvmtVec = (lateralVec + forwardVec);

        if (isGrounded) {
            mvmtVec = adjustForSlope(mvmtVec);
        }

        if(!isGrounded) {
            velocityModifier += mvmtVec;
        } else if (isGrounded) { 
            if (Input.GetButtonDown("Jump")) {
                jumpAdj.y = Mathf.Sqrt(2f * jumpHeight * 9.81f);
                // float jumpMagnitude = Vector3.Dot(jumpAdj, ground.normal);
                // jumpAdj.y = jumpMagnitude;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }

            Vector3 orthoVel = Vector3.Dot(prevVelocity, ground.normal) / ground.normal.magnitude * ground.normal;
            Vector3 prevxz = prevVelocity - orthoVel; // new Vector3(prevVelocity.x, 0, prevVelocity.z);
            float xzspeed = prevxz.magnitude;

            if (timeOnGround > bhopTolerance && xzspeed > 0) { // give a buffer before we apply friction
                float drop = xzspeed * friction * dTime;
                prevxz *= Mathf.Max(xzspeed - drop, 0) / (xzspeed + 0.001f);
                // Debug.Log(prevxz);
                prevVelocity = prevxz + orthoVel;
            }

            if (timeOnGround > landCooldown) { // allow player input after landCooldown
                velocityModifier += mvmtVec;
            }
        }

        float termVel = isGrounded ? termGroundVel : termAirVel;

        if (velocityModifier.magnitude > 0.01) {
            float projectedVel = Vector3.Dot(prevVelocity, velocityModifier.normalized);
            float accelVel = accelerationCoeff * friction * dTime; // control for friction, time

            if (projectedVel + accelVel > termVel) {
                accelVel = Mathf.Clamp(termVel - projectedVel, 0, termVel);
            }

            velocityModifier = velocityModifier.normalized * accelVel;
            // if (!isGrounded) velocityModifier *= acrobatics;
        }

        Vector3 agrav = new Vector3(0, 0, 0);
        if (isGrounded) {
            agrav += -9.81f * ground.normal.normalized * Time.deltaTime;
        }

        rb.velocity = prevVelocity + velocityModifier + agrav;

        rb.AddForce(velocityModifier + agrav, ForceMode.VelocityChange);
        if (canJump)
            rb.AddForce(jumpAdj, ForceMode.VelocityChange);
        if (jumpAdj.y > 0) {
            isGrounded = false;
            canJump = false;
        }
    }

    void FixedUpdate()
    {
    }

    private void Aim()
    {
        float xSensitivity = sensitivity;
        float ySensitivity = sensitivity;
        float dTime = Time.deltaTime;
        float y = Input.GetAxis("Mouse X") * xSensitivity * dTime;
        float x = Input.GetAxis("Mouse Y") * ySensitivity * dTime;

        // Vector3 rotateValue = new Vector3(x, y*-1, 0);
        // cameraObj.transform.eulerAngles = transform.eulerAngles - rotateValue;
        xRotation -= x;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        cameraObj.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * y);
    }
}
