using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;

    AnimatorManager animatorManager;

    PlayerMovement playerMovement;

    public Vector2 movementInput;
    public Vector2 cameraMovementInput;

    public float verticalInput;
    public float horizontalInput;

    public float cameraInputX;
    public float cameraInputY;

    public float movementAmount;

    [Header("Input Buttons Flags")]
    public bool bInput;
    public bool jumpInput;

    public bool fireInput;
    public bool reloadInput;
    public bool scopeInput;
    public bool switchInput;
    public bool pickupInput;

    public float sprintThreshold = 0.8f;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if(playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.CameraMovement.performed += i => cameraMovementInput = i.ReadValue<Vector2>();
            playerControls.PlayerActions.B.performed += i => bInput = true;
            playerControls.PlayerActions.B.canceled += i => bInput = false;

            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;

            playerControls.PlayerActions.Reloading.performed += i => reloadInput = true;

            playerControls.PlayerActions.Fire.performed += i => fireInput = true;
            playerControls.PlayerActions.Fire.canceled += i => fireInput = false;

            playerControls.PlayerActions.Scope.performed += i => scopeInput = true;
            playerControls.PlayerActions.Scope.canceled += i => scopeInput = false;

            playerControls.PlayerActions.SwitchGun.performed += i => switchInput = true;
            playerControls.PlayerActions.SwitchGun.canceled += i => switchInput = false;

            playerControls.PlayerActions.PickUp.performed += i => pickupInput = i.ReadValueAsButton();


        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInput()
    {
        HandleMovementInput();
        HandleSprintingInput();
        HandleJumpingInput();

    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraMovementInput.x;
        cameraInputY = cameraMovementInput.y;

        movementAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.ChangeAnimatorValues(0, movementAmount, playerMovement.isSprinting);

    }

    private void HandleSprintingInput()
    {
        if(bInput && movementAmount > 0.5)
        {
            playerMovement.isSprinting = true;
        }
        else
        {
            playerMovement.isSprinting = false;
        }

        if(movementInput.magnitude > sprintThreshold)
        {
            playerMovement.isSprinting = true;
        }
        else
        {
            playerMovement.isSprinting = false;
        }
        
    }

    private void HandleJumpingInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerMovement.HandleJumping();
        }
    }
}
