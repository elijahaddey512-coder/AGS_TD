
using UnityEngine;
using UnityEngine.AI;

public class BeeFlyingAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Flying")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float flyingHeight = 3f;
    [SerializeField] private float moveSpeed = 2f;
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

    private float groundHeight;
    private float nextAttackTime;
    private float attackAnimationEndTime;

    private bool isAttacking;
    private bool isMoving;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent != null)
        {
            // The script controls movement and rotation.
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }

    private void Start()
    {
        if (agent == null)
            return;

        // Record the ground NavMesh height.
        groundHeight = agent.nextPosition.y;

        // Move the Bee above the ground.
        Vector3 position = transform.position;
        position.y = groundHeight + flyingHeight;
        transform.position = position;
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
            agent.ResetPath();

            rotationDirection = Vector3.zero;

            PlayMovement(false);
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

        // Keep the NavMeshAgent's internal position on the ground.
        Vector3 navPosition = transform.position;
        navPosition.y = groundHeight;

        agent.nextPosition = navPosition;
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

        // Ask the NavMeshAgent to calculate the route.
        Vector3 targetPosition = currentTarget.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            targetPosition,
            out hit,
            3f,
            agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }

        // Follow the calculated path.
        Vector3 nextPoint = agent.steeringTarget;

        // Keep the Bee at its flying height.
        nextPoint.y = groundHeight + flyingHeight;

        Vector3 direction = nextPoint - transform.position;

        rotationDirection = direction;

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextPoint,
            moveSpeed * Time.deltaTime
        );

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








