using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.MonsterSystem; // 敵データ用

namespace GameCore.BattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Enemy Area")]
        [SerializeField] private Transform enemySpawnPoint; // 敵モデルを出す場所
        [SerializeField] private TextMeshProUGUI enemyNameText; // 敵の名前表示

        private void Start()
        {
            // EncounterManagerから「接触した敵のデータ」をもらう
            MonsterSpecies enemyData = EncounterManager.EncounteredEnemy;

            if (enemyData != null)
            {
                SetupBattle(enemyData);
            }
            else
            {
                Debug.LogWarning("デバッグ起動: 敵データがありません。");
            }
        }

        private void SetupBattle(MonsterSpecies enemy)
        {
            // 名前を表示
            if (enemyNameText != null)
            {
                enemyNameText.text = enemy.SpeciesName;
            }

            // 敵の3Dモデルを生成
            if (enemy.ModelPrefab != null && enemySpawnPoint != null)
            {
                GameObject model = Instantiate(enemy.ModelPrefab, enemySpawnPoint);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
            }

            Debug.Log($"{enemy.SpeciesName} があらわれた！");
        }
    }
}