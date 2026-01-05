using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int xpReward = 50; // XP given to player on death

    [Header("Visual Feedback")]
    public Renderer meshRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    private EnemyAI ai; // Reference to stop moving when dead

    void Start()
    {
        currentHealth = maxHealth;
        ai = GetComponent<EnemyAI>();
        
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // Visual Flash
        if (meshRenderer != null) 
        {
            StopAllCoroutines();
            StartCoroutine(FlashColor());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Give XP to player
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if(player != null) player.GainXP(xpReward);

        // Disable AI
        if(ai != null) ai.enabled = false;
        GetComponent<Collider>().enabled = false; // Stop blocking path

        Debug.Log($"{name} Died!");
        Destroy(gameObject, 2f); // Delete after 2 seconds
    }

    IEnumerator FlashColor()
    {
        meshRenderer.material.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = originalColor;
    }
}