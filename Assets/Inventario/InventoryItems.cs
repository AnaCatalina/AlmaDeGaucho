using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItems : MonoBehaviour
{
    public int amountFac;
    public Text amountFacText;
    public int amountBol;
    public Text amountBolText;
    public int amountMon;
    public Text amountMonText;
    public int amountDam;
    public Text amountDamText;
    public int amountCar;
    public Text amountCarText;
    public int amountYer;
    public Text amountYerText;
    public int amountCue;
    public Text amountCueText;
    public int amountCh;
    public Text amountChText;
    public GameObject panelInventario;
    public bool inventarioAbierto;


    // Start is called before the first frame update
    void Start()
    {
        inventarioAbierto = false;
        amountFac = 0;
        amountBol = 0;
        amountMon = 15;
        amountDam = 0;
        amountCar = 0;
        amountYer = 0;
        amountCue = 0;
        amountCh = 0;

    }

    // Update is called once per frame
    void Update()
    {
        amountMonText.text = "x " + amountMon.ToString();
        amountDamText.text = "x " + amountDam.ToString();
        amountCarText.text = "x " + amountCar.ToString();
        amountYerText.text = "x " + amountYer.ToString();
        amountCueText.text = "x " + amountCue.ToString();
        amountChText.text = "x " + amountCh.ToString();
        AbrirInventario();
    }

    void AbrirInventario()
    {
        if (Input.GetKey(KeyCode.I) && !inventarioAbierto)
        {
            inventarioAbierto = true;
            panelInventario.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

        }
        else if(Input.GetKey(KeyCode.I) && inventarioAbierto) {
        
            inventarioAbierto = false;
            panelInventario.SetActive(false);
        }
        /*if (Input.GetKeyDown(KeyCode.I))
        {
            panelInventario.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            panelInventario.SetActive(false);
        }*/
        
    }

    public void DesactivarCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other) 
    {
        Debug.Log("Detecto la colision");

         switch (other.gameObject.tag)
         {
            case "Facon":
                  amountFac += 1;
                  amountFacText.text = "x " + amountFac.ToString();
                  Destroy(other.gameObject);
                  break;

            case "Boleadora":
                 amountBol += 1;
                 amountBolText.text = "x " + amountBol.ToString();
                 Destroy(other.gameObject);
                 break;

            default:
                 Debug.Log("No se reconoce objeto");
                 break;
         }
    }
}
