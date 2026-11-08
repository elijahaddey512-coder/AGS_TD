
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
    private BrawlerSkill brawlerSkill;

    private void Awake()
    {
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        brawlerAI = GetComponent<BrawlerAI>();
        brawlerSkill = GetComponent<BrawlerSkill>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

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

    private void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Brawler has died!");

        // Stop the Brawler's AI and skill scripts.
        if (brawlerAI != null)
        {
            brawlerAI.enabled = false;
        }

        if (brawlerSkill != null)
        {
            brawlerSkill.enabled = false;
        }

        // Play the death animation.
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Stop movement.
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.enabled = false;
        }

        // Remove the Brawler after the death animation.
        Destroy(gameObject, deathAnimationDuration);
    }
}

