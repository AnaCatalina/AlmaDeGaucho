using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarpinchoBehaviourScript : AnimalBehaviourScript
{
    public float fleeDistance = 15f;
    public float detectionRange = 16f;
    public Transform player;
    public List<Transform> patrolPoints;
    private Animator animator;
    private bool isFleeing = false;

    private bool isStunned = false;
    private bool isPatrol = false;
    public float stunDuration = 5f; // Duraciom stun

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();

        entityName = "Niandus";
        health = 100f;
        //speed = 20f;
        meatAmount = 10;
        leatherAmount = 5;

        

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
            return;
        }

        StartCoroutine(Patrol());
    }


    void Update()
    {
        Debug.Log("destino "+ agent.destination);
        if (!isFleeing && !isStunned)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRuning", false);

        }


        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer < fleeDistance && !isFleeing)
        {
            StopCoroutine(Patrol());
            isFleeing = true;

            Flee();
        }
        else if (isFleeing && distanceToPlayer >= detectionRange)
        {
            isFleeing = false;
            StartCoroutine(Patrol());
        }
        if (agent.isStopped)
        {
            speed = 0;
            
        }

        agent.speed = speed;
        animator.SetFloat("Vel",agent.speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("boleadora"))
        {
            StunAnimal();
        }
    }

    private void StunAnimal()
    {
        if (isStunned) return;

        isStunned = true;
        animator.SetBool("isRuning", false);
        animator.SetBool("isWalking", false);
        agent.isStopped = true;

        StartCoroutine(StunTimer());
    }

    private IEnumerator StunTimer()
    {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        agent.isStopped = false;

        if (!isFleeing)
        {
            StartCoroutine(Patrol());
        }
        else
        {
            Flee();
        }
    }

    private IEnumerator Patrol()
    {
        while (!isFleeing && !isStunned)
        {
            if (patrolPoints.Count == 0) yield break;

            int randomIndex = Random.Range(0, patrolPoints.Count);
            //Vector3 patrolDestination = patrolPoints[randomIndex].position;

            speed = 2f;
            animator.SetBool("isWalking", true);
            // da el destino y activa la animación
            agent.SetDestination(patrolPoints[randomIndex].position);



            // espera hasta el destino

            while (Vector3.Distance(transform.position, patrolPoints[randomIndex].position) > 2f)
            {
                agent.isStopped = false;
                yield return null;
                //Debug.Log("Entrada While"+ Vector3.Distance(transform.position, patrolPoints[randomIndex].position));

            }
            agent.isStopped = true;
            // animación de caminar y activa iddle
            animator.SetBool("isWalking", false);
            animator.SetBool("isRuning", false);
            yield return new WaitForSeconds(3f); //espera2 seg mientras esta en idle

        }
    }


    private void Flee()
    {
        animator.SetBool("isRuning", true);
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + direction * fleeDistance*3;
        speed = 17f;
        agent.SetDestination(fleePosition);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }

}
