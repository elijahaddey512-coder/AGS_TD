using UnityEngine;

public class charPickup : MonoBehaviour, IPickable
{
    public fourBars itemScriptableObject;

    public void PickItem()
    {
        Destroy(gameObject);
    }
}
