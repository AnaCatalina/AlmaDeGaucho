using CamaraTerceraPersona;
using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CinematicaBehaviour : MonoBehaviour
{
    public VideoPlayer cinematica_1;

    public GameObject textoPanel;

    public AudioSource[] audios;
    //public Transform puntoControl;
    //public GameObject juan;

    public Transform player;
    private JuanMoveBehaviour playerMovement;
    private Controller playerController;
    private CamaraBahaviour camaraJugador;

    void Awake()
    {
        textoPanel.SetActive(false);
        foreach (AudioSource sonidos in audios)
        {
            sonidos.enabled = false;
        }
        cinematica_1 = GetComponent<VideoPlayer>();
        cinematica_1.Play();
        cinematica_1.loopPointReached += CheckOver;

        playerMovement = player.GetComponent<JuanMoveBehaviour>();
        playerController = player.GetComponent<Controller>();
        camaraJugador = player.GetComponent<CamaraBahaviour>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DetenerCinemática();
        }
    }

    public void CheckOver(VideoPlayer vp)
    {
        DetenerCinemática();
    }

    public void DetenerCinemática()
    {
        textoPanel.SetActive (true);
        foreach (AudioSource sonidos in audios)
        {
            sonidos.enabled = true;
        }
        //juan.transform.position = puntoControl.transform.position;
        playerMovement.enabled = true;
        playerMovement.atacando = false;
        playerController.enabled = true;
        camaraJugador.enabled = true;
        gameObject.SetActive(false);
    }
}
