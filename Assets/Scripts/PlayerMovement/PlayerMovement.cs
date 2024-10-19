using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Playeer Health")]

    public float maxHealth = 150f;
    public float currentHealth;
    public Slider healthSlider;
    public GameObject playerUI;


    [Header("Ref & Physics")]


    InputManager inputManager;

    PlayerManager playerManager;

    PlayerControllerManager playerControllerManager;

    AnimatorManager animatorManager;

    Vector3 movDirection;

    Transform cameraGameObject;

    Rigidbody playerRigidbody;

    [Header("Falling and Landing")]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset = 0.5f;
    public LayerMask groundLayer;


    [Header("Movement Flags")]
    public bool isMoving;
    public bool isSprinting;

    public bool isGrounded;
    public bool isJumping;


    [Header("Movement Values")]
    public float movementSpeed = 2f;

    public float rotationSpeed = 13f;

    public float sprintingSpeed = 7f;

    [Header("Jump Variables")]

    public float jumpHeight = 4f;
    public float gravityIntensity = -15f;

    PhotonView view;

    public int playerTeam;


    private void Awake()
    {
        view = GetComponent<PhotonView>();
        currentHealth = maxHealth;
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraGameObject = Camera.main.transform;

        playerControllerManager = PhotonView.Find((int)view.InstantiationData[0]).GetComponent<PlayerControllerManager>();

        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(playerRigidbody);
            Destroy(playerUI);
        }

        if (view.Owner.CustomProperties.ContainsKey("Team"))
        {
            int team = (int)view.Owner.CustomProperties["Team"];
            playerTeam = team;
        }
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();
        if (playerManager.isInteracting)
        {
            return;
        }

        HandleMovement();
        HandleRotation();

    }

    void HandleMovement()
    {
        if (isJumping)
        {
            return;
        }

        movDirection = new Vector3(cameraGameObject.forward.x, 0f, cameraGameObject.forward.z) * inputManager.verticalInput;
        movDirection = movDirection + cameraGameObject.right * inputManager.horizontalInput;
        movDirection.Normalize();

        movDirection.y = 0;

        if(isSprinting )
        {
            movDirection = movDirection * sprintingSpeed;
        }
        else
        {
            if(inputManager.movementAmount >= 0.5f)
            {
                movDirection = movDirection * movementSpeed;
                isMoving = true;
            }

            if (inputManager.movementAmount <= 0)
            {
                isMoving = false;
            }
        }

        Vector3 movementVelocity = movDirection;
        playerRigidbody.velocity = movementVelocity;
    }

    void HandleRotation()
    {
        if(isJumping)
        {
            return;
        }

        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraGameObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraGameObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if(targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }

    void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        Vector3 targetPosition;
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;
        targetPosition = transform.position;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnim("Falling", true);
            }

            inAirTimer = inAirTimer + Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        if(Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnim("Landing", true);
            }

            Vector3 rayCastHitPoint = hit.point;
            targetPosition.y = rayCastHitPoint.y;
            inAirTimer = 0;
            isGrounded = true;

        }
        else
        {
            isGrounded = false;
        }

        if(isGrounded && !isJumping)
        {
            if(playerManager.isInteracting || inputManager.movementAmount > 0)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.1f);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }

    public void HandleJumping()
    {
        if(isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnim("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = movDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.velocity = playerVelocity;

            isJumping = false;
        }
    }

    public void SetIsJumping(bool isJumping)
    {
        this.isJumping = isJumping;
    }

    public void ApplyDamage(float damageValue)
    {
        view.RPC("RPC_TakeDamage", RpcTarget.All, damageValue);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!view.IsMine)
        {
            return;
        }

        currentHealth -= damage;
        healthSlider.value = currentHealth;
        if(currentHealth <= 0)
        {
            Die();
        }

        Debug.Log("Damage Taken" + damage);
        Debug.Log("Current Health" + currentHealth);
    }

    private void Die()
    {
        playerControllerManager.Die();

        ScoreBoard.instance.PlayerDied(playerTeam);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Healer"))
        {
            currentHealth = maxHealth;
            healthSlider.value = currentHealth;
            Debug.Log("Maximised Health" + currentHealth);
            StartCoroutine(DisableHealerTemporarily(other.gameObject, 30f));
        }
    }

    private IEnumerator DisableHealerTemporarily(GameObject healer, float duration)
    {
        healer.SetActive(false); // Disable the healer
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        healer.SetActive(true); // Reactivate the healer
    }
}
