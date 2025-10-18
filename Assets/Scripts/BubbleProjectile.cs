using UnityEngine;

public class BubbleProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 3.5f;
    public int baseDamage = 2;
    float dmgMultiplier = 1f;
    Vector3 dir = Vector2.right;

    public void Init(int damage, float dmgMul)
    {
        baseDamage = damage;
        dmgMultiplier = dmgMul;
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var eh = other.GetComponent<EnemyHealth>();
            var ec = other.GetComponent<EnemyController>();

            if (eh != null)
            {
                int dmg = Mathf.RoundToInt(baseDamage * dmgMultiplier);
                eh.TakeDamage(dmg);
                // trap enemy
                ec.Trap(2.5f); // trapped duration
                // optionally attach a visual bubble: omitted for simplicity
                Destroy(gameObject);
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
