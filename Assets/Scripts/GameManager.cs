using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int enemiesDefeated = 0;

    //public CharacterData[] characters;
    public GameObject playerPrefab;
    private GameObject playerObj;
    public Transform playerSpawnPoint;

    public PlayerController playerController { get; private set; }

    public PlayerExperience playerExperience;
    public HPIcon hpIconUI;
    public TextMeshProUGUI levelUpText;
    public TextMeshProUGUI gameClearText;
    public TextMeshProUGUI gameOverText;
    public GameObject gameOverPanel;
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

        playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

        playerController = playerObj.GetComponent<PlayerController>();
        var ph = playerObj.GetComponent<PlayerHealth>();

        if (hpIconUI != null && ph != null)
        {
            hpIconUI.InitializeHP(ph);
        }
    }

    void Update()
    {
        if (isGameOver && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Retry();
        }
    }

    public void EnemyDefeated()
    {
        if (!isGameActive) return;

        if (playerExperience != null)
        {
            playerExperience.AddExp(1);
            enemiesDefeated++;
        }
    }

    public IEnumerator FadeOut()
    {
        while (true)
        {
            for (int i = 0; i < 255; i++)
            {
                levelUpText.color = levelUpText.color - new Color32(0, 0, 0, 1);
                yield return new WaitForSeconds(0.001f);
            }
        }
    }

    public void GameClear()
    {
        isGameActive = false;
        isGameOver = true;

        gameClearText.gameObject.SetActive(true);
        Instantiate(clearEffectPrefab, Vector3.zero, Quaternion.identity);
        retryButton.SetActive(true);
        titleButton.SetActive(true);

        clearSound.Play();

        BGMManager.instance.StopAndSwitchBGM(BGMManager.instance.clearBGM);
    }

    public void GameOver()
    {
        isGameActive = false;
        isGameOver = true;

        gameOverSound.Play();

        gameOverText.gameObject.SetActive(true);
        gameOverPanel.gameObject.SetActive(true);
        retryButton.SetActive(true);
        titleButton.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void BackToTitle()
    {
        if (BGMManager.instance != null)
        {
            BGMManager.instance.SwitchToTitleBGM();
        }

        SceneManager.LoadScene("TitleScene");
    }
}
