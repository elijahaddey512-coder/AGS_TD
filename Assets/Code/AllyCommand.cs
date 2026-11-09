using UnityEngine;
using UnityEngine.AI;

public class AllyCommand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHiring playerHiring;

    private void Awake()
    {
        if (playerHiring == null)
            playerHiring = GetComponent<PlayerHiring>();

        if (playerHiring == null)
            Debug.LogError("PlayerHiring is Missing!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SummonAllies();
        }
    }

    private void SummonAllies()
    {
        if (playerHiring == null)
            return;
        
        NavMeshHit hit;

        bool playerOnNavMesh = NavMesh.SamplePosition(
            transform.position,
            out hit,
            2f,
            NavMesh.AllAreas
        );

        Debug.Log("Player on NavMesh: " + playerOnNavMesh);

        if (playerOnNavMesh)
        {
            Debug.Log("Closest NavMesh position: " + hit.position);
        }
        foreach (UniversalHiring ally in playerHiring.HiredAllies)
        {
            switch (ally)
            {
                case var a when a.GetComponent<ShooterAI>() != null:
                    a.GetComponent<ShooterAI>().SetFollowTarget(transform);
                    break;

                case var a when a.GetComponent<BrawlerAI>() != null:
                    a.GetComponent<BrawlerAI>().SetFollowTarget(transform);
                    break;

                default:
                    ally.SummonToPlayer(transform.position);
                    break;
            }
        }

        Debug.Log("Summon command issued!");
    }
}
