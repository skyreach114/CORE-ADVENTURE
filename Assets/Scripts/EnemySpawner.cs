using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    private Transform player;
    private bool hasSpawned = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null || hasSpawned) return;

        Vector3 playerPos = player.position;
        Vector3 cameraMaxPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        Vector3 scale = enemyPrefab.transform.lossyScale;

        float distance = Vector2.Distance(transform.position, new Vector2(playerPos.x, transform.position.y));
        float spawnDis = Vector2.Distance(playerPos, new Vector2(cameraMaxPos.x + scale.x / 2f, playerPos.y));

        if (distance <= spawnDis)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            hasSpawned = true;
            Destroy(gameObject);
        }
    }
}