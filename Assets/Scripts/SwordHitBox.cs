using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 6;
    public float duration = 0.3f; // 攻撃が続く時間
    public LayerMask enemyLayer;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            var eh = other.GetComponent<EnemyHealth>();
            var erb = other.GetComponent<Rigidbody2D>();

            if (eh != null)
            {
                eh.TakeDamage(damage);

                if (erb != null && eh.GetHP() > 0)
                {
                    // ===== ノックバック処理 =====
                    Vector2 knockDir = (erb.transform.position - transform.position).normalized;

                    float knockbackPowerX = 4f;
                    float knockbackPowerY = 3f;

                    Vector2 knockForce = new Vector2(knockDir.x * knockbackPowerX, knockbackPowerY);

                    // Rigidbodyに直接速度を与える代わりに、敵スクリプト側へ通知
                    var slime = other.GetComponent<EnemyController>();
                    if (slime != null)
                    {
                        slime.ApplyKnockback(knockForce);
                    }

                    var frog = other.GetComponent<FrogController>();
                    if (frog != null)
                    {
                        frog.ApplyKnockback(knockForce);
                    }

                    var eagle = other.GetComponent<EagleController>();
                    if (eagle != null)
                    {
                        eagle.ApplyKnockback(knockForce);
                    }
                }
            }
        }
    }
}
