


using UnityEngine;
using UnityEngine.AI;

public class UniversalHiring : MonoBehaviour
{
    [Header("Hiring Settings")]
    [SerializeField] private float hiringDistance = 3f;


    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger = "Idle";

    // This NavMeshAgent belongs to the NPC, not the Player.
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

        // Only warn if this NPC has an agent but is not placed on the NavMesh.
        if (agent != null && agent.enabled && !agent.isOnNavMesh)
        {
            Debug.LogWarning(
                gameObject.name + " has a NavMeshAgent but is not currently on the NavMesh!"
            );
        }
    }

    private void Update()
    {
        if (IsHired && agent != null && agent.hasPath)
        {
            Debug.Log(
                gameObject.name +
                " | Distance: " + agent.remainingDistance +
                " | Velocity: " + agent.velocity
            );
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            agent.isStopped = false;

            bool result = agent.SetDestination(
                transform.position + transform.forward * 5f
            );

            Debug.Log("Test movement destination accepted: " + result);
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
                    gameObject.name +
                    " was hired, but its NavMeshAgent is not on the NavMesh."
                );
            }
        }

        Debug.Log("<color=green>" + gameObject.name + " has been hired!</color>");

        return true;
    }

    public void SummonToPlayer(Vector3 playerPosition)
    {
        Debug.Log("Summon called!");

        if (!IsHired || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        Vector3 targetPosition = playerPosition + Vector3.forward * 5f;

        bool result = agent.SetDestination(targetPosition);

        Debug.Log("Destination accepted: " + result);
        Debug.Log("Target position: " + targetPosition);
        Debug.Log("Agent destination: " + agent.destination);
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





