using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public float lookRadius = 15f;  // How far it can see
    public float attackRadius = 2f; // How close to bite
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    Transform target;
    NavMeshAgent agent;
    float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Find Player automatically
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, transform.position);

        // 1. Chase
        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);

            // 2. Attack
            if (distance <= attackRadius)
            {
                // Face the target
                FaceTarget();
                
                // Attack logic
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
        // If you have animations: GetComponent<Animator>().SetTrigger("Attack");
        
        // Deal Damage
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