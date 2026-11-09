
using UnityEngine;

public class CoreHealth : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private float interactionDistance = 3.5f;
    [SerializeField] private float holdDuration = 10f;

    private float holdTimer = 0f;
    private bool isDestroyed = false;
    private int lastDisplayedSecond = -1;

    public float InteractionDistance => interactionDistance;
    public float Progress => holdTimer / holdDuration;
    public bool IsDestroyed => isDestroyed;

    public eheath healthbar;    


    public void AdvanceHold(float deltaTime)
    {
        if (isDestroyed)
            return;

        holdTimer += deltaTime;

        int currentSecond = Mathf.FloorToInt(holdTimer);

        if (currentSecond != lastDisplayedSecond)
        {
            lastDisplayedSecond = currentSecond;

            Debug.Log(
                gameObject.name + " destruction progress: " +
                Mathf.Min(currentSecond, Mathf.CeilToInt(holdDuration)) +
                "/" + Mathf.CeilToInt(holdDuration) + " seconds"
            );
        }

        if (holdTimer >= holdDuration)
        {
            DestroyCore();
        }
    }

    public void ResetHold()
    {
        if (isDestroyed)
            return;

        if (holdTimer > 0f)
        {
            Debug.Log(gameObject.name + " destruction cancelled. Timer reset.");
        }

        holdTimer = 0f;
        lastDisplayedSecond = -1;
    }

    private void DestroyCore()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log("<color=red>" + gameObject.name + " has been destroyed!</color>");

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}




