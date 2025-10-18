using UnityEngine;
using TMPro; // TextMeshProを使うため

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance;

    public UnityEngine.UI.Image expMeterImage;

    public TextMeshProUGUI levelText;

    public GameObject expMeterAll;
    public GameObject levelUPEffectPrefab;

    private const int MAX_LEVEL = 3;
    private const int EXP_PER_LEVEL = 30;

    public AudioSource levelUPSound;

    public int currentLevel { get; private set; } = 1;
    public int currentExp { get; private set; } = 0; // 現在の経験値（メーターのメモリ）

    void Awake()
    {
        // シングルトンとしての初期化処理
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // シーンに複数のインスタンスがある場合は、自身を破棄
            Destroy(gameObject);
        }
    }

    public void AddExp(int amount)
    {
        if (currentLevel >= MAX_LEVEL) return;

        currentExp += amount;

        while (currentExp >= EXP_PER_LEVEL)
        {
            if (currentLevel < MAX_LEVEL)
            {
                LevelUp();
                currentExp -= EXP_PER_LEVEL; // メーターをリセットして余剰分を保持
            }
            else
            {
                currentExp = EXP_PER_LEVEL; // MAXレベルなら経験値メーターをMaxで固定
                break;
            }
        }

        UpdateUI();
    }

    void LevelUp()
    {
        currentLevel++;
        AudioSource.PlayClipAtPoint(levelUPSound.clip, new Vector3(0, 0, -9), 0.4f);
        Instantiate(levelUPEffectPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log($"プレイヤーがLv{currentLevel}にレベルアップしました！");

        GameManager.Instance.levelUpText.gameObject.SetActive(true);
        GameManager.Instance.levelUpText.color = new Color32(255, 255, 255, 255);
        GameManager.Instance.StartCoroutine(GameManager.Instance.FadeOut());

        // レベルごとの強化
        if (currentLevel == 2)
        {
            Debug.Log("LevelUP! currentLevel:" + currentLevel);
        }
        else if (currentLevel == 3)
        {
            Debug.Log("LevelUP! currentLevel:" + currentLevel);
        }
    }

    void UpdateUI()
    {
        if (expMeterImage != null)
        {
            float fillAmount = (float)currentExp / EXP_PER_LEVEL;
            expMeterImage.fillAmount = fillAmount;
        }

        if (levelText != null)
        {
            if (currentLevel >= MAX_LEVEL)
            {
                levelText.text = "Lv. MAX"; // Lv3以上でMax表示
                expMeterAll.SetActive(false);
            }
            else
            {
                levelText.text = $"Lv. {currentLevel}";
            }
        }
    }
}