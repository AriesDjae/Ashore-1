using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int xpReward = 50;

    [Header("Respawn Settings")]
    public float respawnDelay = 5f; 
    public float respawnDistance = 20f; 

    [Header("Visual Feedback")]
    public Renderer meshRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    
    private EnemyAI ai;
    private NavMeshAgent agent;
    private Collider col;

    void Start()
    {
        currentHealth = maxHealth;
        ai = GetComponent<EnemyAI>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<Collider>();
        
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        if (ai != null) ai.Stun(0.5f);

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
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if(player != null) player.GainXP(xpReward);

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // 1. Trigger Death Animation & Disable AI
        if(ai != null) 
        {
            ai.enabled = false;
            // Access the animator through the AI script reference
            if(ai.animator) ai.animator.SetTrigger("Death");
        }
        
        if(agent != null) agent.isStopped = true;
        if(col != null) col.enabled = false; 

        Debug.Log($"{name} Died! Respawning in {respawnDelay}s...");

        // 2. Wait for animation to finish
        // Standard death anims are ~2 seconds. Adjust this if yours is longer.
        yield return new WaitForSeconds(2.0f);

        // 3. Hide Enemy Visuals (The "Poof" moment)
        if(meshRenderer != null) meshRenderer.enabled = false;
        
        // 4. Wait for the rest of the respawn timer
        yield return new WaitForSeconds(respawnDelay);

        // 5. Calculate New Position & Reset
        Vector3 respawnPos = CalculateRespawnPosition();
        Respawn(respawnPos);
    }

    Vector3 CalculateRespawnPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 targetPos = transform.position; 

        if (player != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * respawnDistance;
            Vector3 searchPos = player.transform.position + new Vector3(randomDir.x, 0, randomDir.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPos, out hit, 10f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }
        }
        return targetPos;
    }

    void Respawn(Vector3 position)
    {
        currentHealth = maxHealth;

        if(agent != null) agent.Warp(position);
        else transform.position = position;

        if(meshRenderer != null) meshRenderer.enabled = true;
        if(col != null) col.enabled = true;

        if(ai != null) 
        {
            ai.enabled = true;
            // Force reset to Idle so it doesn't play death anim again
            if(ai.animator) ai.animator.Play("Idle"); 
        }

        Debug.Log($"{name} Respawned at {position}!");
    }

    IEnumerator FlashColor()
    {
        meshRenderer.material.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = originalColor;
    }
}