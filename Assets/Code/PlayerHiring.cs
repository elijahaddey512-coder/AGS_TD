

using UnityEngine;

public class PlayerHiring : MonoBehaviour
{
    public bool TryHire(UniversalHiring target)
    {
        if (target == null)
        {
            Debug.LogWarning("Hiring failed: No NPC was detected.");
            return false;
        }

        // Prevent the Player from hiring itself.
        if (target.transform.root == transform.root)
        {
            Debug.LogWarning("Hiring failed: Detected the Player instead of an NPC.");
            return false;
        }

        if (!target.CanBeHired())
        {
            Debug.Log(target.gameObject.name + " has already been hired.");
            return false;
        }

        float distance = Vector3.Distance(
            transform.position,
            target.transform.position
        );

        if (distance > target.HiringDistance)
        {
            Debug.Log("Hiring failed: NPC is too far away.");
            return false;
        }

        bool success = target.Hire();

        if (success)
        {
            Debug.Log(
                "<color=green>PlayerHiring: Successfully hired " +
                target.gameObject.name + "</color>"
            );
        }

        return success;
    }
}




