using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.MonsterSystem;
using GameCore.PartySystem;
using GameCore.SceneManagement;

namespace GameCore.BattleSystem
{
    public enum BattleState
    {
        Intro,
        PlayerTurn,
        EnemyTurn,
        Won,
        Lost
    }

    public class BattleManager : MonoBehaviour
    {
        [Header("System")]
        [SerializeField] private BattleState state;
        [SerializeField] private BattleCameraController cameraController;

        [Header("Spawn Points")]
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform[] playerSpawnPoints;

        [Header("UI References")]
        [SerializeField] private BattleHUD battleHUD;
        [SerializeField] private TextMeshProUGUI enemyNameText;

        [Header("Action Settings")]
        [SerializeField] private float moveSpeed = 10f; // 移動速度
        [SerializeField] private float attackDistance = 1.5f; // 敵の手前で止まる距離

        // データ
        private MonsterSpecies currentEnemySpecies;
        private int currentEnemyHP;
        private int currentEnemyMaxHP;

        // 生成したオブジェクトの参照リスト
        private List<GameObject> activePartyModels = new List<GameObject>();
        // 各キャラクターの初期位置を記憶するリスト
        private List<Vector3> partyOriginalPositions = new List<Vector3>();

        private List<MonsterData> activePartyData;
        private GameObject currentEnemyObject;
        private Vector3 enemyOriginalPosition;

        private PartyManager partyManager;

        public static BattleManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            partyManager = FindAnyObjectByType<PartyManager>();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            StartCoroutine(SetupBattle());
        }

        // --- 戦闘開始フロー ---
        private IEnumerator SetupBattle()
        {
            state = BattleState.Intro;
            battleHUD.ToggleCommandButtons(false);

            // 1. 敵の生成
            currentEnemySpecies = EncounterManager.EncounteredEnemy;
            if (currentEnemySpecies != null)
            {
                currentEnemyObject = Instantiate(currentEnemySpecies.ModelPrefab, enemySpawnPoint);
                currentEnemyObject.transform.localPosition = Vector3.zero;
                currentEnemyObject.transform.localRotation = Quaternion.identity;

                // 敵の初期位置を記憶
                enemyOriginalPosition = currentEnemyObject.transform.position;

                currentEnemyMaxHP = currentEnemySpecies.BaseMaxHP;
                currentEnemyHP = currentEnemyMaxHP;

                if (enemyNameText != null) enemyNameText.text = currentEnemySpecies.SpeciesName;
                battleHUD.SetupEnemyHUD(currentEnemySpecies);
                battleHUD.UpdateEnemyHP(currentEnemyHP, currentEnemyMaxHP);
            }
            else
            {
                battleHUD.SetLogText("敵データなし（デバッグ）");
            }

            // 2. 味方パーティの生成
            SpawnPartyMonsters();

            // 3. カメラ演出
            battleHUD.SetLogText($"{currentEnemySpecies?.SpeciesName} があらわれた！");

            if (cameraController != null)
            {
                yield return StartCoroutine(cameraController.MoveToOverview());
                yield return new WaitForSeconds(1.0f);
                yield return StartCoroutine(cameraController.MoveToCommandView());
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }

            state = BattleState.PlayerTurn;
            PlayerTurn();
        }

        private void SpawnPartyMonsters()
        {
            if (partyManager == null) return;

            activePartyData = partyManager.MainParty;
            battleHUD.SetupPartyHUD(activePartyData);

            int count = Mathf.Min(activePartyData.Count, playerSpawnPoints.Length);
            for (int i = 0; i < count; i++)
            {
                MonsterData data = activePartyData[i];
                if (data != null && data.Species != null)
                {
                    GameObject model = Instantiate(data.Species.ModelPrefab, playerSpawnPoints[i]);
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localRotation = Quaternion.identity;

                    activePartyModels.Add(model);
                    // 初期位置を記憶
                    partyOriginalPositions.Add(model.transform.position);
                }
            }
        }

        private void PlayerTurn()
        {
            battleHUD.SetLogText("どうする？");
            battleHUD.ToggleCommandButtons(true);
        }

        public void OnAttackButton()
        {
            if (state != BattleState.PlayerTurn) return;
            StartCoroutine(PlayerPartyAttackSequence());
        }

        public void OnRunButton()
        {
            if (state != BattleState.PlayerTurn) return;
            StartCoroutine(EndBattle(false));
        }

        // --- ★修正: 味方全員攻撃シーケンス ---
        private IEnumerator PlayerPartyAttackSequence()
        {
            battleHUD.ToggleCommandButtons(false);

            // 味方全員が順番に行動する
            for (int i = 0; i < activePartyModels.Count; i++)
            {
                // 敵がすでに倒れていたら終了
                if (currentEnemyHP <= 0) break;

                GameObject attacker = activePartyModels[i];
                MonsterData attackerData = activePartyData[i];

                // 1. カメラ演出: 攻撃者に寄る
                if (cameraController != null)
                {
                    yield return StartCoroutine(cameraController.FocusOnAttacker(attacker.transform, currentEnemyObject.transform));
                }

                // 2. 移動: 敵の目の前へ
                yield return StartCoroutine(MoveToTarget(attacker.transform, currentEnemyObject.transform.position));

                // 3. 攻撃実行
                battleHUD.SetLogText($"{attackerData.Nickname} の攻撃！");

                // ここで攻撃モーション再生 (Animatorがあれば)
                // attacker.GetComponent<Animator>()?.SetTrigger("Attack");
                yield return new WaitForSeconds(0.3f); // モーションのヒットタイミング待ち

                // 4. ダメージ計算
                int damage = Random.Range(15, 25); // 本来はステータス参照
                currentEnemyHP -= damage;
                if (currentEnemyHP < 0) currentEnemyHP = 0;

                battleHUD.UpdateEnemyHP(currentEnemyHP, currentEnemyMaxHP);
                battleHUD.SetLogText($"{damage} のダメージ！");

                // 少し待機
                yield return new WaitForSeconds(0.5f);

                // 5. 帰還: 元の位置へ戻る
                yield return StartCoroutine(MoveToPosition(attacker.transform, partyOriginalPositions[i]));

                // 6. カメラを戻す（次の人のために一旦戻すか、そのまま次へ行くかはお好みで）
                // ここではテンポよく次へ行くためにカメラは戻さず、ループの頭で次のキャラにフォーカスさせます
            }

            // 全員の攻撃が終わったらカメラを定位置に戻す
            if (cameraController != null)
            {
                yield return StartCoroutine(cameraController.MoveToCommandView());
            }

            // 判定
            if (currentEnemyHP <= 0)
            {
                state = BattleState.Won;
                StartCoroutine(EndBattle(true));
            }
            else
            {
                state = BattleState.EnemyTurn;
                StartCoroutine(EnemyTurn());
            }
        }

        // --- ★修正: 敵の攻撃ターン ---
        private IEnumerator EnemyTurn()
        {
            state = BattleState.EnemyTurn;

            // ターゲット：味方の中からランダム
            int targetIndex = Random.Range(0, activePartyModels.Count);
            GameObject targetObj = activePartyModels[targetIndex];
            MonsterData targetData = activePartyData[targetIndex];
            Vector3 targetPos = targetObj.transform.position;

            // 1. カメラ演出: 敵に寄る
            if (cameraController != null)
            {
                yield return StartCoroutine(cameraController.FocusOnAttacker(currentEnemyObject.transform, targetObj.transform));
            }

            // 2. 移動: ターゲットの目の前へ
            yield return StartCoroutine(MoveToTarget(currentEnemyObject.transform, targetPos));

            battleHUD.SetLogText($"{currentEnemySpecies.SpeciesName} の攻撃！");
            yield return new WaitForSeconds(0.3f);

            // 3. ダメージ処理
            int damage = Random.Range(8, 15);
            targetData.CurrentHP -= damage;
            if (targetData.CurrentHP < 0) targetData.CurrentHP = 0;

            battleHUD.UpdateAllyHP(targetIndex, targetData.CurrentHP);
            battleHUD.SetLogText($"{targetData.Nickname} は {damage} のダメージを受けた！");

            yield return new WaitForSeconds(0.5f);

            // 4. 帰還: 元の位置へ戻る
            yield return StartCoroutine(MoveToPosition(currentEnemyObject.transform, enemyOriginalPosition));

            // 5. カメラを戻す
            if (cameraController != null)
            {
                yield return StartCoroutine(cameraController.MoveToCommandView());
            }

            state = BattleState.PlayerTurn;
            PlayerTurn();
        }

        // --- ★追加: 移動用コルーチン ---

        // ターゲットの手前まで移動する
        private IEnumerator MoveToTarget(Transform mover, Vector3 targetPos)
        {
            // ターゲットへの方向
            Vector3 direction = (targetPos - mover.position).normalized;
            // ターゲット位置から少し手前（AttackDistance分）の座標を計算
            Vector3 destination = targetPos - (direction * attackDistance);

            yield return StartCoroutine(MoveToPosition(mover, destination));
        }

        // 指定座標へ移動する（補間移動）
        private IEnumerator MoveToPosition(Transform mover, Vector3 destination)
        {
            float dist = Vector3.Distance(mover.position, destination);

            // 距離がある間ループ
            while (dist > 0.1f)
            {
                // MoveTowardsで等速移動
                mover.position = Vector3.MoveTowards(mover.position, destination, moveSpeed * Time.deltaTime);
                dist = Vector3.Distance(mover.position, destination);
                yield return null;
            }
            mover.position = destination; // ズレ補正
        }

        private IEnumerator EndBattle(bool isWin)
        {
            if (isWin)
            {
                battleHUD.SetLogText($"{currentEnemySpecies.SpeciesName} を倒した！\n経験値 10 を手に入れた！");
                yield return new WaitForSeconds(3f);
            }
            else
            {
                battleHUD.SetLogText("うまく逃げ切れた！");
                yield return new WaitForSeconds(2f);
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("FieldScene");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("FieldScene");
            }
        }
    }
}