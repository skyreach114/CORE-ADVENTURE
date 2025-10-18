using UnityEngine;
using TMPro; // TextMeshPro���g������

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
    public int currentExp { get; private set; } = 0; // ���݂̌o���l�i���[�^�[�̃������j

    void Awake()
    {
        // �V���O���g���Ƃ��Ă̏���������
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // �V�[���ɕ����̃C���X�^���X������ꍇ�́A���g��j��
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
                currentExp -= EXP_PER_LEVEL; // ���[�^�[�����Z�b�g���ė]�蕪��ێ�
            }
            else
            {
                currentExp = EXP_PER_LEVEL; // MAX���x���Ȃ�o���l���[�^�[��Max�ŌŒ�
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
        Debug.Log($"�v���C���[��Lv{currentLevel}�Ƀ��x���A�b�v���܂����I");

        GameManager.Instance.levelUpText.gameObject.SetActive(true);
        GameManager.Instance.levelUpText.color = new Color32(255, 255, 255, 255);
        GameManager.Instance.StartCoroutine(GameManager.Instance.FadeOut());

        // ���x�����Ƃ̋���
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
                levelText.text = "Lv. MAX"; // Lv3�ȏ��Max�\��
                expMeterAll.SetActive(false);
            }
            else
            {
                levelText.text = $"Lv. {currentLevel}";
            }
        }
    }
}