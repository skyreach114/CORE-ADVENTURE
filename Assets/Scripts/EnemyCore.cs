using UnityEngine;
using System.Collections;

public class EnemyCore : MonoBehaviour
{
    // === Inspector�ݒ荀�� ===
    [Header("Core Drop Settings")]
    public GameObject coreToDropPrefab; // �h���b�v����R�A��Prefab
    public AbilityType dropType = AbilityType.Fire; // �h���b�v����R�A�̃^�C�v

    // ���S���ɊO���iEnemyHealth�j����Ă΂��
    public void DropCore()
    {
        if (coreToDropPrefab != null)
        {
            var core = Instantiate(coreToDropPrefab, transform.position, Quaternion.identity);

            // Core�̃R���|�[�l���g�Ƀh���b�v�^�C�v��ݒ�
            var coreComponent = core.GetComponent<Core>();
            if (coreComponent != null)
            {
                coreComponent.ability = dropType;
            }

            // �^�O�̐ݒ�
            switch (dropType)
            {
                case AbilityType.Fire: core.tag = "Core_Fire"; break;
                case AbilityType.Wind: core.tag = "Core_Wind"; break;
                case AbilityType.Water: core.tag = "Core_Water"; break;
            }
        }
        // ������Destroy(gameObject); �͌Ă΂��AEnemyHealth���Œx���j��������
    }
}