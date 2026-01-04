using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerStats playerStats;

    [System.Serializable]
    public class Skill
    {
        public string id;
        public string displayName;
        public string description;
        public bool isUnlocked;
        public bool isPassive; 
        public int cost; 
    }

    public string[] equippedSkills = new string[3]; 

    public List<Skill> skills = new List<Skill>();

    void Start()
    {
        if (!playerController) playerController = GetComponent<PlayerController>();
        if (!playerStats) playerStats = GetComponent<PlayerStats>();

        InitializeSkills();
        
        equippedSkills[0] = "heal"; 
    }

    void InitializeSkills()
    {
        // PASSIVES
        skills.Add(new Skill { id = "combat_mastery", displayName = "Warrior's Edge", description = "+20% Damage", isUnlocked = false, isPassive = true, cost = 1 });
        skills.Add(new Skill { id = "crafting_prof", displayName = "Crafting Proficiency", description = "Efficient Crafting", isUnlocked = true, isPassive = true, cost = 1 }); 
        skills.Add(new Skill { id = "stealth_plus", displayName = "Shadow Walk", description = "Harder to detect", isUnlocked = true, isPassive = true, cost = 1 }); 

        // ACTIONS / UNLOCKS
        skills.Add(new Skill { id = "heavy_strike", displayName = "Heavy Strike", description = "Unlock RMB Attack", isUnlocked = false, isPassive = true, cost = 2 });
        skills.Add(new Skill { id = "double_jump", displayName = "Air Step", description = "Jump in air", isUnlocked = false, isPassive = true, cost = 2 });

        // ACTIVE SKILLS
        skills.Add(new Skill { id = "heal", displayName = "First Aid", description = "Heal 30 HP", isUnlocked = true, isPassive = false, cost = 1 });
    }

    public void UnlockSkill(string skillId)
    {
        Skill skill = skills.Find(s => s.id == skillId);
        if (skill == null || skill.isUnlocked) return;

        if (playerStats.skillPoints >= skill.cost)
        {
            playerStats.skillPoints -= skill.cost;
            skill.isUnlocked = true;
            Debug.Log("Unlocked: " + skill.displayName);
            ApplyPassiveEffect(skill.id);
        }
        else
        {
            Debug.Log("Not enough Skill Points!");
        }
    }

    public void UseSkillSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length) return;
        
        string skillId = equippedSkills[slotIndex];
        if (string.IsNullOrEmpty(skillId)) return;

        ActivateSkill(skillId);
    }

    void ActivateSkill(string id)
    {
        switch (id)
        {
            case "heal":
                Debug.Log("Used Heal Skill!");
                playerStats.currentHealth += 30;
                if(playerStats.currentHealth > playerStats.maxHealth) playerStats.currentHealth = playerStats.maxHealth;
                break;
        }
    }

    void ApplyPassiveEffect(string id)
    {
        switch (id)
        {
            // FIX: This now targets the multiplier instead of flat damage
            case "combat_mastery": playerController.damageMultiplier *= 1.2f; break;
            
            case "heavy_strike": playerController.unlockedHeavyAttack = true; break;
            case "double_jump": playerController.unlockedDoubleJump = true; break;
            case "stealth_plus": playerController.stealthDetectionMultiplier = 0.5f; break;
        }
    }
}