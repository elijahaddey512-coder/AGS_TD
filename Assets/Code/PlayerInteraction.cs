
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private BrawlerHiring currentBrawler;
    private CoreHealth currentCore;

    private void Update()
    {
        FindNearestInteractable();

        // Hire the Brawler with one press.
        if (currentBrawler != null &&
            Input.GetKeyDown(interactionKey))
        {
            currentBrawler.HireBrawler();
        }

        // Destroy the Core by holding F.
        if (currentCore != null)
        {
            if (Input.GetKey(interactionKey))
            {
                currentCore.AdvanceHold(Time.deltaTime);
            }
            else
            {
                currentCore.ResetHold();
            }
        }
    }

    private void FindNearestInteractable()
    {
        BrawlerHiring nearestBrawler = null;
        CoreHealth nearestCore = null;

        float nearestDistance = Mathf.Infinity;

        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            3.5f,
            interactionLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider nearbyObject in nearbyObjects)
        {
            BrawlerHiring brawler =
                nearbyObject.GetComponentInParent<BrawlerHiring>();

            CoreHealth core =
                nearbyObject.GetComponentInParent<CoreHealth>();

            if (brawler != null && !brawler.IsHired)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    brawler.transform.position
                );

                if (distance <= brawler.HiringDistance &&
                    distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBrawler = brawler;
                    nearestCore = null;
                }
            }

            if (core != null && !core.IsDestroyed)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    core.transform.position
                );

                if (distance <= core.InteractionDistance &&
                    distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCore = core;
                    nearestBrawler = null;
                }
            }
        }

        // Reset the old Core if the Player changes targets.
        if (currentCore != null && currentCore != nearestCore)
        {
            currentCore.ResetHold();
        }

        currentBrawler = nearestBrawler;
        currentCore = nearestCore;
    }

    private void OnDisable()
    {
        if (currentCore != null)
        {
            currentCore.ResetHold();
        }
    }
}

