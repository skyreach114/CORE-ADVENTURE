using System.Collections.Generic;
using UnityEngine;

public class HPIcon : MonoBehaviour
{
    public GameObject hpIconPrefab;

    private PlayerHealth playerHealth;
    private int currentHP;
    private List<GameObject> hpIconList = new List<GameObject>();

    public void InitializeHP(PlayerHealth health)
    {
        playerHealth = health;

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthの参照がnullです。HPアイコンの初期化ができません。");
            return;
        }

        currentHP = playerHealth.GetHP();
        CreateHPIcon();
    }

    private void CreateHPIcon()
    {
        for (int i = 0; i < playerHealth.maxHP; i++)
        {
            GameObject icon = Instantiate(hpIconPrefab, transform);
            hpIconList.Add(icon);
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        ShowHPIcon();
    }

    public void ForceUpdateIcon()
    {
        ShowHPIcon();
    }

    private void ShowHPIcon()
    {
        if (currentHP == playerHealth.GetHP()) return;

        for (int i = 0; i < hpIconList.Count; i++)
        {
            hpIconList[i].SetActive(i < playerHealth.GetHP());
        }
        currentHP = playerHealth.GetHP();
    }
}
