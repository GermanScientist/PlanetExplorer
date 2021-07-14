using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (GravityBody))]
public class PlanetPlayerController : MonoBehaviour
{
    public float mounseSensX = 250;
    public float mounseSensY = 250;

    public float currentspeed;
    public float walkspeed = 4;
    public float sprintspeed = 8;
    public float jumpForce = 220;
    public LayerMask groundedMask;

    bool grounded;
    Vector3 moveAmount;
    Vector3 smoothMoveVel;
    float verLookRotation;
    Transform cameraTransform;
    Rigidbody rb;

    public GameObject planet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraTransform = Camera.main.transform;

        currentspeed = walkspeed;

        planet = GameObject.Find("Planet");
    }

    // Update is called once per frame
    void Update()
    {
        //Look rotation
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mounseSensX * Time.deltaTime );
        verLookRotation += Input.GetAxis("Mouse Y") * mounseSensY * Time.deltaTime;
        verLookRotation = Mathf.Clamp(verLookRotation, -80, 60);
        cameraTransform.localEulerAngles = Vector3.left * verLookRotation;

        //Calculate movement
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0, inputY).normalized;
        Vector3 targetMoveAmount = moveDirection * currentspeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVel, 0.15f);

        //Jump
        if(Input.GetButtonDown("Jump"))
        {
            if(grounded)
            {
                rb.AddForce(transform.up * jumpForce);
            }
        }

        //Ground check
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1.1f, groundedMask))
        {
            grounded = true;
        } else
        {
            grounded = false;
        }

        CheckToSprint();
    }

    private void FixedUpdate()
    {
        //Apply movement to player
        Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + localMove);
    }

    //Checks whether the player is sprinting
    private void CheckToSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            //Setsp playerspeed to sprintspeed when sprinting
            currentspeed = sprintspeed;
        }
        else
        {
            //Setsp playerspeed to normalspeed when not sprinting
            currentspeed = walkspeed;
        }
    }
}
