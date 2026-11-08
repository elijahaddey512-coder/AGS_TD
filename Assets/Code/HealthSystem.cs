
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationDuration = 2f;

    private float currentHealth;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        // Automatically find Animator if one is not assigned.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void TakeDamage(float damage)
    {
        // Ignore damage if already dead.
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

        // Stop movement if this object has a NavMeshAgent.
        UnityEngine.AI.NavMeshAgent agent =
            GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Stop movement if this object has a Rigidbody.
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Destroy(gameObject, deathAnimationDuration);
    }
}


