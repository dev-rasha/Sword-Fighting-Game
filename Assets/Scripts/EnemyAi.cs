using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    // [SerializeField]
    // private int enemyCount = 12;

    // public int EnemyCount
    // {
    //     get { return enemyCount; }
    //     set { enemyCount = value; }
    // }

    public static int enemyCount = 12;

    public NavMeshAgent agent;

    public PlayerController p;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    public GameObject GameOverUI;

    public float attackDelay = 0.4f;

    public int attackDamage = 10;

    public Animator animator;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool readytoAttack = true;
    bool Attacking = false;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    void Start()
    {
        GameOverUI.SetActive(false);
    }

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();

    SetAnimations();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "Idle";
    public const string ATTACKING = "Attack";
    
    string currentEnemyAnimationState;

    public void ChangeEnemyAnimationState(string newState) 
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentEnemyAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentEnemyAnimationState = newState;
        animator.CrossFadeInFixedTime(currentEnemyAnimationState, 0.1f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if(!Attacking)
        {
            ChangeEnemyAnimationState(IDLE);
        }
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if(!readytoAttack || Attacking) return;

        readytoAttack = false;
        Attacking = true;
        
        if(Attacking)
        {
            ChangeEnemyAnimationState(ATTACKING);
            DamageThePlayer();
        }
        else
        {
            ChangeEnemyAnimationState(IDLE);
        }

        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void ResetAttack()
    {
        readytoAttack = true;
        Attacking = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) 
        Invoke(nameof(DestroyEnemy), 0.1f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);

        enemyCount -= 1;

        if(enemyCount == 0)
        {
            GameOverUI.SetActive(true);
        }

         Debug.Log("Current enemyCount: " + enemyCount);
    }

    void DamageThePlayer()
    {
        // p = GetComponent<PlayerController>();
        p = FindObjectOfType<PlayerController>();
        p.DamagePlayer(attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
