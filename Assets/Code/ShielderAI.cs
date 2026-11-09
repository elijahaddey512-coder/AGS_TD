
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ShielderAI : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private UniversalHiring hiring;

    [Header("Charge Skill")]
    [SerializeField] private KeyCode chargeKey = KeyCode.Alpha4;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chargeDistance = 5f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float chargeCooldown = 10f;

    [Header("Charge Damage")]
    [SerializeField] private float chargeDamage = 20f;
    [SerializeField] private float impactRadius = 1.5f;
    [SerializeField] private float knockbackForce = 4f;

    [Header("Enemy Aggro")]
    [Range(0f, 1f)]
    [SerializeField] private float aggroChance = 0.3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private HealthSystem ownHealth;

    private bool isCharging;
    private float nextChargeTime;

    public float CooldownRemaining =>
        Mathf.Max(0f, nextChargeTime - Time.time);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ownHealth = GetComponent<HealthSystem>();

        if (hiring == null)
            hiring = GetComponent<UniversalHiring>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (ownHealth != null && ownHealth.IsDead)
            return;

        if (hiring == null || !hiring.IsHired)
            return;

        if (isCharging)
            return;

        if (Input.GetKeyDown(chargeKey))
        {
            ActivateCharge();
        }

        UpdateMovementAnimation();
    }

    // =====================================
    // CHARGE SKILL
    // =====================================

    private void ActivateCharge()
    {
        if (Time.time < nextChargeTime)
        {
            Debug.Log("Shielder charge is on cooldown!");
            return;
        }

        if (agent == null || !agent.enabled ||
            !agent.isOnNavMesh)
        {
            Debug.LogWarning("Shielder has no active NavMeshAgent!");
            return;
        }

        Transform target = FindNearestEnemy();

        if (target == null)
        {
            Debug.Log("Shielder cannot find an enemy to charge!");
            return;
        }

        nextChargeTime = Time.time + chargeCooldown;

        StartCoroutine(ChargeRoutine(target));
    }

    private IEnumerator ChargeRoutine(Transform target)
    {
        isCharging = true;

        Vector3 originalPosition = transform.position;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            isCharging = false;
            yield break;
        }

        direction.Normalize();

        // Face the enemy.
        transform.rotation =
            Quaternion.LookRotation(direction);

        // Calculate the charge destination.
        Vector3 desiredPosition =
            originalPosition + direction * chargeDistance;

        NavMeshHit destinationHit;

        if (!NavMesh.SamplePosition(
            desiredPosition,
            out destinationHit,
            2f,
            agent.areaMask))
        {
            isCharging = false;
            yield break;
        }

        Vector3 chargeDestination = destinationHit.position;

        // Prevent charging beyond the NavMesh boundary.
        NavMeshHit obstacleHit;

        if (NavMesh.Raycast(
            originalPosition,
            chargeDestination,
            out obstacleHit,
            agent.areaMask))
        {
            chargeDestination = obstacleHit.position;
        }

        if (Vector3.Distance(
            originalPosition,
            chargeDestination) < 0.5f)
        {
            isCharging = false;
            yield break;
        }

        bool previousUpdatePosition = agent.updatePosition;
        bool previousStopped = agent.isStopped;

        agent.isStopped = true;
        agent.ResetPath();
        agent.updatePosition = false;

        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Run");
            animator.SetTrigger("Charge");
        }

        // Rush forward.
        yield return MoveCharge(
            chargeDestination,
            chargeSpeed
        );

        // Deal damage and knockback at the charge destination.
        DealChargeDamage();

        // Return to the original position.
        yield return MoveCharge(
            originalPosition,
            returnSpeed
        );

        if (agent != null && agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.Warp(originalPosition);
            agent.nextPosition = originalPosition;

            agent.updatePosition = previousUpdatePosition;
            agent.isStopped = previousStopped;
        }

        isCharging = false;

        UpdateMovementAnimation();

        Debug.Log("Shielder completed its charge!");
    }

    private IEnumerator MoveCharge(
        Vector3 destination,
        float speed)
    {
        while (Vector3.Distance(
            transform.position,
            destination) > 0.05f)
        {
            if (ownHealth != null && ownHealth.IsDead)
                yield break;

            if (agent == null || !agent.enabled)
                yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                speed * Time.deltaTime
            );

            agent.nextPosition = transform.position;

            yield return null;
        }

        transform.position = destination;
        agent.nextPosition = destination;
    }

    // =====================================
    // CHARGE DAMAGE
    // =====================================

    private void DealChargeDamage()
    {
        Collider[] nearbyEnemies =
            Physics.OverlapSphere(
                transform.position,
                impactRadius
            );

        HashSet<HealthSystem> damagedEnemies =
            new HashSet<HealthSystem>();

        foreach (Collider nearbyObject in nearbyEnemies)
        {
            if (!HasEnemyTag(nearbyObject.transform))
                continue;

            HealthSystem enemyHealth =
                nearbyObject.GetComponentInParent<HealthSystem>();

            if (enemyHealth == null ||
                enemyHealth.IsDead ||
                !damagedEnemies.Add(enemyHealth))
                continue;

            enemyHealth.TakeDamage(chargeDamage, transform);

            Rigidbody enemyBody =
                enemyHealth.GetComponent<Rigidbody>();

            if (enemyBody != null && !enemyBody.isKinematic)
            {
                Vector3 pushDirection =
                    enemyHealth.transform.position - transform.position;

                pushDirection.y = 0f;

                if (pushDirection.sqrMagnitude > 0.001f)
                {
                    enemyBody.AddForce(
                        pushDirection.normalized * knockbackForce,
                        ForceMode.Impulse
                    );
                }
            }

            Debug.Log(
                "Shielder charged into " + enemyHealth.name
            );
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

        HashSet<HealthSystem> checkedEnemies =
            new HashSet<HealthSystem>();

        foreach (Collider nearbyObject in nearbyObjects)
        {
            if (!HasEnemyTag(nearbyObject.transform))
                continue;

            HealthSystem health =
                nearbyObject.GetComponentInParent<HealthSystem>();

            if (health == null ||
                health.IsDead ||
                !checkedEnemies.Add(health))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                health.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = health.transform;
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
    // 30% AGGRO SYSTEM FOR ENEMIES
    // =====================================

    public static Transform GetAggroTarget(
        Transform enemy,
        float enemyDetectionRange,
        ref Transform lockedTarget,
        ref bool hasRolled)
    {
        // Keep an existing Shielder target while it is valid.
        if (lockedTarget != null)
        {
            ShielderAI lockedShielder =
                lockedTarget.GetComponent<ShielderAI>();

            if (lockedShielder != null &&
                lockedShielder.IsValidAggroTarget() &&
                Vector3.Distance(
                    enemy.position,
                    lockedTarget.position
                ) <= enemyDetectionRange)
            {
                return lockedTarget;
            }

            lockedTarget = null;
            hasRolled = false;
        }

        ShielderAI[] shielders =
            FindObjectsByType<ShielderAI>();

        ShielderAI nearestShielder = null;
        float nearestDistance = Mathf.Infinity;

        foreach (ShielderAI shielder in shielders)
        {
            if (shielder == null ||
                !shielder.IsValidAggroTarget())
                continue;

            float distance = Vector3.Distance(
                enemy.position,
                shielder.transform.position
            );

            if (distance > enemyDetectionRange)
                continue;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestShielder = shielder;
            }
        }

        // No hired Shielder nearby.
        if (nearestShielder == null)
            return null;

        // Only roll once for this target-selection period.
        if (hasRolled)
            return null;

        hasRolled = true;

        if (Random.value <= nearestShielder.aggroChance)
        {
            lockedTarget = nearestShielder.transform;

            Debug.Log(
                enemy.name + " is targeting the Shielder!"
            );

            return lockedTarget;
        }

        return null;
    }

    private bool IsValidAggroTarget()
    {
        if (hiring == null || !hiring.IsHired)
            return false;

        if (ownHealth == null || ownHealth.IsDead ||
            ownHealth.CurrentHealth <= 0f)
            return false;

        return true;
    }

    // =====================================
    // MOVEMENT ANIMATION
    // =====================================

    private void UpdateMovementAnimation()
    {
        if (animator == null || isCharging)
            return;

        bool moving = false;

        if (agent != null && agent.enabled &&
            agent.isOnNavMesh)
        {
            moving = agent.velocity.sqrMagnitude > 0.1f;
        }

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
            impactRadius
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            transform.position,
            transform.forward * chargeDistance
        );
    }
}

