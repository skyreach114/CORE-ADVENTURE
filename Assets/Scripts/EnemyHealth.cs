using UnityEngine;
using System.Collections; // IEnumeratorのために必要

public class EnemyHealth : MonoBehaviour
{
    // === Inspector設定項目 ===
    public int maxHealth = 3;
    public float trapFloatSpeed = 0.6f; // 泡に閉じ込められた時の浮遊速度
    public GameObject dieEffectPrefab;
    public AudioSource damageSound;

    [SerializeField] private EnemyCore enemyCore; // コアクラスへの参照

    // === 内部変数 ===
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

    // 白点滅処理
    IEnumerator FlashCoroutine()
    {
        isFlashing = true;

        float flashInterval = 0.05f;

        AudioSource.PlayClipAtPoint(damageSound.clip, transform.position, 0.3f);

        float startTime = Time.time;

        while (Time.time < startTime + invincibilityDuration)
        {
            // 点滅（Rendererをオンオフする）
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }

        // コルーチン終了後、元の状態に戻す
        spriteRenderer.enabled = true;
        isFlashing = false;
    }

    // 死亡処理
    public void Die()
    {

        // コアのドロップを要求
        if (enemyCore != null)
        {
            enemyCore.DropCore();
        }

        Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}