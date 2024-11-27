using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalBehaviourScript : EntityBehaviourScript
{
    public int meatAmount;
    public int leatherAmount;
    public GameObject meatPrefab;
    public GameObject leatherPrefab;
    public NavMeshAgent agent;

    protected override void Start() 
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();   
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

    }

    protected override void Die()
    {
        DropLoot();
        base.Die();
    }
    protected virtual void Idle()
    {
    }

    protected virtual void Run()
    {
    }

    private void DropLoot()
    {
        if (meatPrefab != null)
        {
            GameObject meat = Instantiate(meatPrefab, transform.position, Quaternion.identity);
            LootScript meatLoot = meat.GetComponent<LootScript>();
            meatLoot.amount = meatAmount;
            meatLoot.lootName = "Carne";
        }

        if (leatherPrefab != null)
        {
            GameObject leather = Instantiate(leatherPrefab, transform.position, Quaternion.identity);
            LootScript leatherLoot = leather.GetComponent<LootScript>();
            leatherLoot.amount = leatherAmount;
            leatherLoot.lootName = "Cuero";
        }
    }
}
