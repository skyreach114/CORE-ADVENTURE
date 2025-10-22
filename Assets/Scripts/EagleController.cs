using System.Collections;
using UnityEngine;

public class EagleController : MonoBehaviour
{
    public float detectRange = 5f;
    public float attackSpeed = 7.5f;
    public float returnSpeed = 6f;
    public float idleAmplitude = 0.3f; // �H�΂����㉺�̕�
    public float idleFrequency = 2f;
    private float maxAttackDuration = 1f;   // �ő�U�����ԁi�������[�v�h�~�j
    private float maxReturnDuration = 1f;   // �ő�A�Ҏ��ԁi�������[�v�h�~�j

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

        idleTimeOffset = Random.Range(0f, 2f * Mathf.PI); // �H�΂����̃Y������������Ǝ��R
    }

    void Update()
    {
        if (isKnockbacked || isAttacking || isReturning) return;

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance < detectRange)
            {
                // �v���C���[�̕���������
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
                    rb.linearVelocity = Vector2.zero; // ���������Z�b�g
                }
            }
        }

        // �H�΂����A�j���[�V�����F�㉺�ɂ����
        float newY = startPos.y + Mathf.Sin(Time.time * idleFrequency + idleTimeOffset) * idleAmplitude;
        Vector2 targetPos = new Vector2(transform.position.x, newY);
        rb.MovePosition(targetPos);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isKnockbacked) return;

        // �U���E�A�҂���������
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
        yield return null; // 1�t���[���҂��ăA�j���J�ڂ����f�����悤��

        Vector3 target = player.position;
        float timer = 0f;

        while (Vector2.Distance(transform.position, target) > 0.5f && timer < maxAttackDuration)
        {
            // �m�b�N�o�b�N���ꂽ�璆�f
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

        // �ڕW���B���ɑ��x���~
        rb.linearVelocity = Vector2.zero;

        // �U��������A���̈ʒu�ɖ߂�
        animator.SetBool("isAttacking", false);
        isAttacking = false;
        isReturning = true;
        timer = 0f;

        while (Vector2.Distance(transform.position, startPos) > 0.1f && timer < maxReturnDuration)
        {
            // �m�b�N�o�b�N���ꂽ�璆�f
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

        // �A�Ҋ���
        rb.linearVelocity = Vector2.zero;
        isReturning = false;
        rb.MovePosition(startPos);
    }
}

