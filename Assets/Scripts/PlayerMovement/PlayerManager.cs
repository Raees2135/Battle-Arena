using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    InputManager inputManager;

    PlayerMovement PlayerMovement;
    CameraManager cameraManager;

    Animator animator;

    public bool isInteracting;

    PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        inputManager = GetComponent<InputManager>();
        PlayerMovement = GetComponent<PlayerMovement>();
        cameraManager = FindAnyObjectByType<CameraManager>();
        animator = GetComponent<Animator>();

    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(GetComponentInChildren<CameraManager>().gameObject);
        }
    }

    private void Update()
    {
        if (!view.IsMine)
        {
            return;
        }
        inputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        if (!view.IsMine)
        {
            return;
        }

        PlayerMovement.HandleAllMovement();
    }

    private void LateUpdate()
    {
        if (!view.IsMine)
        {
            return;
        }

        cameraManager.HandleAllCameraMovement();

        isInteracting = animator.GetBool("isInteracting");

        PlayerMovement.isJumping = animator.GetBool("isJumping");
        animator.SetBool("isGrounded", PlayerMovement.isGrounded);
    }
}
