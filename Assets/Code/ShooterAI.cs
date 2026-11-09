
using UnityEngine;
using UnityEngine.AI;

public class ShooterAI : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private UniversalHiring hiring;

    [Header("Combat")]
    [SerializeField] private float detectionRange = 35f;
    [SerializeField] private float shootingRange = 25f;
    [SerializeField] private float groundDamage = 30f;
    [SerializeField] private float flyingDamage = 80f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float shootingAnimationDuration = 0.7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 240f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;

    private float nextAttackTime;
    private float shootingEndTime;

    private bool isMoving;
    private bool hasCommandDestination;

    private Vector3 commandDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (hiring == null)
            hiring = GetComponent<UniversalHiring>();

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
        if (hiring == null || !hiring.IsHired)
            return;

        if (agent == null || !agent.enabled ||
            !agent.isOnNavMesh)
            return;

        if (hasCommandDestination)
        {
            FollowCommandDestination();
            return;
        }

        if (Time.time < shootingEndTime)
            return;

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

        if (distance > shootingRange)
        {
            MoveTowardsEnemy();
        }
        else
        {
            ShootEnemy();
        }
    }

    // =====================================
    // FIND NEAREST ENEMY
    // =====================================

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
            if (!HasEnemyTag(nearbyObject.transform))
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

    private bool HasEnemyTag(Transform target)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag("Enemy"))
                return true;

            current = current.parent;
        }

        return false;
    }

    // =====================================
    // MOVEMENT
    // =====================================

    private void MoveTowardsEnemy()
    {
        agent.isStopped = false;

        Vector3 targetPosition = currentTarget.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            targetPosition,
            out hit,
            10f,
            agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }

        PlayMovement(true);

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        RotateTowards(direction);
    }

    // =====================================
    // SHOOT ENEMY
    // =====================================

    private void ShootEnemy()
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

        if (enemyHealth == null || enemyHealth.IsDead ||
            enemyHealth.CurrentHealth <= 0)
            return;

        BeeFlyingAI bee =
            currentTarget.GetComponentInParent<BeeFlyingAI>();

        float damageToDeal =
            bee != null ? flyingDamage : groundDamage;

        // Identify the Shooter as the attacker.
        enemyHealth.TakeDamage(damageToDeal, transform);

        Debug.Log(
            "Shooter fired at " + currentTarget.name +
            " for " + damageToDeal + " damage!"
        );

        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Run");
            animator.ResetTrigger("Shoot");

            animator.SetTrigger("Shoot");
        }

        nextAttackTime = Time.time + attackCooldown;

        isMoving = true;

        shootingEndTime =
            Time.time + shootingAnimationDuration;
    }

    // =====================================
    // ROTATION
    // =====================================

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

    // =====================================
    // ANIMATION
    // =====================================

    private void PlayMovement(bool moving)
    {
        if (isMoving == moving)
            return;

        isMoving = moving;

        if (animator == null)
            return;

        if (moving)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Run");
        }
        else
        {
            animator.ResetTrigger("Run");
            animator.SetTrigger("Idle");
        }
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
                   Animator.StringToHash("Hurt")
            || state.shortNameHash ==
                   Animator.StringToHash("Die");
    }

    // =====================================
    // COMMAND MODE MOVEMENT
    // =====================================

    public void SetCommandDestination(Vector3 destination)
    {
        if (hiring == null || !hiring.IsHired)
            return;

        if (agent == null || !agent.enabled ||
            !agent.isOnNavMesh)
            return;

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            destination,
            out hit,
            2f,
            agent.areaMask))
            return;

        if (!agent.SetDestination(hit.position))
            return;

        commandDestination = hit.position;
        hasCommandDestination = true;

        agent.isStopped = false;

        PlayMovement(true);

        Debug.Log("Shooter received movement command!");
    }

    private void FollowCommandDestination()
    {
        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            !agent.hasPath)
        {
            hasCommandDestination = false;

            agent.isStopped = true;

            PlayMovement(false);

            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            hasCommandDestination = false;

            agent.isStopped = true;
            agent.ResetPath();

            PlayMovement(false);

            Debug.Log("Shooter reached destination!");
            return;
        }

        agent.isStopped = false;

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        RotateTowards(direction);

        PlayMovement(true);
    }

    // =====================================
    // GIZMOS
    // =====================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            shootingRange
        );
    }
}


