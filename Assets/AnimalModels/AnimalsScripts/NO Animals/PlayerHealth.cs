using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public JuanMoveBehaviour moveBehaviour;

    void Start()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            moveBehaviour.estaMuerto = true;
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    private void Die()
    {
        Debug.Log("Player died!");
    }

    // Update is called once per frame
    void Update()
    {
        // Solo como ejemplo, puedes quitar esta parte
        if (Input.GetKeyDown(KeyCode.K))
        {
            Heal(10);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TakeDamage(10);
        }
    }
}
