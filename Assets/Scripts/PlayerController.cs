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

    private float airControlFactor = 1f;
    private float airControlDecayRate = 1.8f; // 減衰スピード
    public int maxJumps = 2;
    int jumpsLeft;

    private bool isKnockback = false;

    private Rigidbody2D rb;
    private PlayerHealth ph;
    public Animator animator;
    private bool isJumping = false;
    private bool isDashing = false;

    // === Ability & attack ===
    public AbilityType currentAbility = AbilityType.Fire;
    public Transform attackPoint;
    private float baseMeleeRange = 0.5f;
    
    public LayerMask enemyLayer;

    // 攻撃クールダウン関連
    private bool canAttack = true;
    private float attackCooldown = 0f;

    // 各武器タイプごとのクールダウン設定
    [SerializeField] private float fireCooldown = 0.5f;
    [SerializeField] private float windCooldown = 1.0f;
    [SerializeField] private float waterCooldown = 0.7f;

    public int fireDamage = 6;
    public int windDamage = 4;
    public int waterDamage = 2;

    public GameObject SwordEffectPrefab;
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
        ph = GetComponent<PlayerHealth>();
        jumpsLeft = maxJumps;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        if (isKnockback) return;

        // 地上でのみShift入力を反映
        if (!isJumping)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                isDashing = true;
            else if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
                isDashing = false;
        }

        // クールダウンタイマーを減らす
        if (!canAttack)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
                canAttack = true;
        }

        // 攻撃入力
        if (inputActions.Player.Attack.WasPressedThisFrame())
        {
            if (canAttack)
                DoAttack();
        }

        // Core吸収
        //TryPickupCore();

        // ===== 向き変更 =====
        LookMoveDirection();

        // ===== ジャンプ =====
        if (inputActions.Player.Jump.WasPressedThisFrame() && jumpsLeft > 0)
        {
            DoJump();
        }   

        if (moveInput.x == 0)
        {
            animator.SetBool("isMoving", false);
        }
        else
        {
            animator.SetBool("isMoving", true);
        }

        // ===== 移動処理 =====
        Vector3 moveDirection = new Vector3(moveInput.x, 0, 0);
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime);
        
        float clampedX = Mathf.Clamp(transform.position.x, -8.3f, Mathf.Infinity);
        transform.position = new Vector3(clampedX, transform.position.y, 0);
    }

    void FixedUpdate()
    {
        if (isKnockback) return;

        if (!isJumping)
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

        // 攻撃を無効化してクールダウン開始
        canAttack = false;

        // 武器タイプに応じたクールダウン設定
        switch (currentAbility)
        {
            case AbilityType.Fire:
                attackCooldown = fireCooldown;
                break;
            case AbilityType.Wind:
                attackCooldown = windCooldown;
                break;
            case AbilityType.Water:
                attackCooldown = waterCooldown;
                break;
        }

        // killCountによるダメージ倍率補正
        int tier = killCount / killsPerTier;
        float dmgMul = 1f + tier * damageMultiplierPerTier;
        float rangeMul = 1f + tier * rangeMultiplierPerTier;

        if (currentAbility == AbilityType.Fire)
        {
            GameObject sword = Instantiate(SwordEffectPrefab, attackPoint);

            // SwordHitboxのパラメータ設定
            SwordHitbox hitbox = sword.GetComponent<SwordHitbox>();
            if (hitbox != null)
            {
                hitbox.damage = Mathf.RoundToInt(fireDamage * dmgMul); // 実ダメージを反映
                hitbox.enemyLayer = enemyLayer;                        // 敵レイヤー指定
                hitbox.duration = 0.3f;                                // 攻撃持続時間
            }

            // アニメーション同期などがあればここでトリガー
            // animator.SetTrigger("Attack");
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

    void DoJump()
    {
        isJumping = true;
        animator.SetBool("isJumping", true);
        airControlFactor = 1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
    }
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

    void SpawnFollowCore(AbilityType prev)
    {
        if (followCorePrefab == null) return;
        var obj = Instantiate(followCorePrefab, transform.position + Vector3.right * followOffsetX, Quaternion.identity);
        var fc = obj.GetComponent<FollowCore>();
        if (fc != null) fc.Init(this.transform, prev);
    }

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
            isJumping = false;
            animator.SetBool("isJumping", false);
            jumpsLeft = maxJumps; 
        }

        if (col.collider.CompareTag("Enemy"))
        {
            if (ph.IsInvincible()) return;

            EnemyHealth enemyHealth = col.collider.GetComponent<EnemyHealth>();

            if (GameManager.Instance.isGameActive)
            {
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(1);
                }

                ph.TakeDamage(1);
            }

            // ===== ノックバック処理 =====
            Vector2 knockDir = (transform.position - col.transform.position).normalized;

            float knockbackPowerX = 4.5f;
            float knockbackPowerY = 3.5f;

            // 現在の速度をリセット
            rb.linearVelocity = Vector2.zero;

            rb.linearVelocity = new Vector2(knockDir.x * knockbackPowerX, knockbackPowerY);

            StartCoroutine(KnockbackRoutine(0.3f));
        }
    }

    // 操作ロック（ジャンプ・移動を一時停止）
    IEnumerator KnockbackRoutine(float duration)
    {
        isKnockback = true;
        inputActions.Disable();

        yield return new WaitForSeconds(duration);

        isKnockback = false;
        inputActions.Enable();
    }
}
