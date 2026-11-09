

using UnityEngine;
using UnityEngine.AI;

public class UniversalHiring : MonoBehaviour
{
    [Header("Hiring Settings")]
    [SerializeField] private float hiringDistance = 3f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger = "Idle";

    private NavMeshAgent agent;

    public bool IsHired { get; private set; } = false;
    public float HiringDistance => hiringDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        StopNPC();
        PlayIdleAnimation();

        Debug.Log(gameObject.name + " is waiting to be hired.");

        if (agent == null)
        {
            Debug.LogWarning(gameObject.name + " has no NavMeshAgent!");
        }
        else if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(gameObject.name + " is not currently on the NavMesh!");
        }
    }

    private void StopNPC()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void PlayIdleAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(idleTrigger))
            return;

        animator.ResetTrigger(idleTrigger);
        animator.SetTrigger(idleTrigger);
    }

    public bool Hire()
    {
        if (IsHired)
        {
            Debug.Log(gameObject.name + " has already been hired.");
            return false;
        }

        IsHired = true;

        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
            else
            {
                Debug.LogWarning(
                    gameObject.name + " was hired, but its NavMeshAgent is not on the NavMesh."
                );
            }
        }

        Debug.Log("<color=green>" + gameObject.name + " has been hired!</color>");

        return true;
    }

    public bool CanBeHired()
    {
        return !IsHired;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hiringDistance);
    }
}




