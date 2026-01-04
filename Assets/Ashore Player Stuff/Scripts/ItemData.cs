using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName = "New Item";
    public Sprite icon; // For UI later
    public bool isWeapon;

    [Header("Weapon Stats (If Weapon)")]
    public float damage = 10f;
    public float staminaCost = 15f;
    public GameObject modelPrefab; // The 3D Sword Model (.fbx)
    
    [Header("Animation Type")]
    // 0 = Unarmed, 1 = 2H Sword, 2 = Dagger, etc.
    public int weaponAnimationID = 1; 
}