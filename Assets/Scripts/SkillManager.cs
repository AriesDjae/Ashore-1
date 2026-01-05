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
        public int cost; 
    }

    public List<Skill> skills = new List<Skill>();

    // Hotbar logic removed for MVP clarity, can be re-added easily
    
    void Start()
    {
        if (!playerController) playerController = GetComponent<PlayerController>();
        if (!playerStats) playerStats = GetComponent<PlayerStats>();

        InitializeSkills();
    }

    void InitializeSkills()
    {
        // BRANCH 1: COMBAT (Strength)
        skills.Add(new Skill { id = "combat_1", displayName = "Damage Boost I", description = "+10% Damage", isUnlocked = false, cost = 1 });
        skills.Add(new Skill { id = "heavy_strike", displayName = "Heavy Attack", description = "Unlock RMB Action", isUnlocked = false, cost = 2 }); // Requires combat_1 logically (UI layout handles this)

        // BRANCH 2: AGILITY (Movement)
        skills.Add(new Skill { id = "stamina_1", displayName = "Endurance I", description = "Stamina Costs -10%", isUnlocked = false, cost = 1 });
        skills.Add(new Skill { id = "double_jump", displayName = "Air Step", description = "Jump in mid-air", isUnlocked = false, cost = 2 });
    }

    // DEBUG: Added this so you can unlock skills without UI
    void Update()
    {
        // Press Numpad or Alpha keys to cheat/test
        if (Input.GetKeyDown(KeyCode.Alpha1)) UnlockSkill("combat_1");
        if (Input.GetKeyDown(KeyCode.Alpha2)) UnlockSkill("heavy_strike");
        if (Input.GetKeyDown(KeyCode.Alpha3)) UnlockSkill("stamina_1");
        if (Input.GetKeyDown(KeyCode.Alpha4)) UnlockSkill("double_jump");
    }

    public void UnlockSkill(string skillId)
    {
        Skill skill = skills.Find(s => s.id == skillId);
        if (skill == null) return;

        // Note: For Dev Debugging, we can bypass the cost check if we want, 
        // but for now we keep it standard. If you want free unlocks, 
        // just set skillPoints to 99 in PlayerStats inspector.
        
        if (skill.isUnlocked)
        {
            Debug.Log($"<color=yellow>Skill '{skill.displayName}' is already unlocked.</color>");
            return;
        }

        if (playerStats.skillPoints >= skill.cost)
        {
            playerStats.skillPoints -= skill.cost;
            skill.isUnlocked = true;
            Debug.Log($"<color=green>UNLOCKED: {skill.displayName}</color>");
            
            ApplySkillEffect(skill.id);
        }
        else
        {
            Debug.Log("<color=red>Not enough Skill Points!</color>");
        }
    }

    void ApplySkillEffect(string id)
    {
        switch (id)
        {
            // Combat Branch
            case "combat_1": 
                playerController.damageMultiplier += 0.1f; 
                break;
            case "heavy_strike": 
                playerController.unlockedHeavyAttack = true; 
                break;

            // Agility Branch
            case "stamina_1": 
                playerController.staminaCostMultiplier -= 0.1f; 
                break;
            case "double_jump": 
                playerController.unlockedDoubleJump = true; 
                break;
        }
    }
}