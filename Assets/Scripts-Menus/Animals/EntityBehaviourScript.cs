using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehaviourScript : MonoBehaviour
{
    public float health;
    public string entityName;
    public float speed;
    public Vector3 initialPosition;

    protected virtual void Start()
    {
        initialPosition = transform.position;
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

