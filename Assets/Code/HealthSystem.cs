
using UnityEngine;
using UnityEngine.AI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationDuration = 2f;

    private float currentHealth;
    private bool isDead = false;

    public Transform LastAttacker { get; private set; }
    public float LastAttackedTime { get; private set; }

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

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
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(gameObject.name + " took " + damage +
                  " damage. HP: " + currentHealth);

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




