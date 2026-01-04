using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Defense (Survivability)")]
    public float defense = 0f; 

    [Header("Resources")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 15f;
    public float staminaRegenDelay = 1f; 

    [Header("Progression")]
    public float currentXP = 0;
    public float maxXP = 1000;
    public int currentMoney = 0;
    public int skillPoints = 0;

    private float lastStaminaUseTime;
    private PlayerController controller; 

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleStaminaRegen();
    }

    private void HandleStaminaRegen()
    {
        if (Time.time - lastStaminaUseTime > staminaRegenDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }
        }
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        lastStaminaUseTime = Time.time;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"<color=orange><b>[INCOMING]</b> Received {amount} Damage...</color>");

        if (controller != null)
        {
            // 1. Check PARRY
            if (controller.isParrying)
            {
                Debug.Log("<color=cyan><b>[PARRY SUCCESS]</b> 0 Damage taken! (Perfect Timing)</color>");
                // Add Sound Effect here
                return; 
            }
            
            // 2. Check BLOCK
            if (controller.isBlocking)
            {
                float original = amount;
                amount *= 0.5f; // Reduce damage by 50%
                UseStamina(10f); // Stamina penalty
                Debug.Log($"<color=blue><b>[BLOCKED]</b> Damage mitigated: {original} -> {amount}</color>");
            }
        }

        // 3. Apply DEFENSE Stat
        float damageAfterDefense = Mathf.Max(amount - defense, 1f); 
        
        currentHealth -= damageAfterDefense;
        Debug.Log($"<color=orange><b>[DAMAGE]</b> Final HP: {currentHealth}/{maxHealth} (-{damageAfterDefense})</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GainXP(float amount)
    {
        currentXP += amount;
        Debug.Log($"<color=yellow><b>[XP]</b> Gained {amount} XP</color>");
        if(currentXP >= maxXP)
        {
            currentXP -= maxXP;
            maxXP *= 1.2f; 
            skillPoints++;
            Debug.Log("<color=yellow><b>[LEVEL UP]</b> Skill Points Available: " + skillPoints + "</color>");
        }
    }

    public void GainMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"<color=yellow><b>[MONEY]</b> +${amount}</color>");
    }

    // Skill Tree Upgrades
    public void IncreaseMaxHealth(float amount) { maxHealth += amount; currentHealth += amount; }
    public void IncreaseMaxStamina(float amount) { maxStamina += amount; currentStamina += amount; }
    public void IncreaseDefense(float amount) { defense += amount; }

    private void Die()
    {
        Debug.Log("<color=red><b>[DEATH]</b> Player has died.</color>");
    }
}