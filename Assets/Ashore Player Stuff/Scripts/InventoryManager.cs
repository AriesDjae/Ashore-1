using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponSocket; // The "WeaponSocket" empty object in your hand
    
    [Header("Starting Gear")]
    public ItemData startingWeapon;

    [Header("Current State")]
    public ItemData equippedWeapon;
    private GameObject currentWeaponModel;

    // A simple list for now, can be expanded to a UI grid later
    public List<ItemData> inventory = new List<ItemData>();

    void Start()
    {
        if (startingWeapon != null)
        {
            AddItem(startingWeapon);
            EquipWeapon(startingWeapon);
        }
    }

    public void AddItem(ItemData item)
    {
        if (!inventory.Contains(item))
        {
            inventory.Add(item);
        }
    }

    public void EquipWeapon(ItemData newItem)
    {
        if (newItem == null) return;
        if (!newItem.isWeapon) return;

        equippedWeapon = newItem;

        // 1. Destroy old model if it exists
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        // 2. Spawn new model
        if (newItem.modelPrefab != null && weaponSocket != null)
        {
            currentWeaponModel = Instantiate(newItem.modelPrefab, weaponSocket);
            
            // Ensure local position/rotation is zeroed out relative to the hand
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }

        Debug.Log($"Equipped: {newItem.itemName} (Damage: {newItem.damage})");
    }

    public float GetCurrentDamage()
    {
        if (equippedWeapon != null) return equippedWeapon.damage;
        return 5f; // Base fist damage
    }

    public float GetCurrentStaminaCost()
    {
        if (equippedWeapon != null) return equippedWeapon.staminaCost;
        return 10f; // Base fist cost
    }
}