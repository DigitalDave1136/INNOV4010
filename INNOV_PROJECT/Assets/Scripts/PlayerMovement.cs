using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //Initialized variables
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    
    //For info panel display
    public LayerMask layerMask;
    public List<GameObject> canvasList;
    private float rayDist = 7f;
    //Movement
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;


    /// <summary>
    /// Start is called when the scene is run
    /// </summary>
    void Start()
    {
        //Gets components
        characterController = GetComponent<CharacterController>();
        //locks and makes invisible cursor so it's in first person
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    void Update()
    {
        //Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        //Jump
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        //Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        //Crouch
        if (Input.GetKey(KeyCode.C) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;

        }
        else
        {
            //Set normal height
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        //Moving
        characterController.Move(moveDirection * Time.deltaTime);

        //Look around
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        //Interact (left click)
        if (Input.GetMouseButtonDown(0))
        {
            //If there's no menu in the way
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //Check out if you can interact with the painting (by being close enough)
                RaycastHit hit;
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, rayDist, layerMask))
                {
                    //Open the info panel and allow you to move your mose
                    string gameTag = hit.transform.tag;
                    canvasList[int.Parse(gameTag)].SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        //Escape key is pressed
        if (Input.GetKey(KeyCode.Escape))
        {
            //Close info panel and locks your mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            for(int i = 0; i < canvasList.Count; i++)
            {
                canvasList[i].SetActive(false);
            }
        }
    }
}
