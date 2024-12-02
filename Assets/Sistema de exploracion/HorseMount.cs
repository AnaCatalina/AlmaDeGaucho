using CamaraTerceraPersona;
using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HorseMount : MonoBehaviour
{
    public Transform player; // Referencia al player
    public Transform mountPoint; // El punto donde el player se posicionará al montar
    public float mountDistance = 2f; // Distancia máxima para poder montar

    private NavMeshAgent agent;
    private HorseFollowNavMesh followNavMesh;
    private MovementHorse movementHorse;

    private bool isMounted = false;
    private JuanMoveBehaviour playerMovement; // Referencia al script de movimiento del player
    private Rigidbody playerRb;
    private CapsuleCollider playerCollider;
    private Controller playerController;
    public Animator playerAnimator;
    //private CamaraBahaviour camaraJugador;

    void Start()
    {
        playerMovement = player.GetComponent<JuanMoveBehaviour>();
        playerRb = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<CapsuleCollider>();
        playerController = player.GetComponent<Controller>();
        //camaraJugador = player.GetComponent<CamaraBahaviour>();
        agent = GetComponent<NavMeshAgent>();
        followNavMesh = GetComponent<HorseFollowNavMesh>();
        movementHorse = GetComponent<MovementHorse>();  
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Montar el caballo
        if (distanceToPlayer <= mountDistance && Input.GetKeyDown(KeyCode.E) && !isMounted && !playerMovement.tengoBoleadoras && !playerMovement.tengoFacon && playerMovement.isIdle && playerMovement.currentGroundInfo.isInContactWithGround)
        {
            MountHorse();
            playerController.puedoUsarMenu = false;
        }

        // Desmontar el caballo
        if (isMounted && Input.GetKeyDown(KeyCode.Q))
        {
            DismountHorse();
            playerController.puedoUsarMenu = true;
        }
    }

    void MountHorse()
    {
        // Desactivar el movimiento del player
        playerMovement.enabled = false;
        //player.parent = transform; // Hacer que el player sea hijo del caballo

        playerRb.isKinematic = true; // Desactivar la física del player
        playerCollider.enabled = false; // Desactivar colisiones del player

        playerAnimator.SetBool("Sentado", true);

        // Posicionar al player en el punto de montar
        player.position = mountPoint.position;
        player.rotation = mountPoint.rotation;

        player.parent = transform; // Hacer que el player sea hijo del caballo
        isMounted = true;

        // Habilitar el control del caballo
        agent.enabled = false;
        followNavMesh.enabled = false;
        movementHorse.enabled = true;
        movementHorse.rb.isKinematic = false;
        //camaraJugador.playerCamera = camaraJugador.horseCamera;
        //camaraJugador.pCamera.enabled = false;
        //camaraJugador.horseCamera.enabled = true;
        //playerAnimator.SetBool("Sentado", true);
    }

    void DismountHorse()
    {
        isMounted = false;
        playerRb.isKinematic = false; // Reactivar la física del player
        playerCollider.enabled = true; // Reactivar colisiones del player

        // Desactivar el control del caballo
        movementHorse.enabled = false;
        agent.enabled = true;
        followNavMesh.enabled = true;

        // Permitir que el player se mueva de nuevo
        playerMovement.enabled = true;
        player.parent = null; // Separar al player del caballo

        //camaraJugador.pCamera.enabled = true;
        //camaraJugador.horseCamera.enabled = false;
        //camaraJugador.playerCamera = camaraJugador.pCamera;

        playerAnimator.SetBool("Sentado", false);

        // Colocar al player al lado del caballo al desmontar
        player.position = transform.position + transform.right * 2 + transform.up * 2;
        movementHorse.rb.isKinematic = true;
    }
}
