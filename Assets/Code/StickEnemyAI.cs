
using UnityEngine;
using UnityEngine.AI;

public class StickEnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float startingSpeed = 1.5f;
    [SerializeField] private float increasedSpeed = 4f;
    [SerializeField] private float speedIncreaseDelay = 5f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Model Rotation")]
    [SerializeField] private float modelForwardOffset = 0f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackAnimationDuration = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;

    private Vector3 rotationDirection;

    private float nextAttackTime;
    private float attackAnimationEndTime;
    private float movementStartTime;

    private bool isAttacking;
    private bool isMoving;
    private bool speedIncreased;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent != null)
        {
            agent.updateRotation = false;
        }
    }

    private void Start()
    {
        if (agent != null)
        {
            agent.speed = startingSpeed;
        }
    }

    private void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (isAttacking && Time.time >= attackAnimationEndTime)
        {
            isAttacking = false;
        }

        currentTarget = FindTarget();

        if (currentTarget == null)
        {
            agent.isStopped = true;
            rotationDirection = Vector3.zero;
            PlayMovement(false);

            // Reset the speed timer when no target is available.
            movementStartTime = 0f;
            speedIncreased = false;
            agent.speed = startingSpeed;

            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTarget.position
        );

        if (distance > attackRange)
        {
            MoveTowardsTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    private void LateUpdate()
    {
        FaceDirection(rotationDirection);
    }

    private Transform FindTarget()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(transform.position, detectionRange);

        Transform nearestNPC = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyObject in nearbyObjects)
        {
            if (!nearbyObject.CompareTag("NPC"))
                continue;

            HealthSystem health =
                nearbyObject.GetComponentInParent<HealthSystem>();

            if (health == null || health.IsDead ||
                health.CurrentHealth <= 0)
                continue;

            Transform target = health.transform;

            float distance = Vector3.Distance(
                transform.position,
                target.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestNPC = target;
            }
        }

        if (nearestNPC != null)
            return nearestNPC;

        if (player != null)
        {
            HealthSystem playerHealth =
                player.GetComponentInParent<HealthSystem>();

            if (playerHealth != null &&
                !playerHealth.IsDead &&
                playerHealth.CurrentHealth > 0)
            {
                float playerDistance = Vector3.Distance(
                    transform.position,
                    player.position
                );

                if (playerDistance <= detectionRange)
                    return playerHealth.transform;
            }
        }

        return null;
    }

    private void MoveTowardsTarget()
    {
        agent.isStopped = false;

        agent.SetDestination(currentTarget.position);

        // Start the timer when the Stick begins moving.
        if (!isMoving)
        {
            movementStartTime = Time.time;
            speedIncreased = false;
        }

        // Increase speed after 5 seconds of movement.
        if (!speedIncreased &&
            Time.time >= movementStartTime + speedIncreaseDelay)
        {
            agent.speed = increasedSpeed;
            speedIncreased = true;

            Debug.Log("Stick has increased its movement speed!");
        }

        // Face the actual movement direction.
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            rotationDirection = agent.velocity;
        }
        else if (agent.desiredVelocity.sqrMagnitude > 0.01f)
        {
            rotationDirection = agent.desiredVelocity;
        }
        else
        {
            rotationDirection =
                agent.steeringTarget - transform.position;
        }

        PlayMovement(true);
    }

    private void AttackTarget()
    {
        agent.isStopped = true;

        rotationDirection =
            currentTarget.position - transform.position;

        PlayMovement(false);

        if (Time.time >= nextAttackTime && !isAttacking)
        {
            HealthSystem targetHealth =
                currentTarget.GetComponentInParent<HealthSystem>();

            if (targetHealth != null && !targetHealth.IsDead)
            {
                targetHealth.TakeDamage(damage);
            }

            nextAttackTime = Time.time + attackCooldown;

            isAttacking = true;

            attackAnimationEndTime =
                Time.time + attackAnimationDuration;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        targetRotation *= Quaternion.Euler(
            0f,
            modelForwardOffset,
            0f
        );

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

        if (moving && animator != null)
        {
            animator.SetTrigger("Move");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

