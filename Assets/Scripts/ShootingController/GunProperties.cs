using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunProperties", menuName = "ScriptableObjects/GunProperties", order = 1)]
public class GunProperties : ScriptableObject
{
    public string gunName;
    public float fireRate;
    public float fireRange;
    public float fireDamage;
    public int maxAmmo;
    public float reloadTime;
    public Vector3 localPosition;
    public Vector3 localRotation;
    public Vector3 localScale;
}
