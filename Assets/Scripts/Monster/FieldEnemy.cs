using UnityEngine;
using GameCore.MonsterSystem; // MonsterSpeciesを使うため

namespace GameCore.BattleSystem
{
    public enum EnemyState
    {
        Idle,   // 待機
        Wander, // 徘徊
        Chase   // 追跡
    }

    [RequireComponent(typeof(CharacterController))]
    public class FieldEnemy : MonoBehaviour
    {
        [Header("Enemy Data")]
        public MonsterSpecies Species; // 敵の種類（スライム等）
        [SerializeField] private int level = 1;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3.0f;       // 歩く速度
        [SerializeField] private float chaseSpeed = 5.0f;      // 追いかける速度
        [SerializeField] private float wanderRadius = 10.0f;   // 徘徊する範囲
        [SerializeField] private float detectionRadius = 8.0f; // プレイヤーに気づく距離

        [Header("Wait Settings")]
        [SerializeField] private float minWaitTime = 1.0f;
        [SerializeField] private float maxWaitTime = 3.0f;

        // 内部変数
        private CharacterController controller;
        private Transform playerTransform;
        private Vector3 startPosition; // 縄張りの中心
        private Vector3 targetPosition; // 次の目的地
        private EnemyState currentState = EnemyState.Idle;
        private float waitTimer = 0f;

        // 重力計算用
        private float verticalVelocity = 0f;
        private const float Gravity = -9.81f;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            startPosition = transform.position;

            // プレイヤーをタグで探す
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            // 最初の目的地を決める
            SetNewWanderTarget();
        }

        private void Update()
        {
            // --- 1. 重力の計算 ---
            if (controller.isGrounded)
            {
                verticalVelocity = -2f; // 地面に押し付ける
            }
            else
            {
                verticalVelocity += Gravity * Time.deltaTime;
            }

            // --- 2. 状態遷移の判断 ---
            float distToPlayer = float.MaxValue;
            if (playerTransform != null)
            {
                distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            }

            // プレイヤーが近いなら「追跡」、遠ければ「徘徊」に戻る
            if (distToPlayer < detectionRadius)
            {
                currentState = EnemyState.Chase;
            }
            else if (currentState == EnemyState.Chase && distToPlayer > detectionRadius * 1.5f)
            {
                // 一度追いかけたら、少し離れるまで追いかける（1.5倍の距離で諦める）
                currentState = EnemyState.Wander;
                SetNewWanderTarget();
            }

            // --- 3. 行動の実行 ---
            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdle();
                    break;
                case EnemyState.Wander:
                    HandleWander();
                    break;
                case EnemyState.Chase:
                    HandleChase();
                    break;
            }
        }

        // 待機中の処理
        private void HandleIdle()
        {
            waitTimer -= Time.deltaTime;

            // 待機時間が終わったら、次の目的地を決めて歩き出す
            if (waitTimer <= 0)
            {
                SetNewWanderTarget();
                currentState = EnemyState.Wander;
            }

            // 待機中も重力だけは適用して落下させる
            Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
            controller.Move(gravityMove * Time.deltaTime);
        }

        // 徘徊中の移動
        private void HandleWander()
        {
            MoveTowards(targetPosition, moveSpeed);

            // 目的地に近づいたら（水平距離で判定）
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                 new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.5f)
            {
                // 到着したので待機モードへ
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
                currentState = EnemyState.Idle;
            }
        }

        // 追跡中の移動
        private void HandleChase()
        {
            if (playerTransform != null)
            {
                MoveTowards(playerTransform.position, chaseSpeed);
            }
        }

        // 実際にキャラクターを動かす共通メソッド
        private void MoveTowards(Vector3 target, float speed)
        {
            // 向きの計算（Y軸の差は無視して水平方向のみ）
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            // 移動入力があるなら回転させる
            if (direction != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
            }

            // 移動量の計算（速度 + 重力）
            Vector3 velocity = direction * speed;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);
        }

        // ランダムな徘徊先を決める
        private void SetNewWanderTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomPoint.x, 0, randomPoint.y);
        }

        // --- ★重要: 接触判定 (Trigger) ---
        // 敵のTriggerコライダーに何かが入った時に呼ばれる
        private void OnTriggerEnter(Collider other)
        {
            // ぶつかってきた相手がプレイヤーか？
            if (other.CompareTag("Player"))
            {
                Debug.Log($"敵({this.name})がプレイヤーを捕まえました！ 戦闘開始！");

                if (EncounterManager.Instance != null)
                {
                    // 戦闘開始処理を呼び出す
                    EncounterManager.Instance.StartBattle(this.Species);

                    // エンカウントしたら、このシンボルは消す
                    Destroy(gameObject);
                }
            }
        }

        // デバッグ表示（シーンビューで範囲が見えるように）
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius); // 検知範囲
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(startPosition, wanderRadius); // 徘徊範囲
        }
    }
}