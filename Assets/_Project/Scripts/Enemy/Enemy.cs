using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;

    public Transform player;
    public float attackRange = 2f;
    public float chaseRange = 10f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    public Transform[] patrolPoints;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;

    public Image healthBarFill;
    public Transform uiContainer;

    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float currentWaitDuration = 0f;

    private float lastAttackTime;
    private Animator animator;
    private bool isDead = false;
    private Camera mainCamera;
    private PlayerHealth playerHealth;

    private NavMeshAgent agent;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null || isDead) return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            if (animator != null) animator.SetBool("IsWalking", false);
            Patrol();
            return;
        }

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack") || stateInfo.IsName("Hit"))
            {
                agent.isStopped = true;
                return;
            }
        }

        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            if (animator != null) animator.SetBool("IsWalking", false);

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void LateUpdate()
    {
        if (uiContainer != null && mainCamera != null)
        {
            uiContainer.LookAt(uiContainer.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            if (animator != null) animator.SetBool("IsWalking", false);
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= currentWaitDuration)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (animator != null) animator.SetBool("IsWalking", true);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            isWaiting = true;
            waitTimer = 0f;
            currentWaitDuration = Random.Range(minWaitTime, maxWaitTime);

            if (animator != null) animator.SetBool("IsWalking", false);
        }
    }

    private void ChasePlayer()
    {
        isWaiting = false;
        agent.SetDestination(player.position);

        if (animator != null) animator.SetBool("IsWalking", true);
    }

    private void AttackPlayer()
    {
        isWaiting = false;
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void ApplyDamage()
    {
        if (player == null || playerHealth == null || playerHealth.IsDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange + 0.5f)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    private void Die()
    {
        isDead = true;

        if (uiContainer != null)
        {
            uiContainer.gameObject.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (agent != null)
        {
            agent.enabled = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, 3f);
    }
}