using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    public float speed = 7f;
    public float maxDistance = 6f;
    public float returnSpeed = 9f;
    public int baseDamage = 4;
    Transform owner;
    Vector3 startPos;
    bool returning = false;
    float dmgMultiplier = 1f;

    public void Init(Transform ownerTransform, int damage, float dmgMul)
    {
        owner = ownerTransform;
        startPos = transform.position;
        baseDamage = damage;
        dmgMultiplier = dmgMul;
        // set rotation / sprite as needed
    }

    void Update()
    {
        if (!returning)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (Vector3.Distance(startPos, transform.position) >= maxDistance) returning = true;
        }
        else
        {
            if (owner == null) Destroy(gameObject);
            Vector3 dir = (owner.position - transform.position).normalized;
            transform.position += dir * returnSpeed * Time.deltaTime;
            if (Vector3.Distance(owner.position, transform.position) < 0.5f) Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var e = other.GetComponent<EnemyHealth>();
            if (e != null)
            {
                int dmg = Mathf.RoundToInt(baseDamage * dmgMultiplier);
                e.TakeDamage(dmg); // no direct killer ref (could be set)
            }
        }
    }
}

