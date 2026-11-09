
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    //==========================================================
    //INTERACTION SETTINGS
    //==========================================================

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;


    //==========================================================
    //REFERENCES
    //==========================================================

    [Header("References")]
    [SerializeField] private PlayerHiring playerHiring;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private TMP_Text interactionPrompt;


    //==========================================================
    //ANIMATION
    //==========================================================

    [Header("Interaction Animation")]
    [SerializeField] private string comTrigger = "Com";


    //==========================================================
    //INTERACTION VARIABLES
    //==========================================================

    [Header("Interaction Variables")]
    private UniversalHiring nearestNPC;
    private CoreHealth nearestCore;
    private CoreHealth activeCore;


    //==========================================================
    //INITIALIZATION
    //==========================================================

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


    //==========================================================
    //MAIN UPDATE LOOP
    //==========================================================

    private void Update()
    {
        FindInteractionTarget();
        UpdateInteractionPrompt();
        HandleInteraction();
    }


    //==========================================================
    //TARGET DETECTION
    //==========================================================

    private void FindInteractionTarget()
    {
        nearestNPC = null;
        nearestCore = null;

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide);

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
                float distance = Vector3.Distance(transform.position, npc.transform.position);

                float allowedDistance = Mathf.Min(interactionDistance, npc.HiringDistance);

                if (distance <= allowedDistance && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                    nearestCore = null;
                }
            }

            // Check for a Core.
            if (core != null)
            {
                float distance = Vector3.Distance(transform.position, core.transform.position);

                float allowedDistance = Mathf.Min(interactionDistance, core.InteractionDistance);

                if (distance <= allowedDistance && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCore = core;
                    nearestNPC = null;
                }
            }
        }
    }


    //==========================================================
    //UI PROMPT
    //==========================================================

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null)
            return;

        if (nearestNPC != null)
        {
            interactionPrompt.text = "[ F ] Hire " + nearestNPC.transform.root.name;
        }

        else if (nearestCore != null)
        {
            interactionPrompt.text = "[ Hold F ]  Destroy " + nearestCore.transform.root.name;
        }
        else
        {
            interactionPrompt.text = "";
        }

        interactionPrompt.gameObject.SetActive(nearestNPC != null || nearestCore != null);
    }


    //==========================================================
    //INTERACTION INPUT
    //==========================================================

    private void HandleInteraction()
    {
        // Start interaction when F is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (nearestNPC != null)
            {
                if (playerHiring.TryHire(nearestNPC))
                {
                    PlayComAnimation();
                }
            }
            else if (nearestCore != null)
            {
                activeCore = nearestCore;
                activeCore.ResetHold();

                PlayComAnimation();

                Debug.Log("Started destroying Core!");
            }
        }

        // Continue holding F to destroy Core
        if (Input.GetKey(KeyCode.F) && activeCore != null)
        {
            if (activeCore.IsDestroyed)
            {
                activeCore = null;
                return;
            }

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
            }
        }

        // Stop holding when F is released
        if (Input.GetKeyUp(KeyCode.F) && activeCore != null)
        {
            activeCore.ResetHold();
            activeCore = null;
        }
    }

    //==========================================================
    //ANIMATION
    //==========================================================

    private void PlayComAnimation()
    {
        if (playerAnimator == null || string.IsNullOrEmpty(comTrigger))
            return;

        playerAnimator.ResetTrigger(comTrigger);
        playerAnimator.SetTrigger(comTrigger);

        Debug.Log("Player Com animation triggered.");
    }


    //==========================================================
    //DEBUG VISUALIZATION
    //==========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
