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
        // ���񏈗����R���[�`���ŊJ�n
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

            // ���t���[�������x�ނ悤�ɂ���CPU���������Ȃ�
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

    // ���^�C�v�i�A�j����Ă΂��
    public void Trap(float duration)
    {
        if (isTrapped) return;

        StopCoroutine(patrolCoroutine);
        StartCoroutine(DoTrap(duration));
    }

    IEnumerator DoTrap(float duration)
    {
        isTrapped = true;
        // AI�ƕ������Z���ꎞ��~
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // �A�j���[�V�������~�܂��́u�����߂��v�A�j���ɐ؂�ւ�
        animator.SetBool("isTrapped", true);

        // �Փ˔�����ꎞ�I�ɃI�t (���ʂ������悤�ɂ��邽�߂̍H�v)
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float t = 0f;
        while (t < duration)
        {
            // �ӂ�ӂ핂�V
            if (rb != null) rb.linearVelocity = new Vector2(0, trapFloatSpeed);
            t += Time.deltaTime;
            yield return null;
        }

        // �g���b�v����
        col.enabled = true;
        animator.SetBool("isTrapped", false);
        isTrapped = false;

        patrolCoroutine = StartCoroutine(PatrolCycle());
    }
}
