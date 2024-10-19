using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BulletsText : MonoBehaviour
{
    private ShootingController shootingController;
    public Text bulletText;
    private PhotonView view;

    private void Start()
    {
        shootingController = GetComponent<ShootingController>();
        view = GetComponent<PhotonView>();

        // Subscribe to gun change event
        shootingController.OnGunChanged += UpdateBulletText;

        // Initialize bullet text
        UpdateBulletText(shootingController.CurrentAmmo, shootingController.MaxAmmo);
    }

    private void OnDestroy()
    {
        // Unsubscribe from gun change event to prevent memory leaks
        if (shootingController != null)
            shootingController.OnGunChanged -= UpdateBulletText;
    }

    public void UpdateBulletText(int currentAmmo, int maxAmmo)
    {
        bulletText.text = $"{currentAmmo}/{maxAmmo}";
    }

    private void UpdateBulletText()
    {
        bulletText.text = $"{shootingController.CurrentAmmo}/{shootingController.MaxAmmo}";
    }
}
