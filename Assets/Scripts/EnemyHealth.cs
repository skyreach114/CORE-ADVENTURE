using UnityEngine;
using System.Collections; // IEnumerator�̂��߂ɕK�v

public class EnemyHealth : MonoBehaviour
{
    // === Inspector�ݒ荀�� ===
    public int maxHealth = 3;
    public float trapFloatSpeed = 0.6f; // �A�ɕ����߂�ꂽ���̕��V���x
    public GameObject dieEffectPrefab;
    public AudioSource damageSound;

    [SerializeField] private EnemyCore enemyCore; // �R�A�N���X�ւ̎Q��

    // === �����ϐ� ===
    private float invincibilityDuration = 0.4f;
    protected int currentHealth;
    protected SpriteRenderer spriteRenderer;
    private bool isFlashing = false;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyCore == null) enemyCore = GetComponent<EnemyCore>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isFlashing) return;

        currentHealth -= damageAmount;

        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ���_�ŏ���
    IEnumerator FlashCoroutine()
    {
        isFlashing = true;

        float flashInterval = 0.05f;

        AudioSource.PlayClipAtPoint(damageSound.clip, transform.position, 0.3f);

        float startTime = Time.time;

        while (Time.time < startTime + invincibilityDuration)
        {
            // �_�ŁiRenderer���I���I�t����j
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }

        // �R���[�`���I����A���̏�Ԃɖ߂�
        spriteRenderer.enabled = true;
        isFlashing = false;
    }

    // ���S����
    public void Die()
    {

        // �R�A�̃h���b�v��v��
        if (enemyCore != null)
        {
            enemyCore.DropCore();
        }

        Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}