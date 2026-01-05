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
    public Animator animator; // DRAG YOUR CHILD MODEL HERE IN INSPECTOR

    Transform target;
    NavMeshAgent agent;
    float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if(animator == null) animator = GetComponentInChildren<Animator>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update()
    {
        // 1. Update Animation Speed
        if(animator != null)
        {
            // agent.velocity.magnitude is the current speed
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (target == null) return;

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
        Debug.Log(name + " Attacks Player!");
        
        // Trigger Animation
        if(animator != null) animator.SetTrigger("Attack");
        
        // Deal Damage (Simple distance check hit)
        PlayerStats pStats = target.GetComponent<PlayerStats>();
        if (pStats != null)
        {
            pStats.TakeDamage(attackDamage);
        }
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