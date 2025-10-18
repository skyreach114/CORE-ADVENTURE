using UnityEngine;

public class Core : MonoBehaviour
{
    public AbilityType ability = AbilityType.Fire;
    void Start()
    {
        // Optional: set sprite color based on ability
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (ability == AbilityType.Fire) sr.color = new Color(1f, 0.5f, 0.2f);
            else if (ability == AbilityType.Wind) sr.color = new Color(0.5f, 1f, 0.6f);
            else if (ability == AbilityType.Water) sr.color = new Color(0.6f, 0.9f, 1f);
        }
    }

    // We do NOT auto-absorb here (player pressing E picks up).
    // But to support auto-pickup: uncomment OnTriggerEnter2D:
    /*
    void OnTriggerEnter2D(Collider2D other) {
        var p = other.GetComponent<PlayerController>();
        if (p != null) {
            p.AbsorbCore(this);
        }
    }
    */
}
