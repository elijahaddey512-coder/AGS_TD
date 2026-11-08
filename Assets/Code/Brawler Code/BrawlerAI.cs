
using UnityEngine;
using UnityEngine.AI;

public class BrawlerAI : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private BrawlerHiring hiring;

    [Header("Combat")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackAnimationDuration = 0.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;

    private float nextAttackTime;
    private float attackEndTime;

    private bool isMoving;
    private bool isUsingSkill;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (hiring == null)
            hiring = GetComponent<BrawlerHiring>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.speed = moveSpeed;
            agent.isStopped = true;
        }
    }

    private void Update()
    {
        // Do nothing until hired.
        if (hiring == null || !hiring.IsHired)
            return;

        if (agent == null || !agent.enabled ||
            !agent.isOnNavMesh)
            return;

        if (isUsingSkill)
            return;

        // Wait for the attack animation to finish.
        if (Time.time < attackEndTime)
            return;

        // Never interrupt Attack, Hurt or Die.
        if (IsPlayingPriorityAnimation())
        {
            agent.isStopped = true;
            return;
        }

        currentTarget = FindNearestEnemy();

        if (currentTarget == null)
        {
            agent.isStopped = true;
            agent.ResetPath();

            PlayMovement(false);
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTarget.position
        );

        if (distance > attackRange)
        {
            MoveTowardsEnemy();
        }
        else
        {
            AttackEnemy();
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                detectionRange
            );

        Transform nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyObject in nearbyObjects)
        {
            if (!nearbyObject.CompareTag("Enemy"))
                continue;

            HealthSystem health =
                nearbyObject.GetComponentInParent<HealthSystem>();

            if (health == null || health.IsDead ||
                health.CurrentHealth <= 0)
                continue;

            Transform enemy = health.transform;

            float distance = Vector3.Distance(
                transform.position,
                enemy.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private void MoveTowardsEnemy()
    {
        agent.isStopped = false;

        agent.SetDestination(currentTarget.position);

        PlayMovement(true);

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        RotateTowards(direction);
    }

    private void AttackEnemy()
    {
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        RotateTowards(direction);

        if (Time.time < nextAttackTime)
        {
            PlayMovement(false);
            return;
        }

        HealthSystem enemyHealth =
            currentTarget.GetComponentInParent<HealthSystem>();

        if (enemyHealth == null || enemyHealth.IsDead)
            return;

        // Deal normal attack damage.
        enemyHealth.TakeDamage(damage);

        // Play Attack animation.
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Prevent another attack until cooldown finishes.
        nextAttackTime = Time.time + attackCooldown;

        // Prevent Idle or Run from interrupting Attack.
        isMoving = false;

        attackEndTime =
            Time.time + attackAnimationDuration;
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void PlayMovement(bool moving)
    {
        if (isMoving == moving)
            return;

        isMoving = moving;

        if (animator == null)
            return;

        animator.SetTrigger(moving ? "Run" : "Idle");
    }

    private bool IsPlayingPriorityAnimation()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(0);

        if (IsPriorityState(current))
            return true;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animator.GetNextAnimatorStateInfo(0);

            if (IsPriorityState(next))
                return true;
        }

        return false;
    }

    private bool IsPriorityState(AnimatorStateInfo state)
    {
        return state.shortNameHash ==
                   Animator.StringToHash("Attack")
            || state.shortNameHash ==
                   Animator.StringToHash("Hurt")
            || state.shortNameHash ==
                   Animator.StringToHash("Die");
    }

    public void SetUsingSkill(bool value)
    {
        isUsingSkill = value;

        if (agent != null && agent.enabled)
            agent.isStopped = value;

        if (value)
            isMoving = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

