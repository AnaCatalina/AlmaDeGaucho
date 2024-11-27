using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NiandusBehaviourScript : AnimalBehaviourScript
{
    public float fleeDistance = 10f;
    public Transform player;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        entityName = "Niandus";
        health = 100f;
        speed = 5f;
        meatAmount = 10;
        leatherAmount = 5;

        //comprobacion de Player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        float distanceToInitial = Vector3.Distance(initialPosition, transform.position);

        if (distanceToPlayer < fleeDistance)
        {
            Flee();
            //Debug.Log("En huida");
        }
        else if (distanceToInitial > 1f)
        {
            ReturnToInitialPosition();
            //Debug.Log("De regreso al punto inicial");
        }
        else
        {
            //Debug.Log("En posicion de idle o reposo");
            //Idle();
        }
    }

    /*protected override void Idle()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("iddle", true);

    }*/

    /*protected override void Run()
    {
        animator.SetBool("idle", false);
        animator.SetBool("IsRunning", true);
    }*/

    private void Flee()
    {
        //Run();
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + direction * fleeDistance;
        if (fleePosition != null)
        {
            agent.SetDestination(fleePosition);
            Debug.Log("De camino a destino");

        }
        
    }

    private void ReturnToInitialPosition()
    {
        //Run();
        agent.SetDestination(initialPosition);
    }
}
