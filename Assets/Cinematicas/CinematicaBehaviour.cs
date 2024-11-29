using CamaraTerceraPersona;
using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CinematicaBehaviour : MonoBehaviour
{
    public VideoPlayer cinematica_1;

    public Transform puntoControl;
    public GameObject juan;

    public Transform player;
    private JuanMoveBehaviour playerMovement;
    private Controller playerController;
    private CamaraBahaviour camaraJugador;

    void Awake()
    {
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
        
    }

    public void CheckOver(VideoPlayer vp)
    {
        juan.transform.position = puntoControl.transform.position;
        playerMovement.enabled = true;
        playerMovement.atacando = false;
        playerController.enabled = true;
        camaraJugador.enabled = true;
        gameObject.SetActive(false);
    }
}
