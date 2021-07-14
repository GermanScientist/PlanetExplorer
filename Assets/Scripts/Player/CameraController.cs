using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //assign properties
    [SerializeField] private float sensitivity;
    [SerializeField] private float smoothing;

    [SerializeField] private GameObject player;

    [SerializeField] Vector2 smoothedVelocity;
    [SerializeField] Vector2 currentLookingPos;

    //Start is called at the first frame
    private void Start()
    {
        //Sets sensitivity and smoothing
        sensitivity = 2;
        smoothing = 2;

        //Finds player object in scene
        player = transform.parent.gameObject;

        //Locks cursor and makes cursor invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Update is called every frame
    private void Update()
    {
        //RotateCam handles the rotation of the camera
        RotateCam();
    }

    private void RotateCam()
    {
        //Gets player mouse position
        Vector2 inputValues = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        inputValues = Vector2.Scale(inputValues, new Vector2(sensitivity * smoothing, sensitivity * smoothing));

        //Smooths camera movement using lerp
        smoothedVelocity.x = Mathf.Lerp(smoothedVelocity.x, inputValues.x, 1f / smoothing);
        smoothedVelocity.y = Mathf.Lerp(smoothedVelocity.y, inputValues.y, 1f / smoothing);

        //Sets the position of where the player is currently looking
        currentLookingPos += smoothedVelocity;

        //Clamps camera position so the player can't rotate 360 degrees vertically
        /*  currentLookingPos.y = Mathf.Clamp(currentLookingPos.y, -80f, 60f);   */

        //Updates player rotation depending on mouse/camera position 
        transform.localRotation = Quaternion.AngleAxis(-currentLookingPos.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(currentLookingPos.x, player.transform.up);
    }
}
