
using UnityEngine;
using UnityEngine.AI;

public class StickEnemyAI : MonoBehaviour
{
    [Header("Target")]
    private Transform player;

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

    [Header("Retaliation")]
    [SerializeField] private float retaliationMemoryDuration = 5f;

    [Header("Shielder Aggro")]
    [Range(0f, 1f)]
    [SerializeField] private float shielderAggroChance = 0.8f;

    [Header("Shielder Reflection")]
    [SerializeField] private float reflectedDamage = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private Transform lastShielderCandidate;

    private bool prioritizeShielder;

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
        FindPlayer();

        if (agent != null)
        {
            agent.speed = startingSpeed;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Stick could not find the Player!");
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

    // TARGET SELECTION
    private Transform FindTarget()
    {
        // PRIORITY 1: Shielder (80% chance)
        Transform nearestShielder = FindNearestShielder();

        if (nearestShielder == null)
        {
            lastShielderCandidate = null;
            prioritizeShielder = false;
        }
        else if (nearestShielder != lastShielderCandidate)
        {
            lastShielderCandidate = nearestShielder;

            prioritizeShielder =
                Random.value <= shielderAggroChance;
        }

        if (nearestShielder != null && prioritizeShielder)
        {
            return nearestShielder;
        }

        // PRIORITY 2: Recently attacked by a hired NPC
        HealthSystem enemyHealth = GetComponent<HealthSystem>();

        if (enemyHealth != null && enemyHealth.LastAttacker != null)
        {
            float timeSinceAttack =
                Time.time - enemyHealth.LastAttackedTime;

            if (timeSinceAttack <= retaliationMemoryDuration)
            {
                UniversalHiring hiring =
                    enemyHealth.LastAttacker
                    .GetComponentInParent<UniversalHiring>();

                if (hiring != null && hiring.IsHired)
                {
                    HealthSystem attackerHealth =
                        hiring.GetComponent<HealthSystem>();

                    if (attackerHealth != null &&
                        !attackerHealth.IsDead &&
                        attackerHealth.CurrentHealth > 0)
                    {
                        return attackerHealth.transform;
                    }
                }
            }
        }

        // PRIORITY 3: Find nearest alive hired NPC
        Collider[] nearbyObjects =
            Physics.OverlapSphere(transform.position, detectionRange);

        Transform nearestNPC = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyObject in nearbyObjects)
        {
            UniversalHiring hiring =
                nearbyObject.GetComponentInParent<UniversalHiring>();

            if (hiring == null || !hiring.IsHired)
                continue;

            if (hiring.GetComponent<ShielderAI>() != null)
                continue;

            HealthSystem health =
                hiring.GetComponent<HealthSystem>();

            if (health == null ||
                health.IsDead ||
                health.CurrentHealth <= 0)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                health.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestNPC = health.transform;
            }
        }

        if (nearestNPC != null)
        {
            return nearestNPC;
        }

        // PRIORITY 4: Player
        if (player != null)
        {
            HealthSystem playerHealth =
                player.GetComponentInParent<HealthSystem>();

            if (playerHealth != null &&
                !playerHealth.IsDead &&
                playerHealth.CurrentHealth > 0)
            {
                return playerHealth.transform;
            }
        }

        return null;
    }

    private Transform FindNearestShielder()
    {
        ShielderAI[] shielders =
            FindObjectsByType<ShielderAI>();

        Transform nearestShielder = null;
        float nearestDistance = Mathf.Infinity;

        foreach (ShielderAI shielder in shielders)
        {
            if (shielder == null)
                continue;

            UniversalHiring hiring =
                shielder.GetComponent<UniversalHiring>();

            HealthSystem health =
                shielder.GetComponent<HealthSystem>();

            if (hiring == null || !hiring.IsHired)
                continue;

            if (health == null ||
                health.IsDead ||
                health.CurrentHealth <= 0)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                health.transform.position
            );

            if (distance <= detectionRange &&
                distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestShielder = health.transform;
            }
        }

        return nearestShielder;
    }

    private void MoveTowardsTarget()
    {
        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);

        if (!isMoving)
        {
            movementStartTime = Time.time;
            speedIncreased = false;
            agent.speed = startingSpeed;
        }

        if (!speedIncreased &&
            Time.time >= movementStartTime + speedIncreaseDelay)
        {
            agent.speed = increasedSpeed;
            speedIncreased = true;

            Debug.Log("Stick has increased its movement speed!");
        }

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

            if (targetHealth != null &&
                !targetHealth.IsDead &&
                targetHealth.CurrentHealth > 0)
            {
                targetHealth.TakeDamage(damage, transform);

                // Reflect damage when attacking the Shielder
                if (currentTarget.GetComponentInParent<ShielderAI>() != null)
                {
                    HealthSystem stickHealth = GetComponent<HealthSystem>();

                    if (stickHealth != null && !stickHealth.IsDead)
                    {
                        stickHealth.TakeDamage(reflectedDamage, currentTarget);
                    }
                }
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




