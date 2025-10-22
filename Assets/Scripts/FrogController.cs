using System.Collections;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    private float jumpForce = 6.5f;
    private float jumpInterval = 1.5f;
    private float detectRange = 8.5f;
    private float groundCheckDistance = 0.3f; // 地面チェックの距離

    private Animator animator;
    public LayerMask groundLayer;
    private Transform player;
    private Rigidbody2D rb;

    private bool isGrounded;
    private bool canJump = true;
    private float jumpCooldown = 0f;
    private float lastDirX = 1f;

    private float trapFloatSpeed = 0.4f;
    private bool isTrapped = false;
    private bool isKnockbacked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("プレイヤーを見つけた");
        }
    }

    void Update()
    {
        if (isKnockbacked || isTrapped) return;

        // クールダウン管理
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;

            if (jumpCooldown <= 0)
            {
                canJump = true;
                rb.linearVelocity = Vector2.zero;
            }
        }

        // 地面判定
        Collider2D col = GetComponent<Collider2D>();
        float checkDistance = 0.1f;

        Vector2 boxCenter = new Vector2(transform.position.x, col.bounds.min.y - checkDistance * 0.5f);
        Vector2 boxSize = new Vector2(col.bounds.size.x * 0.8f, checkDistance);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, checkDistance, groundLayer);
        isGrounded = hit.collider != null;

        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.4f)
        {
            animator.SetBool("isMoving", false);
            rb.linearVelocity = Vector2.zero;
        }

        // ジャンプしている場合、または地上にいない場合は処理しない
        if(!isGrounded || !canJump || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectRange)
        {
            // プレイヤーの方向を向く
            if (player.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1.3f, 1.3f, 1);
            }
            else
            {
                transform.localScale = new Vector3(1.3f, 1.3f, 1);
            }

            // 足元の先に地面が無ければ移動中止
            if (!IsGroundAhead())
            {
                animator.SetBool("isMoving", false);
                return;
            }

            Vector2 dir = (player.position - transform.position).normalized;
            Jump(dir);
        }
    }

    void Jump(Vector2 dir)
    {
        canJump = false;
        jumpCooldown = jumpInterval;

        // X方向のベクトルが小さすぎる場合は前回の向きを使う
        if (Mathf.Abs(dir.x) < 0.4f)
            dir.x = lastDirX;

        // 現在の向きを記録
        lastDirX = Mathf.Sign(dir.x);

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(dir.x * jumpForce * 0.45f, jumpForce);

        animator.SetBool("isMoving", true);
    }

    // デバッグ用：地面判定の可視化
    void OnDrawGizmosSelected()
    {
        if (GetComponent<Collider2D>() == null) return;

        Gizmos.color = Color.cyan;
        float currentFacingDirection = Mathf.Sign(transform.localScale.x);
        float moveDirection = -currentFacingDirection;
        float offset = GetComponent<Collider2D>().bounds.extents.x + 0.8f;
        Vector2 origin = new Vector2(transform.position.x + moveDirection * offset,
                                     GetComponent<Collider2D>().bounds.min.y);
        Gizmos.DrawLine(origin, origin + Vector2.down * (groundCheckDistance + 1.0f));
    }

    bool IsGroundAhead()
    {
        // 進行方向 (1:右, -1:左) を取得
        float currentFacingDirection = Mathf.Sign(transform.localScale.x);

        // 向いている方向と逆の方向に進むスケールなので反転
        float moveDirection = -currentFacingDirection;

        // チェックを始める位置：少し先
        float offset = GetComponent<Collider2D>().bounds.extents.x + 0.8f;
        Vector2 origin = new Vector2(transform.position.x + moveDirection * offset,
                                     GetComponent<Collider2D>().bounds.min.y);

        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        if (groundHit.collider == null)
        {
            float extraDistance = groundCheckDistance + 1.0f; // ← 少し深めに伸ばす
            groundHit = Physics2D.Raycast(origin, Vector2.down, extraDistance, groundLayer);
            Debug.DrawRay(origin, Vector2.down * extraDistance, Color.yellow);
        }
        else
        {
            Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);
        }

        return groundHit.collider != null;
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isKnockbacked) return;
        StartCoroutine(KnockbackCoroutine(force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force)
    {
        isKnockbacked = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = Vector2.zero;
        isKnockbacked = false;
    }

    // 水タイプ（泡）から呼ばれる
    public void Trap(float duration)
    {
        if (isTrapped) return;

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
    }
}
