
using UnityEngine;
using UnityEngine.AI;

public class BrawlerHiring : MonoBehaviour
{
    [Header("Hiring Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float hiringDistance = 3f;
    [SerializeField] private KeyCode hiringKey = KeyCode.F;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;

    public bool IsHired { get; private set; } = false;

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
        // Find the Player automatically if one is not assigned.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Keep the Brawler stationary until hired.
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

    private void Update()
    {
        if (IsHired || player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= hiringDistance &&
            Input.GetKeyDown(hiringKey))
        {
            HireBrawler();
        }
    }

    private void HireBrawler()
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hiringDistance);
    }
}


