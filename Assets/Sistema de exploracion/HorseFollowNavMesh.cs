using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HorseFollowNavMesh : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public float stopDistance = 6f; // Distancia mínima para detenerse
    public float maxDistance = 20f; // Distancia máxima para correr
    public float walkSpeed = 2f; // Velocidad caminando
    public float trotSpeed = 6f; // Velocidad trotando
    public float runSpeed = 12f; // Velocidad corriendo

    private NavMeshAgent agent;
    private bool isFollowing = false;

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Obtén el componente NavMeshAgent
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            isFollowing = !isFollowing; // Alternar seguimiento
        }

        if (isFollowing)
        {
            FollowPlayerWithNavMesh();
        }
        else
        {
            agent.isStopped = true; // Detener el movimiento si no está siguiendo

        }

        if (agent.isStopped) 
        {
            agent.speed = 0;
        }
        animator.SetFloat("Vel", agent.speed);
        //Debug.Log("Velocidad agente: " + agent.speed);
        
    }

    void FollowPlayerWithNavMesh()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stopDistance)
        {
            agent.isStopped = true; // Detener el agente si está cerca
            return;
        }
        else
        {
            agent.isStopped = false; // Permitir que el agente se mueva
        }

        // Determinar la velocidad según la distancia
        if (distance > maxDistance * 0.75f)
        {
            agent.speed = runSpeed; // Correr si está muy lejos
        }
        else if (distance > maxDistance * 0.5f)
        {
            agent.speed = trotSpeed; // Trotar si está en una distancia media
        }
        else
        {
            agent.speed = walkSpeed; // Caminar si está relativamente cerca
        }

        // Establecer el destino al jugador
        agent.SetDestination(player.position);
    }
}
