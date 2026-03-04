using System.Collections.Generic; // Listを使うため追加
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCore.MonsterSystem;
using GameCore.SceneManagement;

namespace GameCore.BattleSystem
{
    public class EncounterManager : MonoBehaviour
    {
        public static MonsterSpecies EncounteredEnemy { get; private set; }
        public static EncounterManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string fieldSceneName = "FieldScene";

        // 位置保存用
        private Vector3 lastFieldPosition;
        private Quaternion lastFieldRotation;
        private bool isReturningFromBattle = false;

        // --- ★追加: 倒した（接触した）敵のIDリスト ---
        // 簡易的に「初期座標の文字列」をIDとして扱います
        private List<string> defeatedEnemyIds = new List<string>();

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
        }

        private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        // --- ★追加: 敵を登録するメソッド ---
        public void RegisterDefeatedEnemy(Vector3 enemyStartPosition)
        {
            string id = enemyStartPosition.ToString();
            if (!defeatedEnemyIds.Contains(id))
            {
                defeatedEnemyIds.Add(id);
            }
        }

        // --- ★追加: 既に倒されているか確認するメソッド ---
        public bool IsEnemyDefeated(Vector3 enemyStartPosition)
        {
            string id = enemyStartPosition.ToString();
            return defeatedEnemyIds.Contains(id);
        }

        public void StartBattle(MonsterSpecies enemy)
        {
            if (enemy == null) return;

            EncounteredEnemy = enemy;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                lastFieldPosition = player.transform.position;
                lastFieldRotation = player.transform.rotation;
                isReturningFromBattle = true;
            }

            // シーン遷移
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(battleSceneName);
            else SceneManager.LoadScene(battleSceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == fieldSceneName && isReturningFromBattle)
            {
                RestorePlayerPosition();
                isReturningFromBattle = false;
            }
        }

        private void RestorePlayerPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            player.transform.position = lastFieldPosition;
            player.transform.rotation = lastFieldRotation;

            if (controller != null) controller.enabled = true;
        }
    }
}