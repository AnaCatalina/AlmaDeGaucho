using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviourScript : EntityBehaviourScript
{
    public int moneyAmount;
    public int itemAmount;
    public GameObject moneyPrefab;
    public GameObject itemPrefab;
    public GameObject itemPrefab2;
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

    private void DropLoot()
    {
        if (moneyPrefab != null)
        {
            GameObject money = Instantiate(moneyPrefab, transform.position, Quaternion.identity);
            LootScript moneyLoot = money.GetComponent<LootScript>();
            moneyLoot.amount = moneyAmount;
            moneyLoot.lootName = "Real Argentino";
        }

        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            LootScript itemLoot = item.GetComponent<LootScript>();
            itemLoot.amount = itemAmount;
            itemLoot.lootName = "Item Special";
        }
        if (itemPrefab2 != null)
        {
            GameObject item2 = Instantiate(itemPrefab2, transform.position, Quaternion.identity);
            LootScript itemLoot2 = item2.GetComponent<LootScript>();
            itemLoot2.amount = itemAmount;
            itemLoot2.lootName = "Item Special";
        }
    }
}
