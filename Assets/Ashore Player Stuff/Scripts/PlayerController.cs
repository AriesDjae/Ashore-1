using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(InventoryManager))]
public class PlayerController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator; 

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;
    
    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float jumpCost = 10f;

    [Header("Dodge Settings")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.2f;
    public float dodgeCost = 20f;
    public float dodgeCooldown = 0.5f;

    [Header("Defense Settings")]
    public KeyCode blockKey = KeyCode.E;
    public float parryWindowDuration = 0.2f; 
    [Range(0f, 1f)] public float blockMovementMultiplier = 0.5f; 

    [Header("Combat Settings")]
    public KeyCode toggleCombatKey = KeyCode.X;
    public float attackRange = 2.0f;
    public float comboResetTime = 1.0f;
    public float attackCooldown = 0.5f; 
    [Range(0f, 1f)] public float attackMovementMultiplier = 0.1f; // Slows down standing attacks
    public LayerMask enemyLayer;

    // --- SKILL TREE UNLOCKS & MODIFIERS ---
    [Header("Skill Unlocks & Stats")]
    public bool unlockedHeavyAttack = false; 
    public bool unlockedDoubleJump = false; 
    public float staminaCostMultiplier = 1.0f; 
    public float stealthDetectionMultiplier = 1.0f; 
    public float damageMultiplier = 1.0f; 

    // References
    private CharacterController controller;
    private PlayerStats stats;
    private InventoryManager inventory;
    private Transform cameraTransform;
    private CameraFollow cameraScript;
    private SkillManager skillManager;

    // State
    private Vector3 velocity;
    private bool isDodging = false;
    private bool isAttacking = false;
    private bool canDodge = true;
    public bool isBlocking = false; 
    public bool isParrying = false; 
    
    // Combat State
    public bool isCombatMode = false;
    // Removed isRunningAttack flag
    private int comboCount = 0;
    private float lastAttackTime = 0;
    private int jumpCount = 0;
    
    // Temp storage for the current swing's damage
    private float pendingDamage = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        skillManager = GetComponent<SkillManager>();
        inventory = GetComponent<InventoryManager>();
        
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            cameraScript = Camera.main.GetComponent<CameraFollow>();
        }
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) { velocity.y = -2f; jumpCount = 0; }
        if (isDodging) return; 

        velocity.y += gravity * Time.deltaTime;

        HandleDefenseInput(); 
        HandleCombatMode(); 

        // Speed Logic
        float currentSpeed = moveSpeed;
        if (isBlocking) 
        {
            currentSpeed *= blockMovementMultiplier;
        }
        else if (isAttacking) 
        {
            // Always slow down when attacking to commit to the swing
            currentSpeed *= attackMovementMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftControl)) 
        {
            currentSpeed *= 0.5f;
        }

        Vector3 horizontalVelocity = CalculateHorizontalMovement(currentSpeed);
        HandleRotation(horizontalVelocity);

        if (!isBlocking) 
        {
            HandleActionInput(isGrounded);
        }

        Vector3 finalMove = horizontalVelocity + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);

        UpdateAnimation(horizontalVelocity);
    }

    void UpdateAnimation(Vector3 currentVelocity)
    {
        if (animator == null) return;
        float horizontalSpeed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;
        
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsBlocking", isBlocking);
        animator.SetBool("IsCombatMode", isCombatMode); 
    }

    void HandleCombatMode()
    {
        if (Input.GetKeyDown(toggleCombatKey))
        {
            isCombatMode = !isCombatMode;
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboCount = 0;
        }
    }

    void HandleDefenseInput()
    {
        if (Input.GetKeyDown(blockKey) && !isAttacking && !isDodging)
        {
            StartCoroutine(PerformParryWindow());
        }
        if (Input.GetKey(blockKey) && !isAttacking && !isDodging)
        {
            isBlocking = true;
        }
        else if (Input.GetKeyUp(blockKey))
        {
            isBlocking = false;
            isParrying = false;
        }
    }

    IEnumerator PerformParryWindow()
    {
        isParrying = true;
        yield return new WaitForSeconds(parryWindowDuration);
        isParrying = false;
    }

    void HandleRotation(Vector3 currentMoveVelocity)
    {
        bool isIsometric = cameraScript != null && cameraScript.currentMode == CameraFollow.CameraMode.Isometric;

        if (isIsometric) RotateTowardsMouse();
        else if (currentMoveVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentMoveVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 lookDir = ray.GetPoint(rayDistance) - transform.position;
            lookDir.y = 0; 
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * 2f * Time.deltaTime);
            }
        }
    }

    Vector3 CalculateHorizontalMovement(float speed)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();
            return (camForward * direction.z + camRight * direction.x).normalized * speed;
        }
        return Vector3.zero;
    }

    void HandleActionInput(bool isGrounded)
    {
        if (Input.GetButtonDown("Jump") && isGrounded && stats.HasStamina(jumpCost))
        {
            stats.UseStamina(jumpCost);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDodge && !isAttacking)
        {
            if(stats.HasStamina(dodgeCost)) StartCoroutine(PerformDodge());
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformComboAttack(); 
        }
    }

    void PerformComboAttack()
    {
        // Calculate damage
        pendingDamage = inventory.GetCurrentDamage() * damageMultiplier;
        float cost = inventory.GetCurrentStaminaCost() * staminaCostMultiplier;

        if (!stats.HasStamina(cost)) return;

        isCombatMode = true; 
        
        // Removed Running Logic check. Always do standard combo.
        StartCoroutine(ExecuteAttack(cost));
    }

    IEnumerator ExecuteAttack(float cost)
    {
        isAttacking = true;
        
        stats.UseStamina(cost);
        lastAttackTime = Time.time;

        // Always increment combo, never reset to 0 (unless time runs out in Update)
        comboCount++;
        if (comboCount > 3) comboCount = 1;
        
        Debug.Log($"<color=red><b>[COMBAT]</b> Combo {comboCount}</color>");

        animator.SetInteger("ComboCounter", comboCount);
        animator.SetTrigger("Attack"); 

        // Wait for Animation Event or Safety Timer
        float safetyTimer = 0f;
        while(isAttacking && safetyTimer < 2.0f)
        {
            safetyTimer += Time.deltaTime;
            // Cooldown logic
            if(safetyTimer > attackCooldown) break;
            yield return null;
        }
        
        isAttacking = false;
    }

    public void OnAttackHit()
    {
        if (!isAttacking) return;

        Debug.Log("<color=red><b>[EVENT]</b> 'Hit' Event received!</color>");

        Vector3 hitPos = transform.position + transform.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(hitPos, attackRange, enemyLayer);
        foreach(var hit in hits)
        {
            Debug.Log($"<color=red><b>[HIT]</b> {hit.name}!</color>");
            DummyEnemy enemy = hit.GetComponent<DummyEnemy>();
            if(enemy != null) enemy.TakeDamage(pendingDamage);
        }
    }

    IEnumerator PerformDodge()
    {
        isDodging = true;
        canDodge = false;
        stats.UseStamina(dodgeCost);
        animator.SetTrigger("Dodge");
        
        float startTime = Time.time;
        Vector3 dodgeDir = transform.forward;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if(Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0)
        {
             Vector3 camForward = cameraTransform.forward; 
             Vector3 camRight = cameraTransform.right;
             camForward.y = 0; camRight.y = 0;
             dodgeDir = (camForward * v + camRight * h).normalized;
             transform.rotation = Quaternion.LookRotation(dodgeDir);
        }

        while (Time.time < startTime + dodgeDuration)
        {
            controller.Move(dodgeDir * dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        isDodging = false;
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    void OnDrawGizmos()
    {
        if (isAttacking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRange);
        }
    }
}