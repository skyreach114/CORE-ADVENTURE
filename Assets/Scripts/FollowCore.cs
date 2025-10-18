using UnityEngine;

public class FollowCore : MonoBehaviour
{
    public Transform player;
    public AbilityType ability;
    public float followSpeed = 3f;
    public Vector3 offset = new Vector3(-1f, 0.5f, 0f);

    SpriteRenderer sr;
    public void Init(Transform playerTransform, AbilityType a)
    {
        player = playerTransform;
        ability = a;
        sr = GetComponent<SpriteRenderer>();
        ApplyColorByAbility();
    }

    void Update()
    {
        if (player == null) return;
        Vector3 target = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSpeed);
    }

    void ApplyColorByAbility()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        if (ability == AbilityType.Fire) sr.color = new Color(1f, 0.4f, 0.1f);
        else if (ability == AbilityType.Wind) sr.color = new Color(0.4f, 1f, 0.6f);
        else if (ability == AbilityType.Water) sr.color = new Color(0.6f, 0.9f, 1f);
    }
}
