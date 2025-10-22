using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float moveDistance = 3.0f;
    public float stopPatrolTime = 0.8f;
    private float groundCheckDistance = 0.3f;

    public int attackDamage = 1;
    private float attackRange = 0.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isStepping = false;
    private float attackHitDelay = 0.25f;

    private float trapFloatSpeed = 0.4f;
    private bool isTrapped = false;
    private bool isKnockbacked = false;

    private Vector3 startPos;
    private bool movingLeft = true;

    private Rigidbody2D rb;
    private Animator animator;

    public Transform attackPoint;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    private PlayerController pc;

    private Coroutine patrolCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    void Start()
    {
        patrolCoroutine = StartCoroutine(PatrolCycle());

        pc = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (isKnockbacked || isTrapped) return;
        
        TryAttack();

        // 足元の先に地面が無ければ移動中止（ただし攻撃中はチェック不要）
        if (!isAttacking && !IsGroundAhead())
        {
            animator.SetBool("isMoving", false);
            return;
        }
    }

    IEnumerator PatrolCycle()
    {
        while (true)
        {
            if (isTrapped) yield break;

            // 攻撃状態なら巡回を止める
            if (isAttacking)
            {
                if (!isStepping)
                    rb.linearVelocity = Vector2.zero;

                animator.SetBool("isMoving", false);
                yield return null;
                continue;
            }

            if (isKnockbacked)
            {
                rb.linearVelocity = Vector2.zero;

                yield return new WaitForSeconds(0.3f);
                continue;
            }

            float moveDir = movingLeft ? -1 : 1;
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);

            animator.SetBool("isMoving", true);

            yield return null;

            if (!movingLeft && transform.position.x >= startPos.x + moveDistance ||
                movingLeft && transform.position.x <= startPos.x - moveDistance)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;

                animator.SetBool("isMoving", false);

                yield return new WaitForSeconds(stopPatrolTime);

                rb.gravityScale = 1.8f;
                Flip();
                movingLeft = !movingLeft;

                startPos = transform.position;
            }
        }
    }

    void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
    }

    bool IsGroundAhead()
    {
        float currentFacingDirection = Mathf.Sign(transform.localScale.x);

        // デフォルトで左向きのため反転
        float moveDirection = -currentFacingDirection;

        float offset = GetComponent<Collider2D>().bounds.extents.x + 0.5f;
        Vector2 origin = new Vector2(transform.position.x + moveDirection * offset,
                                     GetComponent<Collider2D>().bounds.min.y);

        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);

        return groundHit.collider != null;
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isKnockbacked) return;

        // パトロールコルーチンを中断
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        // 攻撃コルーチンを中断
        if (isAttacking)
        {
            StopCoroutine("DoAttackRoutine");
            isAttacking = false;
            isStepping = false;
            animator.ResetTrigger("Attack");
        }

        // 移動を完全停止
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1.8f; // 重力を戻す
        animator.SetBool("isMoving", false);

        StartCoroutine(KnockbackCoroutine(force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force)
    {
        isKnockbacked = true;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = force;

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = Vector2.zero;
        isKnockbacked = false;

        // パトロール再開
        if (!isTrapped)
        {
            patrolCoroutine = StartCoroutine(PatrolCycle());
        }
    }

    void TryAttack()
    {
        if (isAttacking) return;

        if (Time.time < lastAttackTime + attackCooldown || pc == null) return;

        // 判定範囲内にプレイヤーがいるか（発動条件）
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hit)
        {
            // 攻撃を開始（アニメ同期は AnimationEvent を推奨）
            StartCoroutine(DoAttackRoutine());
        }
    }

    IEnumerator DoAttackRoutine()
    {
        isAttacking = true;

        // 攻撃アニメ再生
        animator.SetTrigger("Attack");

        isStepping = true;
        float moveDir = movingLeft ? -1 : 1;
        rb.linearVelocity = new Vector2(moveDir * 2f, rb.linearVelocity.y); // 踏み込み速度
        yield return new WaitForSeconds(0.2f); // 踏み込み時間

        rb.linearVelocity = Vector2.zero; // すぐ止まるように
        rb.gravityScale = 0f; // 停止中の滑り防止
        isStepping = false;

        yield return new WaitForSeconds(attackHitDelay); // アニメのヒットフレームに合わせる
        DoAttackDamage(); // このタイミングでダメージを与える

        // 攻撃完了処理
        lastAttackTime = Time.time;

        // 小さな余韻（アニメが終わるまで待つ or 即復帰させる）
        float postAttackDelay = Mathf.Max(0f, attackCooldown - attackHitDelay);
        yield return new WaitForSeconds(postAttackDelay);

        rb.gravityScale = 1.8f;
        isAttacking = false;
    }

    // 実際のダメージ判定（AnimationEvent またはコルーチンから呼ぶ）
    public void DoAttackDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hit)
        {
            var playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    // 水タイプ（泡）から呼ばれる
    public void Trap(float duration)
    {
        if (isTrapped) return;

        StopCoroutine(patrolCoroutine);
        StartCoroutine(DoTrap(duration));
    }

    IEnumerator DoTrap(float duration)
    {
        isTrapped = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // アニメーションを停止または「閉じ込められ」アニメに切り替え
        animator.SetBool("isTrapped", true);

        // 衝突判定を一時的にオフ (水面を歩けるようにするための工夫)
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float t = 0f;
        while (t < duration)
        {
            // ふわふわ浮遊
            if (rb != null) rb.linearVelocity = new Vector2(0, trapFloatSpeed);
            t += Time.deltaTime;
            yield return null;
        }

        // トラップ解除
        col.enabled = true;
        animator.SetBool("isTrapped", false);
        isTrapped = false;

        patrolCoroutine = StartCoroutine(PatrolCycle());
    }
}
