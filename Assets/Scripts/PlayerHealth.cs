using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 5;
    public int currentHP;

    private float invincibilityDuration = 1f;
    private bool isInvincible = false;

    Camera mainCamera;
    private float moveCamX = 0.7f;

    private SpriteRenderer spriteRenderer;
    public GameObject dieEffectPrefab;

    public AudioSource damageSound;
    public AudioSource dieSound;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    public int GetHP()
    {
        return currentHP;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return;

        currentHP -= damageAmount;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            //StartCoroutine(InvincibilityCoroutine());
            StartCoroutine(CameraShake());
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float flashInterval = 0.08f;

        AudioSource.PlayClipAtPoint(damageSound.clip, new Vector3(0, 0, -8), 0.3f);

        float startTime = Time.time;

        while (Time.time < startTime + invincibilityDuration)
        {
            // 点滅（Rendererをオンオフする）
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }

        // コルーチン終了後、元の状態に戻す
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    IEnumerator CameraShake()
    {
        for (int i = 0; i < 10; i++)
        {
            mainCamera.transform.Translate(moveCamX, 0, 0);
            moveCamX *= -1;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Die()
    {
        currentHP = 0;

        HPIcon hpIcon = FindFirstObjectByType<HPIcon>();
        if (hpIcon != null)
        {
            hpIcon.ForceUpdateIcon();
        }

        GameManager.Instance.GameOver();

        Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(dieSound.clip, transform.position, 0.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (GameManager.Instance.isGameActive)
            {
                TakeDamage(1);
            }
        }
    }
}
