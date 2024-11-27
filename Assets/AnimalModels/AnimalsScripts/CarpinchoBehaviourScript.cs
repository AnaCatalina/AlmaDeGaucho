using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarpinchoBehaviourScript : AnimalBehaviourScript
{
    public float fleeDistance = 10f;
    public float detectionRange = 15f;
    public Transform player;
    public List<Transform> patrolPoints;
    private Animator animator;
    private bool isFleeing = false;

    private bool isStunned = false;
    public float stunDuration = 5f; // Duraciom stun

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();

        entityName = "Niandus";
        health = 100f;
        speed = 20f;
        meatAmount = 10;
        leatherAmount = 5;

        agent.speed = speed;

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
        if (!isFleeing && !isStunned && !agent.hasPath)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRuning", false);
        }

        if (isStunned) return;

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
            agent.speed = 3f;
            agent.SetDestination(patrolPoints[randomIndex].position);
            animator.SetBool("isWalking", true);

            while (Vector3.Distance(transform.position, patrolPoints[randomIndex].position) > 1f)
            {
                yield return null;
            }

            animator.SetBool("isWalking", false);

            yield return new WaitForSeconds(2f);
        }
    }

    private void Flee()
    {
        animator.SetBool("isRuning", true);
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + direction * fleeDistance;
        agent.speed = 10f;
        agent.SetDestination(fleePosition);
    }
}
