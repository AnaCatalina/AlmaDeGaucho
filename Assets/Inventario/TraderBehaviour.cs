using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraderBehaviour : MonoBehaviour
{
    public GameObject panel;
    public GameObject player;
    public float distance;
    public bool buy;
    public GameObject panelBuy;
    private InventoryItems inventoryItems;
    public GameObject panelSell;
    public bool enTienda;
    // Start is called before the first frame update
    void Start()
    {
        enTienda = false;
        buy = false;
        inventoryItems = player.GetComponent<InventoryItems>();
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance <= 3.0f && !buy)
        {
            enTienda=true;
            panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            enTienda = false;
            panel.SetActive(false);

        }

        if(enTienda && Input.GetKeyDown(KeyCode.C))
        {
            BuyItems();
        }
        if (enTienda && Input.GetKeyDown(KeyCode.V))
        {
            SellItems();
        }

    }

    public void BuyItems()
    {
        //panel.SetActive(false);
        buy = true;
        panelBuy.SetActive(true);
    }

    public void BuyItemDam()
    {
        inventoryItems.amountDam += 1;
        inventoryItems.amountMon -= 3;
        panelBuy.SetActive(false);
        buy = false;

    }

    public void BuyItemCar()
    {
        inventoryItems.amountCar += 1;
        inventoryItems.amountMon -= 2;
        panelBuy.SetActive(false);
        buy = false;

    }

    public void BuyItemYer()
    {
        inventoryItems.amountYer += 1;
        inventoryItems.amountMon -= 2;
        panelBuy.SetActive(false);
        buy = false;

    }

    public void BuyItemCue()
    {
        inventoryItems.amountCue += 1;
        inventoryItems.amountMon -= 4;
        panelBuy.SetActive(false);
        buy = false;

    }

    public void SellItems()
    {
        //panel.SetActive(false);
        buy = true;
        panelSell.SetActive(true);
    }

    public void SellItemCue()
    {
        if(inventoryItems.amountCue >= 1)
        {
            inventoryItems.amountCue -= 1;
            inventoryItems.amountMon += 2;
            panelSell.SetActive(false);
            buy = false;
        }
        else
        {
            panelSell.SetActive(false);
            buy = false;
            
        }  

    }

    public void SellItemCar()
    {
        if(inventoryItems.amountCar >= 1)
        {
            inventoryItems.amountCar -= 1;
            inventoryItems.amountMon += 1;
            panelSell.SetActive(false);
            buy = false;
        }
        else
        {
            panelSell.SetActive(false);
            buy = false;
            
        }  

    }


}
