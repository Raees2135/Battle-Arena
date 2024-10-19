using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;

    public Transform playerTransform;
    public Transform cameraTransform;

    [Header("Camera Movement")]
    public Transform cameraPivot;

    private Vector3 cameraFollowVelocity = Vector3.zero;
    public float cameraFollowSpeed = 0.3f;

    public float camLookSpeed = 2f;
    public float camPivotSpeed = 2f;

    public float lookAngle;
    public float pivotAngle;

    public float minPivotAngle = -30f;
    public float maxPivotAngle = 30f;

    [Header("Camera Collision")]
    public LayerMask collisionLayer;
    private float defaultPosition;

    public float cameraCollisionOffset = 0.2f;
    public float minCollisionOffset = 0.2f;
    public float cameraCollisionRadius = 0.2f;

    private Vector3 cameraVectorPosition;

    PlayerMovement playerMovement;

    [Header("Scope")]
    public GameObject scopeCanvas;
    public GameObject playerUI;
    public Camera mainCamera;

    private bool isScoped = false;
    private float originalFOV = 60f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerTransform = FindObjectOfType<PlayerManager>().transform;
        inputManager = FindObjectOfType<InputManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        cameraTransform = Camera.main.transform;
        defaultPosition = cameraTransform.localPosition.z;

    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        CameraCollision();
        IsPlayerScoped();
    }

    void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, playerTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);

        transform.position = targetPosition;
    }

    void RotateCamera()
    {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle = lookAngle + (inputManager.cameraInputX * camLookSpeed);
        pivotAngle = pivotAngle + (inputManager.cameraInputY * camPivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, minPivotAngle, maxPivotAngle);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation =  Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;

        if (playerMovement.isMoving == false && playerMovement.isSprinting == false)
        {
            playerTransform.rotation = Quaternion.Euler(0, lookAngle, 0);
        }
    }

    void CameraCollision()
    {
        float targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if(Physics.SphereCast(cameraPivot.transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayer))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition = -(distance - cameraCollisionOffset);
        }

        if(Mathf.Abs(targetPosition) < minCollisionOffset)
        {
            targetPosition = targetPosition - minCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }

    public void IsPlayerScoped()
    {
        if (inputManager.scopeInput)
        {
            scopeCanvas.SetActive(true);
            playerUI.SetActive(false);
            mainCamera.fieldOfView = 30f;
        }
        else
        {
            scopeCanvas.SetActive(false);
            playerUI.SetActive(true);
            mainCamera.fieldOfView = originalFOV;
        }
    }

}
