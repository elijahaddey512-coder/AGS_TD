using UnityEngine;
using UnityEngine.AI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startingHealth = 100f;

    [Header("Death")]
    [SerializeField] private float deathAnimationDuration = 2f;
    [SerializeField] private Animator animator;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public Transform LastAttacker { get; private set; }
    public float LastAttackedTime { get; private set; }

    private bool isDead;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CurrentHealth = Mathf.Clamp(
            startingHealth,
            0f,
            maxHealth
        );
    }

    // =====================================
    // TAKE DAMAGE
    // =====================================

    public void TakeDamage(float damage)
    {
        ApplyDamage(damage);
    }

    public void TakeDamage(float damage, Transform attacker)
    {
        if (attacker != null)
        {
            LastAttacker = attacker;
            LastAttackedTime = Time.time;
        }

        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        Debug.Log(
            gameObject.name + " took " + damage +
            " damage. HP: " + CurrentHealth
        );

        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    // =====================================
    // HEAL
    // =====================================

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Min(
            CurrentHealth + amount,
            maxHealth
        );

        Debug.Log(
            gameObject.name + " healed for " +
            (CurrentHealth - previousHealth) +
            ". HP: " + CurrentHealth
        );
    }

    // =====================================
    // DEATH
    // =====================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " has died!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Destroy(gameObject, deathAnimationDuration);
    }
}




