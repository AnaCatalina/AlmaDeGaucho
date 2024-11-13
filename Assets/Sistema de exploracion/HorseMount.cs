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

    private bool isMounted = false;
    private JuanMoveBehaviour playerMovement; // Referencia al script de movimiento del player
    private Rigidbody playerRb;
    private CapsuleCollider playerCollider;
    private Controller playerController;
    public Animator playerAnimator;

    void Start()
    {
        playerMovement = player.GetComponent<JuanMoveBehaviour>();
        playerRb = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<CapsuleCollider>();
        playerController = player.GetComponent<Controller>();
        agent = GetComponent<NavMeshAgent>();
        followNavMesh = GetComponent<HorseFollowNavMesh>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Montar el caballo
        if (distanceToPlayer <= mountDistance && Input.GetKeyDown(KeyCode.E) && !isMounted && !playerMovement.tengoBoleadoras && !playerMovement.tengoFacon)
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
        isMounted = true;
        playerRb.isKinematic = true; // Desactivar la física del player
        playerCollider.enabled = false; // Desactivar colisiones del player

        // Posicionar al player en el punto de montar
        player.position = mountPoint.position;
        player.rotation = mountPoint.rotation;

        // Desactivar el movimiento del player
        playerMovement.enabled = false;
        player.parent = transform; // Hacer que el player sea hijo del caballo

        // Habilitar el control del caballo
        agent.enabled = false;
        followNavMesh.enabled = false;
        GetComponent<MovementHorse>().enabled = true;
        playerAnimator.SetBool("Sentado", true);
    }

    void DismountHorse()
    {
        isMounted = false;
        playerRb.isKinematic = false; // Reactivar la física del player
        playerCollider.enabled = true; // Reactivar colisiones del player

        // Desactivar el control del caballo
        GetComponent<MovementHorse>().enabled = false;
        agent.enabled = true;
        followNavMesh.enabled = true;

        // Permitir que el player se mueva de nuevo
        playerMovement.enabled = true;
        player.parent = null; // Separar al player del caballo
        playerAnimator.SetBool("Sentado", false);

        // Colocar al player al lado del caballo al desmontar
        player.position = transform.position + transform.right * 2 + transform.up * 2;
    }
}
