using UnityEngine;
using GameCore.MonsterSystem;
using GameCore.SceneManagement; // ★修正: GameCore.System から変更

namespace GameCore.BattleSystem
{
    public class EncounterManager : MonoBehaviour
    {
        public static MonsterSpecies EncounteredEnemy { get; private set; }

        [Header("Settings")]
        [Tooltip("遷移先の戦闘シーン名")]
        [SerializeField] private string battleSceneName = "BattleScene";

        public static EncounterManager Instance { get; private set; }

        private void Awake()
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

        public void StartBattle(MonsterSpecies enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("エラー: 敵のデータ(MonsterSpecies)がありません！");
                return;
            }

            EncounteredEnemy = enemy;

            Debug.Log($"<color=red>戦闘開始！</color> 相手: {enemy.SpeciesName} (Lv.{enemy.BaseMaxHP}相当)");

            // ★修正: 新しい名前空間のSceneLoaderを使用
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(battleSceneName);
            }
            else
            {
                Debug.LogWarning("SceneLoaderがいません！BootSceneからゲームを開始してください。");
                UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
            }
        }
    }
}