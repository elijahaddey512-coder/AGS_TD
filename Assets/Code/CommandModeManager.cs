
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class CommandModeManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Command Settings")]
    [SerializeField] private Camera commandCamera;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float navMeshSearchDistance = 2f;

    private bool commandMode = false;

    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisibility;

    private Transform selectedCharacter;
    private NavMeshAgent selectedAgent;

    public bool IsCommandMode => commandMode;

    private void Start()
    {
        if (commandCamera == null)
        {
            commandCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleCommandMode();
        }

        if (commandMode && Input.GetMouseButtonDown(0))
        {
            IssueMovementOrder();
        }
    }

    private void ToggleCommandMode()
    {
        if (commandMode)
        {
            ExitCommandMode();
        }
        else
        {
            EnterCommandMode();
        }
    }

    private void EnterCommandMode()
    {
        commandMode = true;

        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        Time.timeScale = 0f;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Command Mode Activated");
    }

    private void ExitCommandMode()
    {
        commandMode = false;

        Time.timeScale = previousTimeScale;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisibility;

        selectedCharacter = null;
        selectedAgent = null;

        Debug.Log("Command Mode Deactivated");
    }

    public void SelectCharacter(Transform character)
    {
        if (!commandMode)
        {
            Debug.Log("Press R before selecting a character.");
            return;
        }

        if (character == null)
            return;

        NavMeshAgent agent =
            character.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Selected character has no valid NavMeshAgent.");
            return;
        }

        selectedCharacter = character;
        selectedAgent = agent;

        Debug.Log("Selected character: " + character.name);
    }

    private void IssueMovementOrder()
    {
        if (selectedAgent == null || !selectedAgent.enabled)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (commandCamera == null)
            return;

        Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayers))
        {
            NavMeshHit navHit;

            if (NavMesh.SamplePosition(
                hit.point,
                out navHit,
                navMeshSearchDistance,
                selectedAgent.areaMask))
            {
                BrawlerAI brawlerAI =
                    selectedCharacter.GetComponent<BrawlerAI>();

                if (brawlerAI != null)
                {
                    brawlerAI.SetCommandDestination(navHit.position);
                }
                else
                {
                    selectedAgent.isStopped = false;
                    selectedAgent.SetDestination(navHit.position);
                }

                Debug.Log("Movement order sent to " + selectedCharacter.name);
            }
            else
            {
                Debug.Log("No valid NavMesh found at this location.");
            }
        }
    }

    private void OnDestroy()
    {
        if (commandMode)
        {
            Time.timeScale = previousTimeScale;
        }
    }
}


