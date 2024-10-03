using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootScript : MonoBehaviour
{
    public string lootName;
    public int amount;

    public void Collect()
    {
        Debug.Log($"Collected {amount} of {lootName}");
        //lógica inventario del jugador
        Destroy(gameObject);
    }
}
