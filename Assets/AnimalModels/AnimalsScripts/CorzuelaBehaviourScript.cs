using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorzuelaBehaviourScript : AnimalBehaviourScript
{
    public float fleeDistance = 14f;
    public float detectionRange = 20f;
    public Transform player;
    public List<Transform> patrolPoints;
    private Animator animator;
    private bool isFleeing = false;

    private bool isStunned = false;
    //private bool isPatrol = false;
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
        //Debug.Log("destino " + agent.destination);
        if (!isFleeing && !isStunned)
        {
            //animator.SetBool("isWalking", false);
            //animator.SetBool("isRuning", false);
            animator.SetBool("lookAround", true);

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
            animator.SetBool("isRuning", true);
            isFleeing = false;
            StartCoroutine(Patrol());
        }
        if (agent.isStopped)
        {
            speed = 0;

        }

        agent.speed = speed;
        //animator.SetFloat("Vel", agent.speed);
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
        //animator.SetBool("isRuning", false);
        //animator.SetBool("isWalking", false);
        animator.SetBool("stand", true);
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

            speed = 10f;
            //animator.SetBool("lookAround", false);
            //animator.SetBool("stand", false);
            
            // da el destino y activa la animación
            agent.SetDestination(patrolPoints[randomIndex].position);



            // espera hasta el destino

            while (Vector3.Distance(transform.position, patrolPoints[randomIndex].position) > 2f)
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("isRuning", true);
                agent.isStopped = false;
                yield return null;
                //Debug.Log("Entrada While"+ Vector3.Distance(transform.position, patrolPoints[randomIndex].position));

            }
            agent.isStopped = true;
            // animación de caminar y activa iddle
            //animator.SetBool("isWalking", false);
            animator.SetBool("isRuning", false);
            animator.SetBool("lookAround", true);
            yield return new WaitForSeconds(7f); //espera2 seg mientras esta en idle

        }
    }


    private void Flee()
    {;
        //animator.SetBool("stand", true);
        animator.SetBool("lookAround", false);
        animator.SetBool("isRuning", true);
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + direction * detectionRange;
        speed = 13f;
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
