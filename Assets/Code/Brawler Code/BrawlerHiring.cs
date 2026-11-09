
using UnityEngine;
using UnityEngine.AI;

public class BrawlerHiring : MonoBehaviour
{
    [Header("Hiring Settings")]
    [SerializeField] private float hiringDistance = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

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
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
    }

    public void HireBrawler()
    {
        if (IsHired)
            return;

        IsHired = true;

        Debug.Log("Brawler has been hired!");

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
}





