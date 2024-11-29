using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IndianBehaviourScript : EnemyBehaviourScript
{
    private Transform player;
    
    public float detectionRadius = 10.0f;
    public float attackRadius = 2f; // rango de ataque
    public float stopDistance = 3f; // distancia a la que se detendrá antes de atacar
    public float attackDamage = 10f; // daño
    public float attackCooldown = 1f; // tiempo de espera de ataques
    public Animator animator;
    public bool ataco;

    private bool isAttacking = false;
    private float lastAttackTime;

    public Transform[] pointsPatrol;
    private int currentPosition;

    //public Animator animEnemy;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //animEnemy = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }
        animator = GetComponent<Animator>(); 
        ataco = false;

        currentPosition = Random.Range(0, pointsPatrol.Length);
        animator.SetBool("isMove", true);
        agent.isStopped = false;
        agent.SetDestination(pointsPatrol[currentPosition].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            if (player == null || agent == null)
            {
                return;
            }

            if (agent.remainingDistance < 0.1) //este if lo agrege recien
            {
                currentPosition = Random.Range(0, pointsPatrol.Length);
                animator.SetBool("isMove", true);
                agent.isStopped = false;
                agent.SetDestination(pointsPatrol[currentPosition].position);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // dentro del radio de detección se acerca
            if (distanceToPlayer < detectionRadius && distanceToPlayer > stopDistance && !ataco)
            {
                //isAttacking = false; 
                animator.SetBool("isMove", true);
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer <= stopDistance)// se detiene a cierta distancia del jugador y ataca
            {
                agent.isStopped = true;
                if (distanceToPlayer <= attackRadius && !ataco/*&& !isAttacking*/)
                {
                    ataco = true;
                    isAttacking = true;
                    animator.SetBool("isMove", false);
                    animator.SetTrigger("atack");
                    //StartCoroutine(AttackPlayer());
                }
            }
            else
            {
                // se detiene si el jugador está fuera del rango de detección
                isAttacking = false;
                //agent.isStopped = true;  
                //animator.SetBool("isMove", false);
                animator.SetBool("isMove", true);
                agent.SetDestination(pointsPatrol[currentPosition].position); //esto tambien lo agrege recien
            }
            animator.SetBool("attack", isAttacking);
        }
        else
        {
            animator.SetTrigger("isDead");
            isDead = false;
        }
        
    }

    private void MoveTowardsPlayer()
    {
        
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private IEnumerator AttackPlayer()
    {
        if (isAttacking)
        {
            Debug.Log("atacando");

            if (Time.time > lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
            yield return null;
        }
    }
    public void DejadeAtacarIndio()
    {
        ataco = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
