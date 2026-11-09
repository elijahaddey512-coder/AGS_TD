
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BrawlerAI : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private UniversalHiring hiring;

    [Header("Combat")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackAnimationDuration = 0.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float followDistance = 4f;


    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private string runTrigger = "Run";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string skillTrigger = "Skill";

    [Header("Special Skill")]
    [SerializeField] private KeyCode skillKey = KeyCode.Alpha1;
    [SerializeField] private float skillCooldown = 30f;
    [SerializeField] private float skillDamage = 50f;
    [SerializeField] private float impactRadius = 3f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float smashAnimationDuration = 0.8f;
    [SerializeField] private Transform model;

    private NavMeshAgent agent;
    private Transform currentTarget;

    private float nextAttackTime;
    private float attackEndTime;
    private float nextSkillTime;

    private bool isMoving;
    private bool isUsingSkill;

    private bool hasCommandDestination;
    private Vector3 commandDestination;
    private Transform followTarget;


    public float CooldownRemaining =>
        Mathf.Max(0f, nextSkillTime - Time.time);

    // Used by SupportAI to check base damage.
    public float BaseDamage => damage;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (hiring == null)
            hiring = GetComponent<UniversalHiring>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (model == null && animator != null)
            model = animator.transform;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.speed = moveSpeed;
            agent.isStopped = true;
        }
    }

    public void SetFollowTarget(Transform target)
    {
        if (hiring == null || !hiring.IsHired)
            return;

        followTarget = target;

        Debug.Log("Brawler is now following the player!");
    }

    public void StopFollowing()
    {
        followTarget = null;

        agent.isStopped = true;
        agent.ResetPath();

        PlayMovement(false);

        Debug.Log("Brawler stopped following!");
    }

    private void Update()
    {
        if (hiring == null || !hiring.IsHired)
            return;

        if (isUsingSkill)
            return;

        if (Input.GetKeyDown(skillKey) && Time.time >= nextSkillTime)
        {
            StartCoroutine(JumpAndSmash());
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (hasCommandDestination)
        {
            FollowCommandDestination();
            return;
        }

        if (Time.time < attackEndTime)
            return;

        if (IsPlayingPriorityAnimation())
        {
            agent.isStopped = true;
            return;
        }

        currentTarget = FindNearestEnemy();

        if (currentTarget == null)
        {
            FollowPlayer();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTarget.position
        );

        if (distance > attackRange)
            MoveTowardsEnemy();
        else
            AttackEnemy();
    }

    // =====================================
    // FIND NEAREST ENEMY
    // =====================================

    private Transform FindNearestEnemy()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(transform.position, detectionRange);

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
    // MOVEMENT
    // =====================================

    private void MoveTowardsEnemy()
    {

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);

        PlayMovement(true);

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        RotateTowards(direction);
    }

    private void FollowPlayer()
    {
        if (followTarget == null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            PlayMovement(false, true);
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            followTarget.position
        );

        if (distance > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(followTarget.position);

            PlayMovement(true);

            Vector3 direction = agent.desiredVelocity;
            direction.y = 0f;

            RotateTowards(direction);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();

            PlayMovement(false);
        }
    }

    // =====================================
    // NORMAL ATTACK
    // =====================================

    private void AttackEnemy()
    {
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;
        RotateTowards(direction);

        if (Time.time < nextAttackTime)
            return;

        HealthSystem enemyHealth =
            currentTarget.GetComponentInParent<HealthSystem>();

        if (enemyHealth == null || enemyHealth.IsDead)
            return;

        // Check whether the Brawler has a damage buff.
        DamageBuffReceiver buff =
            GetComponent<DamageBuffReceiver>();

        float multiplier = buff != null
            ? buff.DamageMultiplier
            : 1f;

        float finalDamage = damage * multiplier;

        // Identify the Brawler as the attacker.
        enemyHealth.TakeDamage(finalDamage, transform);

        if (animator != null)
        {
            animator.ResetTrigger(idleTrigger);
            animator.ResetTrigger(runTrigger);
            animator.ResetTrigger(attackTrigger);

            animator.SetTrigger(attackTrigger);
        }

        nextAttackTime = Time.time + attackCooldown;
        attackEndTime = Time.time + attackAnimationDuration;

        isMoving = false;
    }

    // =====================================
    // SPECIAL SKILL: JUMP AND SMASH
    // =====================================

    private IEnumerator JumpAndSmash()
    {
        if (model == null)
        {
            Debug.LogWarning("Brawler: Model reference is missing!");
            yield break;
        }

        if (animator == null)
        {
            Debug.LogWarning("Brawler: Animator reference is missing!");
            yield break;
        }

        isUsingSkill = true;
        nextSkillTime = Time.time + skillCooldown;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        isMoving = false;

        TriggerSkillAnimation();

        Debug.Log("Brawler Smash skill started!");

        Vector3 startingPosition = model.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / jumpDuration);

            float height =
                Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            model.localPosition =
                startingPosition + Vector3.up * height;

            yield return null;
        }

        model.localPosition = startingPosition;

        DealImpactDamage();

        Debug.Log("Brawler landed and dealt Smash damage!");

        float remainingTime =
            Mathf.Max(0f, smashAnimationDuration - jumpDuration);

        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        isUsingSkill = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;

        Debug.Log("Brawler Smash skill finished!");
    }

    // =====================================
    // SKILL DAMAGE
    // =====================================

    private void DealImpactDamage()
    {
        Collider[] hitObjects =
            Physics.OverlapSphere(transform.position, impactRadius);

        HashSet<HealthSystem> damagedEnemies =
            new HashSet<HealthSystem>();

        // Check the buff once for this skill.
        DamageBuffReceiver buff =
            GetComponent<DamageBuffReceiver>();

        float multiplier = buff != null
            ? buff.DamageMultiplier
            : 1f;

        float finalDamage = skillDamage * multiplier;

        foreach (Collider hitObject in hitObjects)
        {
            if (!HasEnemyTag(hitObject.transform))
                continue;

            HealthSystem health =
                hitObject.GetComponentInParent<HealthSystem>();

            if (health == null || health.IsDead ||
                health.CurrentHealth <= 0)
                continue;

            if (damagedEnemies.Add(health))
            {
                // Identify the Brawler as the skill attacker.
                health.TakeDamage(finalDamage, transform);
            }
        }
    }

    // =====================================
    // ANIMATIONS
    // =====================================

    private void PlayMovement(bool moving, bool force = false)
    {
        if (isUsingSkill)
            return;

        if (!force && isMoving == moving)
            return;

        isMoving = moving;

        if (animator == null)
            return;

        if (moving)
        {
            animator.ResetTrigger(idleTrigger);
            animator.SetTrigger(runTrigger);
        }
        else
        {
            animator.ResetTrigger(runTrigger);
            animator.SetTrigger(idleTrigger);
        }
    }

    public void TriggerSkillAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("Brawler: Animator is missing!");
            return;
        }

        animator.ResetTrigger(idleTrigger);
        animator.ResetTrigger(runTrigger);
        animator.ResetTrigger(attackTrigger);

        animator.ResetTrigger(skillTrigger);
        animator.SetTrigger(skillTrigger);

        isMoving = false;

        Debug.Log("Brawler Smash Trigger activated!");
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
        return state.shortNameHash == Animator.StringToHash("Hurt")
            || state.shortNameHash == Animator.StringToHash("Die");
    }

    // =====================================
    // COMMAND MOVEMENT
    // =====================================

    public void SetCommandDestination(Vector3 destination)
    {
        if (hiring == null || !hiring.IsHired)
            return;

        if (isUsingSkill)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        hasCommandDestination = true;
        commandDestination = destination;

        agent.isStopped = false;
        agent.SetDestination(commandDestination);

        PlayMovement(true);

        Debug.Log("Brawler received movement command!");
    }

    private void FollowCommandDestination()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        if (agent.hasPath &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            hasCommandDestination = false;

            agent.isStopped = true;
            agent.ResetPath();

            PlayMovement(false, true);

            Debug.Log("Brawler reached its destination!");
            return;
        }

        agent.isStopped = false;

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        RotateTowards(direction);
        PlayMovement(true);
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
    // GIZMOS
    // =====================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}









