

using UnityEngine;

public class CoreHealth : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float holdDuration = 10f;

    private float holdTimer;
    private bool isDestroyed;

    public bool IsDestroyed => isDestroyed;
    public float InteractionDistance => interactionDistance;

    public float HoldProgress =>
        Mathf.Clamp01(holdTimer / holdDuration);

    public void AdvanceHold(float deltaTime)
    {
        if (isDestroyed)
            return;

        holdTimer += deltaTime;

        if (holdTimer >= holdDuration)
        {
            DestroyCore();
        }
    }

    public void ResetHold()
    {
        if (!isDestroyed)
        {
            holdTimer = 0f;
        }
    }

    private void DestroyCore()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log(gameObject.name + " has been destroyed!");

        Destroy(gameObject);
    }

    // Monsters cannot damage the Core.
    public void TakeDamage(float damage)
    {
        Debug.Log("Only the Player can destroy this Core.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}



