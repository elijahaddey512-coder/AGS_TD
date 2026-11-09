using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Items")]
public class fourBars : ScriptableObject
{
    [Header("Properties")]
    public float cooldown;
    public itemType item_Type;
    public Sprite item_sprite;
}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
public enum itemType { Brawler, Shooter, Support, Sheilder};
