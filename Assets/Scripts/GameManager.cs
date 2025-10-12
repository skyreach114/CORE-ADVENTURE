using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //private int enemiesDefeated = 0;

    //public CharacterData[] characters;
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    //public PlayerExperience playerExperience;
    public HPIcon hpIconUI;
    public TextMeshProUGUI levelUpText;
    public TextMeshProUGUI gameStartText;
    public TextMeshProUGUI gameClearText;
    public TextMeshProUGUI gameOverText;
    public GameObject retryButton;
    public GameObject titleButton;
    public GameObject clearEffectPrefab;

    public AudioSource bossSpawnSound;
    public AudioSource gameOverSound;
    public AudioSource clearSound;

    public bool isGameActive = false;
    public bool isBossSpawn = false;
    public bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isGameActive = true;

        var playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

        var ph = playerObj.GetComponent<PlayerHealth>();

        if (hpIconUI != null && ph != null)
        {
            hpIconUI.InitializeHP(ph);
        }
    }

    void Update()
    {
        
    }

    public void GameOver()
    {

    }

    public void GameClear()
    {

    }
}
