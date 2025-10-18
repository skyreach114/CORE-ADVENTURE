using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float moveDistance = 3.0f;
    public float stopPatrolTime = 0.8f;

    public int attackDamage = 1;
    public float attackRange = 0.8f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private float trapFloatSpeed = 0.4f;
    private bool isTrapped = false;

    private Vector3 startPos;
    private bool movingLeft = true;

    private Rigidbody2D rb;
    private Animator animator;

    public Transform attackPoint;
    public LayerMask playerLayer;
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
        // 巡回処理をコルーチンで開始
        patrolCoroutine = StartCoroutine(PatrolCycle());

        pc = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (isTrapped) return;
        
        TryAttack();
    }

    IEnumerator PatrolCycle()
    {
        while (true)
        {
            if (isTrapped) yield break;

            float moveDir = movingLeft ? -1 : 1;
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);

            animator.SetBool("isMoving", true);

            // 毎フレーム少し休むようにしてCPUを圧迫しない
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

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown || pc == null) return;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hit)
        {
            animator.SetTrigger("Attack");
            var playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            lastAttackTime = Time.time;
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
        // AIと物理演算を一時停止
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
