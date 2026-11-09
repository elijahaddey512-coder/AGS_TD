


using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("References")]
    [SerializeField] private PlayerHiring playerHiring;
    [SerializeField] private Animator playerAnimator;

    [Header("Interaction Animation")]
    [SerializeField] private string comTrigger = "Com";

    private CoreHealth activeCore;

    private void Awake()
    {
        if (playerHiring == null)
        {
            playerHiring = GetComponent<PlayerHiring>();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        if (playerHiring == null)
        {
            Debug.LogError("PlayerHiring is missing from the Player!");
        }

        if (playerAnimator == null)
        {
            Debug.LogError("Player Animator is missing!");
        }
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        // F pressed.
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindInteractionTarget();
        }

        // F held for Core destruction.
        if (Input.GetKey(KeyCode.F) && activeCore != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                activeCore.transform.position
            );

            if (distance <= activeCore.InteractionDistance)
            {
                activeCore.AdvanceHold(Time.deltaTime);
            }
            else
            {
                activeCore.ResetHold();
                activeCore = null;

                Debug.Log("Moved too far away. Core interaction cancelled.");
            }
        }

        // F released.
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (activeCore != null)
            {
                activeCore.ResetHold();
                activeCore = null;
            }
        }
    }

    private void FindInteractionTarget()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            interactionDistance,
            interactionLayers,
            QueryTriggerInteraction.Collide
        );

        UniversalHiring nearestNPC = null;
        CoreHealth nearestCore = null;

        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyObject in nearbyObjects)
        {
            // Ignore all colliders belonging to the Player.
            if (nearbyObject.transform.root == transform.root)
            {
                continue;
            }

            UniversalHiring npc =
                nearbyObject.GetComponentInParent<UniversalHiring>();

            CoreHealth core =
                nearbyObject.GetComponentInParent<CoreHealth>();

            // Check for a hireable NPC.
            if (npc != null && npc.CanBeHired())
            {
                float distance = Vector3.Distance(
                    transform.position,
                    npc.transform.position
                );

                float allowedDistance = Mathf.Min(
                    interactionDistance,
                    npc.HiringDistance
                );

                if (distance <= allowedDistance &&
                    distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                    nearestCore = null;
                }
            }

            // Check for a Core.
            if (core != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    core.transform.position
                );

                float allowedDistance = Mathf.Min(
                    interactionDistance,
                    core.InteractionDistance
                );

                if (distance <= allowedDistance &&
                    distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCore = core;
                    nearestNPC = null;
                }
            }
        }

        // NPC found: hire it.
        if (nearestNPC != null)
        {
            Debug.Log("Interaction target detected: " + nearestNPC.gameObject.name);

            if (playerHiring != null)
            {
                bool hired = playerHiring.TryHire(nearestNPC);

                if (hired)
                {
                    PlayComAnimation();
                }
            }

            return;
        }

        // Core found: begin holding F.
        if (nearestCore != null)
        {
            activeCore = nearestCore;

            activeCore.ResetHold();

            PlayComAnimation();

            Debug.Log(
                "Started destroying " + activeCore.gameObject.name +
                ". Hold F for 10 seconds!"
            );

            return;
        }

        Debug.Log("No NPC or Core nearby.");
    }

    private void PlayComAnimation()
    {
        if (playerAnimator == null || string.IsNullOrEmpty(comTrigger))
            return;

        playerAnimator.ResetTrigger(comTrigger);
        playerAnimator.SetTrigger(comTrigger);

        Debug.Log("Player Com animation triggered.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}


