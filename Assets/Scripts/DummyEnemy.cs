using UnityEngine;
using System.Collections;

public class DummyEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Visual Feedback")]
    public Renderer meshRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Auto-assign renderer if not set manually
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        
        // Save original color for the flash effect
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage. Current HP: {currentHealth}");

        // Visual Feedback
        if (meshRenderer != null) StopAllCoroutines();
        if (meshRenderer != null) StartCoroutine(FlashColor());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} Died!");
        // Simple death: disappear. In a real game, play animation/particle first.
        Destroy(gameObject); 
    }

    IEnumerator FlashColor()
    {
        meshRenderer.material.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = originalColor;
    }
}