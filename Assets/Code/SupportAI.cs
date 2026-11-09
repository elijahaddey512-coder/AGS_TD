

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SupportAI : MonoBehaviour
{
    [Header("Support")]
    [SerializeField] private UniversalHiring hiring;
    [SerializeField] private Animator animator;

    [Header("Normal Damage Buff")]
    [SerializeField] private float damageMultiplier = 1.3f;
    [SerializeField] private float buffDuration = 10f;
    [SerializeField] private float normalBuffInterval = 10f;

    [Header("Automatic Healing")]
    [SerializeField] private float supportRange = 10f;
    [SerializeField] private float healThreshold = 60f;
    [SerializeField] private float healAmount = 40f;
    [SerializeField] private float healCheckInterval = 1f;

    [Header("Special Skill")]
    [SerializeField] private KeyCode skillKey = KeyCode.Alpha3;
    [SerializeField] private float skillRadius = 5f;
    [SerializeField] private float skillHealAmount = 40f;
    [SerializeField] private float skillCooldown = 15f;

    [Header("Animation")]
    [SerializeField] private float healAnimationDuration = 0.8f;

    private NavMeshAgent agent;
    private HealthSystem ownHealth;

    private float nextNormalBuffTime;
    private float nextHealCheckTime;
    private float nextSkillTime;
    private float healAnimationEndTime;

    private bool isMoving;

    public float CooldownRemaining =>
        Mathf.Max(0f, nextSkillTime - Time.time);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ownHealth = GetComponent<HealthSystem>();

        if (hiring == null)
            hiring = GetComponent<UniversalHiring>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (animator != null)
            animator.SetTrigger("Idle");

        nextNormalBuffTime = Time.time;
        nextHealCheckTime = Time.time;
    }

    private void Update()
    {
        if (ownHealth != null && ownHealth.IsDead)
            return;

        if (hiring == null || !hiring.IsHired)
            return;

        if (Input.GetKeyDown(skillKey))
        {
            ActivateSkill();
        }

        if (Time.time >= nextNormalBuffTime)
        {
            BuffRandomNPC();
            nextNormalBuffTime =
                Time.time + normalBuffInterval;
        }

        if (Time.time >= nextHealCheckTime)
        {
            AutomaticallyHealAllies();
            nextHealCheckTime =
                Time.time + healCheckInterval;
        }

        UpdateMovementAnimation();
    }

    // =====================================
    // NORMAL DAMAGE BUFF
    // =====================================

    private void BuffRandomNPC()
    {
        UniversalHiring[] allNPCs =
            FindObjectsByType<UniversalHiring>();

        List<UniversalHiring> eligibleNPCs =
            new List<UniversalHiring>();

        foreach (UniversalHiring npc in allNPCs)
        {
            if (npc == null || npc == hiring)
                continue;

            if (!npc.IsHired)
                continue;

            // Shielder cannot receive damage buffs.
            if (npc.GetComponent<ShielderAI>() != null)
                continue;

            if (IsEnemy(npc.transform))
                continue;

            HealthSystem health =
                npc.GetComponent<HealthSystem>();

            if (health == null || health.IsDead ||
                health.CurrentHealth <= 0f)
                continue;

            if (GetBaseDamage(npc.gameObject) < 1f)
                continue;

            eligibleNPCs.Add(npc);
        }

        if (eligibleNPCs.Count == 0)
            return;

        UniversalHiring selectedNPC =
            eligibleNPCs[
                Random.Range(0, eligibleNPCs.Count)
            ];

        ApplyBuff(selectedNPC.gameObject);

        Debug.Log("Support buffed " + selectedNPC.name);

        PlayHealAnimation();
    }

    private float GetBaseDamage(GameObject target)
    {
        float result = 0f;

        BrawlerAI brawler =
            target.GetComponent<BrawlerAI>();

        if (brawler != null)
            result = Mathf.Max(result, brawler.BaseDamage);

        ShooterAI shooter =
            target.GetComponent<ShooterAI>();

        if (shooter != null)
            result = Mathf.Max(result, shooter.BaseDamage);

        return result;
    }

    private void ApplyBuff(GameObject target)
    {
        if (target == null || IsEnemy(target.transform))
            return;

        // Shielder is completely excluded from damage buffs.
        if (target.GetComponent<ShielderAI>() != null)
            return;

        HealthSystem health =
            target.GetComponent<HealthSystem>();

        if (health == null || health.IsDead)
            return;

        DamageBuffReceiver receiver =
            target.GetComponent<DamageBuffReceiver>();

        if (receiver == null)
            receiver = target.AddComponent<DamageBuffReceiver>();

        receiver.ApplyDamageBuff(
            damageMultiplier,
            buffDuration
        );
    }

    // =====================================
    // AUTOMATIC HEALING
    // =====================================

    private void AutomaticallyHealAllies()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                supportRange
            );

        HashSet<HealthSystem> healedCharacters =
            new HashSet<HealthSystem>();

        foreach (Collider nearbyObject in nearbyObjects)
        {
            Transform target = nearbyObject.transform;

            if (IsEnemy(target))
                continue;

            UniversalHiring ally =
                target.GetComponentInParent<UniversalHiring>();

            if (ally == null || !ally.IsHired)
                continue;

            if (ally == hiring)
                continue;

            HealthSystem health =
                ally.GetComponent<HealthSystem>();

            if (health == null || health.IsDead)
                continue;

            if (health.CurrentHealth >= healThreshold)
                continue;

            if (!healedCharacters.Add(health))
                continue;

            health.Heal(healAmount);

            Debug.Log(
                "Support automatically healed " + ally.name
            );

            PlayHealAnimation();
        }
    }

    // =====================================
    // SPECIAL SKILL: KEY 3
    // =====================================

    public void ActivateSkill()
    {
        if (ownHealth != null && ownHealth.IsDead)
            return;

        if (hiring == null || !hiring.IsHired)
            return;

        if (Time.time < nextSkillTime)
        {
            Debug.Log("Support skill is still on cooldown!");
            return;
        }

        nextSkillTime = Time.time + skillCooldown;

        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                skillRadius
            );

        HashSet<HealthSystem> affectedCharacters =
            new HashSet<HealthSystem>();

        foreach (Collider nearbyObject in nearbyObjects)
        {
            Transform target = nearbyObject.transform;

            if (IsEnemy(target))
                continue;

            HealthSystem health = null;

            Transform playerRoot =
                FindTaggedParent(target, "Player");

            if (playerRoot != null)
            {
                health = playerRoot.GetComponent<HealthSystem>();
            }
            else
            {
                UniversalHiring ally =
                    target.GetComponentInParent<UniversalHiring>();

                if (ally == null || !ally.IsHired)
                    continue;

                if (ally == hiring)
                    continue;

                health = ally.GetComponent<HealthSystem>();
            }

            if (health == null || health.IsDead ||
                health.CurrentHealth <= 0f)
                continue;

            if (!affectedCharacters.Add(health))
                continue;

            // Shielder receives healing.
            health.Heal(skillHealAmount);

            // ApplyBuff automatically excludes Shielder.
            ApplyBuff(health.gameObject);

            Debug.Log(
                "Support skill affected " + health.name
            );
        }

        PlayHealAnimation();

        Debug.Log("Support special skill activated!");
    }

    // =====================================
    // TAG CHECKING
    // =====================================

    private bool IsEnemy(Transform target)
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

    private Transform FindTaggedParent(
        Transform target,
        string tagName)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(tagName))
                return current;

            current = current.parent;
        }

        return null;
    }

    // =====================================
    // ANIMATIONS
    // =====================================

    private void PlayHealAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Heal");

        animator.SetTrigger("Heal");

        healAnimationEndTime =
            Time.time + healAnimationDuration;
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null)
            return;

        if (Time.time < healAnimationEndTime)
            return;

        bool moving = false;

        if (agent != null && agent.enabled &&
            agent.isOnNavMesh)
        {
            moving = agent.velocity.sqrMagnitude > 0.1f;
        }

        if (moving == isMoving)
            return;

        isMoving = moving;

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
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            supportRange
        );

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            skillRadius
        );
    }
}


