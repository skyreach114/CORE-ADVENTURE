using UnityEngine;
using System.Collections;

public class EnemyCore : MonoBehaviour
{
    // === Inspector設定項目 ===
    [Header("Core Drop Settings")]
    public GameObject coreToDropPrefab; // ドロップするコアのPrefab
    public AbilityType dropType = AbilityType.Fire; // ドロップするコアのタイプ

    // 死亡時に外部（EnemyHealth）から呼ばれる
    public void DropCore()
    {
        if (coreToDropPrefab != null)
        {
            var core = Instantiate(coreToDropPrefab, transform.position, Quaternion.identity);

            // Coreのコンポーネントにドロップタイプを設定
            var coreComponent = core.GetComponent<Core>();
            if (coreComponent != null)
            {
                coreComponent.ability = dropType;
            }

            // タグの設定
            switch (dropType)
            {
                case AbilityType.Fire: core.tag = "Core_Fire"; break;
                case AbilityType.Wind: core.tag = "Core_Wind"; break;
                case AbilityType.Water: core.tag = "Core_Water"; break;
            }
        }
        // ここでDestroy(gameObject); は呼ばず、EnemyHealth側で遅延破棄させる
    }
}