using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageScript : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            PlayerHealth health  = other.GetComponent<PlayerHealth>();
            health.TakeDamage(damage);
        }

    }
}
