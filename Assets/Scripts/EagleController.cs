using System.Collections;
using UnityEngine;

public class EagleController : MonoBehaviour
{
    public float detectRange = 5f;
    public float attackSpeed = 7.5f;
    public float returnSpeed = 6f;
    public float idleAmplitude = 0.3f; // 羽ばたき上下の幅
    public float idleFrequency = 2f;
    private float maxAttackDuration = 1f;   // 最大攻撃時間（無限ループ防止）
    private float maxReturnDuration = 1f;   // 最大帰還時間（無限ループ防止）

    private Transform player;
    private Vector3 startPos;
    private bool isAttacking = false;
    private bool isReturning = false;
    private bool isKnockbacked = false;

    private Rigidbody2D rb;
    private Animator animator;

    private float idleTimeOffset;

    private Coroutine currentRoutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPos = transform.position;

        idleTimeOffset = Random.Range(0f, 2f * Mathf.PI); // 羽ばたきのズレを持たせると自然
    }

    void Update()
    {
        if (isKnockbacked || isAttacking || isReturning) return;

        if (player != null)
        {
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

                currentRoutine = StartCoroutine(AttackPlayer());
            }
            else
            {
                if (currentRoutine != null)
                {
                    StopCoroutine(currentRoutine);
                    currentRoutine = null;

                    isAttacking = false;
                    isReturning = false;
                    animator.SetBool("isAttacking", false);
                    rb.linearVelocity = Vector2.zero; // 動きをリセット
                }
            }
        }

        // 羽ばたきアニメーション：上下にゆらゆら
        float newY = startPos.y + Mathf.Sin(Time.time * idleFrequency + idleTimeOffset) * idleAmplitude;
        Vector2 targetPos = new Vector2(transform.position.x, newY);
        rb.MovePosition(targetPos);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isKnockbacked) return;

        // 攻撃・帰還を強制解除
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        isAttacking = false;
        isReturning = false;
        animator.SetBool("isAttacking", false);

        StartCoroutine(KnockbackCoroutine(force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force)
    {
        isKnockbacked = true;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = force;

        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;
        isKnockbacked = false;
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        animator.SetBool("isAttacking", true);
        yield return null; // 1フレーム待ってアニメ遷移が反映されるように

        Vector3 target = player.position;
        float timer = 0f;

        while (Vector2.Distance(transform.position, target) > 0.5f && timer < maxAttackDuration)
        {
            // ノックバックされたら中断
            if (isKnockbacked)
            {
                rb.linearVelocity = Vector2.zero;
                yield break;
            }

            Vector2 direction = (target - transform.position).normalized;
            rb.linearVelocity = direction * attackSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        // 目標到達時に速度を停止
        rb.linearVelocity = Vector2.zero;

        // 攻撃完了後、元の位置に戻る
        animator.SetBool("isAttacking", false);
        isAttacking = false;
        isReturning = true;
        timer = 0f;

        while (Vector2.Distance(transform.position, startPos) > 0.1f && timer < maxReturnDuration)
        {
            // ノックバックされたら中断
            if (isKnockbacked)
            {
                isReturning = false;
                rb.linearVelocity = Vector2.zero;
                yield break;
            }

            Vector2 direction = (startPos - transform.position).normalized;
            rb.linearVelocity = direction * returnSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        // 帰還完了
        rb.linearVelocity = Vector2.zero;
        isReturning = false;
        rb.MovePosition(startPos);
    }
}

