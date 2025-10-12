using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // === Movement ===
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private float currentSpeed;
    public float normalSpeed = 3f;
    public float dashSpeed = 4.8f;
    public float jumpForce = 9.5f;

    // 空中での横方向の効き具合（0〜1）
    private float airControlFactor = 1f;
    private float airControlDecayRate = 1.8f; // 減衰スピード（大きいほど早く減速）
    public int maxJumps = 2;
    int jumpsLeft;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isDashing = false;

    // === Ability & attack ===
    //public AbilityType currentAbility = AbilityType.Fire;
    public Transform attackPoint;
    public float baseMeleeRange = 0.9f;
    public LayerMask enemyLayer;

    public int fireDamage = 6;
    public int windDamage = 4;
    public int waterDamage = 2;

    // boomerang / bubble prefabs
    public GameObject boomerangPrefab;
    public GameObject bubblePrefab;

    // core/follow
    public GameObject followCorePrefab;
    public float followOffsetX = -1.0f;

    // 吸収 / インタラクト
    public float corePickupRadius = 1.0f;

    // 成長（敵を一定数倒すと攻撃力＆攻撃範囲アップ）
    public int killCount = 0;
    public int killsPerTier = 5;
    public float damageMultiplierPerTier = 0.2f; // 5キルで +20% ダメージ
    public float rangeMultiplierPerTier = 0.15f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        jumpsLeft = maxJumps;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        // 地上でのみShift入力を反映
        if (isGrounded)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                isDashing = true;
            else if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
                isDashing = false;
        }

        if (inputActions.Player.Attack.WasPressedThisFrame())
        {
            DoAttack();
        }

        // Absorb Core
        //TryPickupCore();

        // ===== 向き変更 =====
        LookMoveDirection();

        // ===== 移動処理 =====
        Vector3 moveDirection = new Vector3(moveInput.x, 0, 0);
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime);
        
        float clampedX = Mathf.Clamp(transform.position.x, -8.3f, 8.3f);
        transform.position = new Vector3(clampedX, transform.position.y, 0);

        // ===== ジャンプ =====
        if (inputActions.Player.Jump.WasPressedThisFrame() && jumpsLeft > 0)
        {
            DoJump();
        }
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            airControlFactor = 1f; // 地上に着地したらリセット
        }
        else
        {
            // 空中にいる間は徐々に減衰
            airControlFactor = 0.8f;
            airControlFactor = Mathf.Max(0.3f, airControlFactor - airControlDecayRate * Time.fixedDeltaTime);
        }

        // ===== スピード設定 =====
        float targetSpeed = isDashing ? dashSpeed : normalSpeed;

        currentSpeed = targetSpeed;

        float moveX = moveInput.x * currentSpeed;
        rb.linearVelocity = new Vector2(moveX * airControlFactor, rb.linearVelocity.y);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void LookMoveDirection()
    {
        if (moveInput.x > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (moveInput.x < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void DoAttack()
    {
        Debug.Log("攻撃！");
    }

 
    void DoJump()
    {
        isGrounded = false;
        airControlFactor = 1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
    }


    /*
    void DoAttack()
    {
        // damage multipliers from killCount tiers
        int tier = killCount / killsPerTier;
        float dmgMul = 1f + tier * damageMultiplierPerTier;
        float rangeMul = 1f + tier * rangeMultiplierPerTier;

        if (currentAbility == AbilityType.Fire)
        {
            // Melee sword: overlap circle
            float range = baseMeleeRange * rangeMul;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);
            int dmg = Mathf.RoundToInt(fireDamage * dmgMul);
            foreach (var c in hits)
            {
                var enemy = c.GetComponent<Enemy>();
                if (enemy != null) enemy.TakeDamage(dmg, this);
            }
        }
        else if (currentAbility == AbilityType.Wind)
        {
            // Boomerang: spawn projectile
            var b = Instantiate(boomerangPrefab, attackPoint.position, Quaternion.identity);
            var bp = b.GetComponent<BoomerangProjectile>();
            if (bp != null) bp.Init(transform, windDamage, dmgMul);
        }
        else if (currentAbility == AbilityType.Water)
        {
            // Bubble gun: spawn projectile
            var b = Instantiate(bubblePrefab, attackPoint.position, Quaternion.identity);
            var bp = b.GetComponent<BubbleProjectile>();
            if (bp != null) bp.Init(waterDamage, dmgMul);
        }
    }
    */

    /*
    void TryPickupCore()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, corePickupRadius);
        Collider2D chosen = null;
        float minDist = float.MaxValue;
        foreach (var c in cols)
        {
            if (c.CompareTag("Core_Fire") || c.CompareTag("Core_Wind") || c.CompareTag("Core_Water"))
            {
                float d = Vector2.SqrMagnitude((Vector2)c.transform.position - (Vector2)transform.position);
                if (d < minDist) { minDist = d; chosen = c; }
            }
        }
        if (chosen != null)
        {
            Core core = chosen.GetComponent<Core>();
            if (core != null)
            {
                AbsorbCore(core);
            }
        }
    }
    */

    /*
    public void AbsorbCore(Core core)
    {
        // Determine ability from core's assigned type
        AbilityType newType = core.ability;
        // if same type, just destroy core (or give small bonus)
        if (newType == currentAbility)
        {
            Destroy(core.gameObject);
            // optional: small heal or score - omitted for simplicity
            return;
        }

        // spawn a follow-core representing previous ability
        SpawnFollowCore(currentAbility);

        // switch ability
        currentAbility = newType;
        Destroy(core.gameObject);

        // Visual: change sprite tint or play small effect (left to you)
    }
    */

    /*
    void SpawnFollowCore(AbilityType prev)
    {
        if (followCorePrefab == null) return;
        var obj = Instantiate(followCorePrefab, transform.position + Vector3.right * followOffsetX, Quaternion.identity);
        var fc = obj.GetComponent<FollowCore>();
        if (fc != null) fc.Init(this.transform, prev);
    }
    */

    // called by Enemy when killed by this player
    public void OnEnemyKilled()
    {
        killCount++;
        // optionally update UI or visual feedback
    }

    // For debug/visualization
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, baseMeleeRange);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, corePickupRadius);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            isDashing = false;
            isGrounded = true;
            jumpsLeft = maxJumps; 
        }
    }
}
