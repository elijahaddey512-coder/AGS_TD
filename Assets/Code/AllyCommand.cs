using UnityEngine;
using UnityEngine.AI;

public class AllyCommand : MonoBehaviour
{
    private bool alliesFollowing = false;

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

        alliesFollowing = !alliesFollowing;

        foreach (UniversalHiring ally in playerHiring.HiredAllies)
        {
            if (ally == null)
                continue;

            if (alliesFollowing)
            {
                switch (ally)
                {
                    case var a when a.GetComponent<ShooterAI>() != null:
                        a.GetComponent<ShooterAI>().SetFollowTarget(transform);
                        break;

                    case var a when a.GetComponent<BrawlerAI>() != null:
                        a.GetComponent<BrawlerAI>().SetFollowTarget(transform);
                        break;

                    case var a when a.GetComponent<ShielderAI>() != null:
                        a.GetComponent<ShielderAI>().SetFollowTarget(transform);
                        break;

                    default:
                        ally.SummonToPlayer(transform.position);
                        break;
                }
            }
            else
            {
                switch (ally)
                {
                    case var a when a.GetComponent<ShooterAI>() != null:
                        a.GetComponent<ShooterAI>().StopFollowing();
                        break;

                    case var a when a.GetComponent<BrawlerAI>() != null:
                        a.GetComponent<BrawlerAI>().StopFollowing();
                        break;

                    case var a when a.GetComponent<ShielderAI>() != null:
                        a.GetComponent<ShielderAI>().StopFollowing();
                        break;
                }
            }
        }
    }
}
