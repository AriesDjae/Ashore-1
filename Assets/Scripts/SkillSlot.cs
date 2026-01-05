using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use UnityEngine.UI if not using TextMeshPro

public class SkillSlot : MonoBehaviour
{
    [Header("Config")]
    public string skillId; // MUST match the ID in SkillManager (e.g., "heavy_strike")
    
    [Header("UI Refs")]
    public Button unlockButton;
    public Image iconImage;
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.yellow;

    private SkillManager manager;

    void Start()
    {
        manager = FindObjectOfType<SkillManager>();
        
        if(unlockButton)
            unlockButton.onClick.AddListener(OnUnlockClick);

        UpdateVisuals();
    }

    void OnUnlockClick()
    {
        if (manager != null)
        {
            manager.UnlockSkill(skillId);
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        if (manager == null) return;

        SkillManager.Skill skill = manager.skills.Find(s => s.id == skillId);
        if (skill == null) return;

        if (skill.isUnlocked)
        {
            iconImage.color = unlockedColor;
            unlockButton.interactable = false; // Disable button if already owned
        }
        else
        {
            iconImage.color = lockedColor;
            // Only interactable if we have points? (Logic can be added here)
            unlockButton.interactable = true; 
        }
    }
}