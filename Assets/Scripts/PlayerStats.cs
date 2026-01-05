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
        if (controller != null)
        {
            // 1. Check I-FRAMES (Dodge Roll)
            if (controller.isInvulnerable)
            {
                Debug.Log("<color=cyan><b>[DODGED]</b> Rolled through damage!</color>");
                return; // Take NO damage
            }

            // 2. Check PARRY
            if (controller.isParrying)
            {
                Debug.Log("<color=cyan><b>[PARRY SUCCESS]</b> 0 Damage taken!</color>");
                return; 
            }
            
            // 3. Check BLOCK
            if (controller.isBlocking)
            {
                amount *= 0.5f; 
                UseStamina(10f); 
                Debug.Log($"<color=blue><b>[BLOCKED]</b> Damage mitigated.</color>");
            }
        }

        float damageAfterDefense = Mathf.Max(amount - defense, 1f); 
        currentHealth -= damageAfterDefense;

        Debug.Log($"<color=orange><b>[DAMAGE]</b> HP: {currentHealth}/{maxHealth}</color>");

        // Trigger Hit Reaction
        if (controller != null && currentHealth > 0)
        {
            controller.PlayHitReaction(damageAfterDefense);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GainXP(float amount) { currentXP += amount; if(currentXP >= maxXP) { currentXP -= maxXP; maxXP *= 1.2f; skillPoints++; } }
    public void GainMoney(int amount) { currentMoney += amount; }
    public void IncreaseMaxHealth(float amount) { maxHealth += amount; currentHealth += amount; }
    public void IncreaseMaxStamina(float amount) { maxStamina += amount; currentStamina += amount; }
    public void IncreaseDefense(float amount) { defense += amount; }

    private void Die()
    {
        Debug.Log("<color=red><b>[DEATH]</b> Player has died.</color>");
        if (controller != null)
        {
            controller.OnDeath();
        }
    }
}