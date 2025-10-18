using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifeTime = 0.45f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
