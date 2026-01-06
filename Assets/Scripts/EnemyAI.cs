using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public float lookRadius = 15f;
    public float attackRadius = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Animation")]
    public Animator animator; 

    Transform target;
    NavMeshAgent agent;
    float lastAttackTime;
    
    // NEW: Stun State
    private bool isStunned = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if(animator == null) animator = GetComponentInChildren<Animator>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update()
    {
        // STOP if stunned or dead (handled by disabling script, but safety check here)
        if (isStunned || target == null) return;

        // Sync Animation Speed
        if(animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);

            if (distance <= attackRadius)
            {
                FaceTarget();
                
                if (Time.time - lastAttackTime > attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void Attack()
    {
        if(animator != null) animator.SetTrigger("Attack");
        
        PlayerStats pStats = target.GetComponent<PlayerStats>();
        if (pStats != null)
        {
            pStats.TakeDamage(attackDamage);
        }
    }

    // --- NEW STUN LOGIC ---
    public void Stun(float duration)
    {
        if (isStunned) return; // Already stunned
        StartCoroutine(StunRoutine(duration));
    }

    System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        
        // Stop moving
        if(agent.isOnNavMesh) agent.isStopped = true;
        
        // Play Hit Animation
        if(animator != null) animator.SetTrigger("GetHit");

        yield return new WaitForSeconds(duration);

        // Resume
        isStunned = false;
        if(agent.isOnNavMesh) agent.isStopped = false;
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}