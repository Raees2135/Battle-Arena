using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiSlider : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public Button damageButton;
    public Button healButton;
    public Slider healthSlider;

    private void Start()
    {
        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        currentHealth = maxHealth;

        damageButton.onClick.AddListener(TakeDamage);
        healButton.onClick.AddListener(HealthHeal);
    }

    public void TakeDamage()
    {
        currentHealth -= 20;
        healthSlider.value = currentHealth;
        Debug.Log("Current Health " + currentHealth);
    }

    public void HealthHeal()
    {
        currentHealth = maxHealth;
        healthSlider.value = currentHealth;
        Debug.Log("Health Maxmised");
    }
}
