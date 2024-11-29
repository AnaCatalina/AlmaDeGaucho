using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MisionesBehaviour : MonoBehaviour
{
    //public bool hasCinematica2;
    public GameObject cinematica3;
    //public ActivarCinematica cinematic;
    public TMP_Text textoMisiones;      // Asigna el texto donde se mostrarán mensajes
    //private string mensaje1 = "Dirígete a la pulpería";
    private string mensaje2 = "Vuelve a casa";

    private string mensaje3 = "Dirígete a la casa de Martin Fierro";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CambiarMensaje(int numCinematica)
    {
        switch (numCinematica)
        {
            case 2:
                {
                    textoMisiones.text = mensaje2;
                    break;
                }
            case 3:
                {
                    textoMisiones.text = mensaje3;
                    break;
                }
        }
        
    }
    public void HabilitarCinematica3()
    {
        cinematica3.SetActive(true);
    }
}
