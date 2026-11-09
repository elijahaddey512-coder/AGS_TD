
using UnityEngine;
using UnityEngine.AI;

public class BrawlerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 70f;
    [SerializeField] private float deathAnimationDuration = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private NavMeshAgent agent;
    private BrawlerAI brawlerAI;

    private void Awake()
    {
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        brawlerAI = GetComponent<BrawlerAI>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // =====================================
    // TAKE DAMAGE
    // =====================================

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(
            "Brawler took " + damage +
            " damage. Remaining HP: " + currentHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            PlayHurtAnimation();
        }
    }

    // =====================================
    // HURT ANIMATION
    // =====================================

    private void PlayHurtAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Skill");

        animator.SetTrigger("Hurt");
    }

    // =====================================
    // DEATH
    // =====================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Brawler has died!");

        // Stop Brawler AI and its skill coroutine.
        if (brawlerAI != null)
            brawlerAI.enabled = false;

        // Play death animation.
        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Run");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Skill");
            animator.ResetTrigger("Hurt");

            animator.SetTrigger("Die");
        }

        // Stop NavMesh movement.
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.enabled = false;
        }

        // Destroy after death animation.
        Destroy(gameObject, deathAnimationDuration);
    }
}



