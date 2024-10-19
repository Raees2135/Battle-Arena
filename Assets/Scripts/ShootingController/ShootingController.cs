using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ShootingController : MonoBehaviour
{
    Animator animator;
    InputManager inputManager;
    PlayerMovement playerMovement;
    private BulletsText bulletsText;

    [Header("Primary Gun Settings")]
    public GameObject primaryGun;
    public GameObject switchPrimaryGun;
    public Transform primaryFirePoint;
    public Transform primaryGunParent; // Parent for the primary gun
    public float fireRate = 0f;
    public float primaryFireRange = 100f;
    public float primaryFireDamage = 15f;
    private float nextFireTime = 0f;
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 1.5f;

    public int CurrentAmmo
    {
        get { return isUsingPrimaryGun ? currentAmmo : secondaryCurrentAmmo; }
    }
    public int MaxAmmo
    {
        get { return isUsingPrimaryGun ? maxAmmo : secondaryMaxAmmo; }
    }

    [Header("Secondary Gun Settings")]
    public GameObject secondaryGun;
    public GameObject switchSecondaryGun;
    public Transform secondaryFirePoint;
    public Transform secondaryGunParent; // Parent for the secondary gun
    public float secondaryFireRate = 0f;
    public float secondaryFireRange = 100f;
    public float secondaryFireDamage = 20f;
    private float nextSecondaryFireTime = 0f;
    public int secondaryMaxAmmo = 10;
    public int secondaryCurrentAmmo;
    public float secondaryReloadTime = 2.0f;

    private bool isUsingPrimaryGun = true;

    [Header("Shooting Flags")]
    public bool isShooting;
    public bool isWalking;
    public bool isShootingInput;
    public bool isReloading = false;
    public bool isScopeInput;

    [Header("SoundsFx")]
    public AudioSource soundAudioSource;
    public AudioClip shootingSoundClip;
    public AudioClip reloadingSoundClip;

    [Header("Effects")]
    private ParticleSystem primaryMuzzleFlash;
    private ParticleSystem secondaryMuzzleFlash;
    public ParticleSystem bloodEffect;

    PhotonView view;

    public int playerTeam;

    public delegate void GunChangedEventHandler();
    public event GunChangedEventHandler OnGunChanged;

    

    private void Start()
    {
        view = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        bulletsText = GetComponent<BulletsText>();
        currentAmmo = maxAmmo;
        secondaryCurrentAmmo = secondaryMaxAmmo;

        if (view.Owner.CustomProperties.ContainsKey("Team"))
        {
            int team = (int)view.Owner.CustomProperties["Team"];
            playerTeam = team;
        }

        switchPrimaryGun.SetActive(true);
        switchSecondaryGun.SetActive(false);

        primaryMuzzleFlash = primaryGunParent.GetComponentInChildren<ParticleSystem>();
        secondaryMuzzleFlash = secondaryGunParent.GetComponentInChildren<ParticleSystem>();

        UpdateBulletText();
    }

    private void Update()
    {
        if (!view.IsMine)
        {
            return;
        }

        if (inputManager.switchInput)
        {
            SwitchGun();
            inputManager.switchInput = false;  // Reset switch input flag
        }

        if (isReloading)
        {
            animator.SetBool("Shoot", false);
            animator.SetBool("ShootingMovement", false);
            animator.SetBool("ShootWalk", false);
            return;
        }

        isWalking = playerMovement.isMoving;
        isShootingInput = inputManager.fireInput;
        isScopeInput = inputManager.scopeInput;

        if (isShootingInput && isWalking)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Shoot();
                animator.SetBool("ShootWalk", true);
            }

            animator.SetBool("Shoot", false);
            animator.SetBool("ShootingMovement", true);
            isShooting = true;
        }
        else if (isShootingInput)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Shoot();
            }

            animator.SetBool("Shoot", true);
            animator.SetBool("ShootingMovement", false);
            animator.SetBool("ShootWalk", false);
            isShooting = true;
        }
        else if (isScopeInput)
        {
            animator.SetBool("Shoot", false);
            animator.SetBool("ShootingMovement", true);
            animator.SetBool("ShootWalk", false);
            isShooting = false;
        }
        else if (isShootingInput && isWalking)
        {
            animator.SetBool("Shoot", true);
            animator.SetBool("ShootingMovement", true);
            animator.SetBool("ShootWalk", true);
            isShooting = true;
        }
        else
        {
            animator.SetBool("Shoot", false);
            animator.SetBool("ShootingMovement", false);
            animator.SetBool("ShootWalk", false);
            isShooting = false;
        }

        if (inputManager.reloadInput && currentAmmo < maxAmmo)
        {
            Reload();
        }

        // Check for gun pickup
        if (inputManager.pickupInput) // Assuming you have a boolean for pickup input in your InputManager
        {
            CheckForGunPickup();
            inputManager.pickupInput = false;
        }

        //Check for the muzzle flash when another gun picked up
        if (primaryMuzzleFlash == null)
        {
            primaryMuzzleFlash = primaryGunParent.GetComponentInChildren<ParticleSystem>();
        }

        if (secondaryMuzzleFlash == null)
        {
            secondaryMuzzleFlash = secondaryGunParent.GetComponentInChildren<ParticleSystem>();
        }


    }

    public void Shoot()
    {
        if ((isUsingPrimaryGun ? currentAmmo : secondaryCurrentAmmo) > 0)
        {
            RaycastHit hit;
            Transform firePoint = isUsingPrimaryGun ? primaryFirePoint : secondaryFirePoint;
            float fireRange = isUsingPrimaryGun ? primaryFireRange : secondaryFireRange;
            float fireDamage = isUsingPrimaryGun ? primaryFireDamage : secondaryFireDamage;
            ParticleSystem muzzleFlash = isUsingPrimaryGun ? primaryMuzzleFlash : secondaryMuzzleFlash;

            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange))
            {
                Debug.Log(hit.transform.name);

                Vector3 hitPoint = hit.point;
                Vector3 hitNormal = hit.normal;

                PlayerMovement playerMovementDamage = hit.collider.GetComponent<PlayerMovement>();
                if (playerMovementDamage != null && playerMovementDamage.playerTeam != playerTeam)
                {
                    playerMovementDamage.ApplyDamage(fireDamage);
                    view.RPC("RPC_Shoot", RpcTarget.All, hitPoint, hitNormal);
                }
            }

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            soundAudioSource.PlayOneShot(shootingSoundClip);
            if (isUsingPrimaryGun)
                currentAmmo--;
            else
                secondaryCurrentAmmo--;

            UpdateBulletText();
        }
        else
        {
            Reload();
        }

    }

    [PunRPC]
    public void RPC_Shoot(Vector3 hitPoint, Vector3 hitNormal)
    {
        ParticleSystem blood = Instantiate(bloodEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        Destroy(blood.gameObject, blood.main.duration);
    }

    public void Reload()
    {
        if (!isReloading && (isUsingPrimaryGun ? currentAmmo : secondaryCurrentAmmo) < (isUsingPrimaryGun ? maxAmmo : secondaryMaxAmmo))
        {
            if (isShootingInput && isWalking)
            {
                animator.SetTrigger("ShootReload");
            }
            else
            {
                animator.SetTrigger("Reload");
            }

            isReloading = true;
            soundAudioSource.PlayOneShot(reloadingSoundClip);
            Invoke("FinishReloading", isUsingPrimaryGun ? reloadTime : secondaryReloadTime);
        }
    }

    public void FinishReloading()
    {
        if (isUsingPrimaryGun)
            currentAmmo = maxAmmo;
        else
            secondaryCurrentAmmo = secondaryMaxAmmo;

        isReloading = false;

        if (isShootingInput && isWalking)
        {
            animator.ResetTrigger("ShootReload");
        }
        else
        {
            animator.ResetTrigger("Reload");
        }

        UpdateBulletText();
    }

    private void SwitchGun()
    {
        isUsingPrimaryGun = !isUsingPrimaryGun;

        // Activate the correct gun and deactivate the other
        if (isUsingPrimaryGun)
        {
            switchPrimaryGun.SetActive(true);
            switchSecondaryGun.SetActive(false);
            currentAmmo = Mathf.Min(currentAmmo, maxAmmo); // Cap ammo to maxAmmo if it exceeds
        }
        else
        {
            switchPrimaryGun.SetActive(false);
            switchSecondaryGun.SetActive(true);
            secondaryCurrentAmmo = Mathf.Min(secondaryCurrentAmmo, secondaryMaxAmmo); // Cap ammo to secondaryMaxAmmo if it exceeds
        }

        if (OnGunChanged != null)
            OnGunChanged();

        // Update bullet text after switching gun
        UpdateBulletText();

        // Sync bullet text with other clients
        view.RPC("RPC_UpdateBulletText", RpcTarget.All, CurrentAmmo, MaxAmmo);
    }

    private void UpdateBulletText()
    {
        if (bulletsText != null)
        {
            bulletsText.UpdateBulletText(CurrentAmmo, MaxAmmo);
        }
    }

    [PunRPC]
    private void RPC_UpdateBulletText(int currentAmmo, int maxAmmo)
    {
        if (bulletsText != null)
        {
            bulletsText.UpdateBulletText(currentAmmo, maxAmmo);
        }
    }

    public void EquipGun(GameObject newGun, GunProperties properties)
    {
        if(switchPrimaryGun.activeSelf)
        {
            // Destroy the current primary gun if there is one
            if (primaryGun != null)
            {
                Destroy(primaryGun);
            }

            primaryGun = Instantiate(newGun, primaryGunParent);
            primaryGun.transform.localPosition = new Vector3(80, -7, 10);
            primaryGun.transform.localRotation = Quaternion.Euler(180, -88, -81);
            primaryGun.transform.localScale = new Vector3(500, 500, 500);

            // Assign properties
            fireRate = properties.fireRate;
            primaryFireRange = properties.fireRange;
            primaryFireDamage = properties.fireDamage;
            maxAmmo = properties.maxAmmo;
            currentAmmo = maxAmmo;
            reloadTime = properties.reloadTime;

            if (primaryMuzzleFlash == null)
            {
                primaryMuzzleFlash = primaryGunParent.GetComponentInChildren<ParticleSystem>();
            }

            soundAudioSource = GetComponent<AudioSource>();
        }
        else if (switchSecondaryGun.activeSelf)
        {
            // Destroy the current secondary gun if there is one
            if (secondaryGun != null)
            {
                Destroy(secondaryGun);
            }

            secondaryGun = Instantiate(newGun, secondaryGunParent);
            secondaryGun.transform.localPosition = new Vector3(80, -7, 10);
            secondaryGun.transform.localRotation = Quaternion.Euler(180, -88, -81);
            secondaryGun.transform.localScale = new Vector3(500, 500, 500);

            // Assign properties
            secondaryFireRate = properties.fireRate;
            secondaryFireRange = properties.fireRange;
            secondaryFireDamage = properties.fireDamage;
            secondaryMaxAmmo = properties.maxAmmo;
            secondaryReloadTime = properties.reloadTime;
            secondaryCurrentAmmo = secondaryMaxAmmo; // Reset ammo when picking up a new gun

            if(secondaryMuzzleFlash == null)
            {
                secondaryMuzzleFlash = secondaryGunParent.GetComponentInChildren<ParticleSystem>();
            }
            

            soundAudioSource = GetComponent<AudioSource>();
        }

        if (OnGunChanged != null)
            OnGunChanged();


        view.RPC("RPC_EquipGun", RpcTarget.Others, newGun.name, properties.gunName, switchPrimaryGun.activeSelf);
    }

    [PunRPC]
    public void RPC_EquipGun(string gunName, string propertiesName, bool isPrimaryGun)
    {
        GameObject gunPrefab = Resources.Load<GameObject>(gunName);
        GunProperties properties = Resources.Load<GunProperties>("GunProperties/" + propertiesName);

        if (isPrimaryGun)
        {
            // Destroy the current primary gun if there is one
            if (primaryGun != null)
            {
                Destroy(primaryGun);
            }

            primaryGun = Instantiate(gunPrefab, primaryGunParent);
            primaryGun.transform.localPosition = new Vector3(80, -7, 10);
            primaryGun.transform.localRotation = Quaternion.Euler(180, -88, -81);
            primaryGun.transform.localScale = new Vector3(500, 500, 500);
            currentAmmo = properties.maxAmmo;

            if(primaryMuzzleFlash == null)
            {
                primaryMuzzleFlash = primaryGunParent.GetComponentInChildren<ParticleSystem>();
            }
            

            // Assign properties
            fireRate = properties.fireRate;
            primaryFireRange = properties.fireRange;
            primaryFireDamage = properties.fireDamage;
            maxAmmo = properties.maxAmmo;
            reloadTime = properties.reloadTime;
            soundAudioSource = primaryGun.GetComponent<AudioSource>();
        }
        else
        {
            // Destroy the current secondary gun if there is one
            if (secondaryGun != null)
            {
                Destroy(secondaryGun);
            }

            secondaryGun = Instantiate(gunPrefab, secondaryGunParent);
            secondaryGun.transform.localPosition = new Vector3(80, -7, 10);
            secondaryGun.transform.localRotation = Quaternion.Euler(180, -88, -81);
            secondaryGun.transform.localScale = new Vector3(500, 500, 500);
            secondaryCurrentAmmo = properties.maxAmmo;

            if (secondaryMuzzleFlash == null)
            {
                secondaryMuzzleFlash = secondaryGunParent.GetComponentInChildren<ParticleSystem>();
            }

            // Assign properties
            secondaryFireRate = properties.fireRate;
            secondaryFireRange = properties.fireRange;
            secondaryFireDamage = properties.fireDamage;
            secondaryMaxAmmo = properties.maxAmmo;
            secondaryReloadTime = properties.reloadTime;
            soundAudioSource = secondaryGun.GetComponent<AudioSource>();
        }
        if (OnGunChanged != null)
            OnGunChanged();
    }

    private void CheckForGunPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(primaryFirePoint.position, primaryFirePoint.forward, out hit, 5f)) // Adjust the range as needed
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            if (hit.collider.CompareTag("GunPickup"))
            {
                Debug.Log("Gun pickup detected");
                GunPickUp gunPickup = hit.collider.GetComponent<GunPickUp>();
                if (gunPickup != null)
                {
                    EquipGun(gunPickup.gunPrefab, gunPickup.gunProperties);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        else
        {
            Debug.Log("No gun pickup in range");
        }
    }
}