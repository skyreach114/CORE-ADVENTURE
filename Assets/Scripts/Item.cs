using UnityEngine;

// アイテムの種類を定義
public enum ItemType { HP_RECOVER, EXP_BOOST }

public class Item : MonoBehaviour
{
    public ItemType itemType;

    private PlayerHealth playerHealth;

    public AudioSource HP_RecoverSound;
    public AudioSource EXP_BoostSound;

    void Start()
    {
        // プレイヤーのコンポーネントを初期化時に取得しておく
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // プレイヤーをTagで探す
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect();
            Destroy(gameObject);
        }

        void ApplyEffect()
        {
            if (itemType == ItemType.HP_RECOVER)
            {
                if (HP_RecoverSound != null)
                {
                    AudioSource.PlayClipAtPoint(HP_RecoverSound.clip, new Vector3(0, 0, -8), 0.3f);
                }
                if (playerHealth != null && playerHealth.currentHP < playerHealth.maxHP)
                {
                    playerHealth.currentHP += 1;
                    GameManager.Instance.hpIconUI.ForceUpdateIcon();
                }
            }
            else if (itemType == ItemType.EXP_BOOST)
            {
                if (EXP_BoostSound != null && PlayerExperience.Instance != null)
                {
                    PlayerExperience.Instance.AddExp(1);
                    AudioSource.PlayClipAtPoint(EXP_BoostSound.clip, new Vector3(0, 0, -8), 0.3f);
                }
            }
        }
    }
}