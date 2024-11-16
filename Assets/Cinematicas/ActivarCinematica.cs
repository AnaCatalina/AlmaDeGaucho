using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CamaraTerceraPersona;
using SUPERCharacte;
using UnityEngine.UI;

public class ActivarCinematica : MonoBehaviour
{
    public RawImage cinematica2;

    public Transform player;
    private JuanMoveBehaviour playerMovement;
    private Controller playerController;
    private CamaraBahaviour camaraJugador;
    private bool primeraVez;
    // Start is called before the first frame update
    void Start()
    {
        primeraVez = true;
        playerMovement = player.GetComponent<JuanMoveBehaviour>();
        playerController = player.GetComponent<Controller>();
        camaraJugador = player.GetComponent<CamaraBahaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && primeraVez)
        {
            Debug.Log("CINEMATICA");
            playerController.enabled = false;
            playerMovement.atacando = true;
            //playerMovement.enabled = false;
            camaraJugador.enabled = false;
            cinematica2.gameObject.SetActive(true);
            primeraVez = false;
        }
    }
}
