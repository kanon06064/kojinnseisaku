using UnityEngine;
using GameCore.InventorySystem;
using GameCore.FieldSystem; // FieldItemなどを使うために必要

namespace GameCore.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactRange = 2.0f; // 調べる距離
        [SerializeField] private LayerMask interactLayer;    // 調べる対象のレイヤー（指定がなければDefault等すべて含む）

        private PlayerInputController inputController;
        private PlayerStamina stamina;
        private InventoryManager inventoryManager;

        private void Awake()
        {
            inputController = GetComponent<PlayerInputController>();
            stamina = GetComponent<PlayerStamina>();
            // インベントリ機能を持っている場合のみ取得
            inventoryManager = GetComponent<InventoryManager>();
        }

        private void Start()
        {
            // 入力イベントの登録
            if (inputController != null)
            {
                inputController.OnToolPressed += UseTool;
                inputController.OnTalkPressed += TalkToNPC;
                inputController.OnInteractPressed += InteractObject;
                inputController.OnTabPressed += OpenMenu;
            }

            // インベントリからの通知（使った・捨てた）を受け取る登録
            if (inventoryManager != null)
            {
                // UIで「使う」が押されたら、このクラスの HandleItemUsed を実行する
                inventoryManager.OnItemUsed += HandleItemUsed;
                // UIで「捨てる」が押されたら、このクラスの DropItemToField を実行する
                inventoryManager.OnItemDropped += DropItemToField;
            }
        }

        private void OnDestroy()
        {
            // イベントの登録解除（エラー防止）
            if (inputController != null)
            {
                inputController.OnToolPressed -= UseTool;
                inputController.OnTalkPressed -= TalkToNPC;
                inputController.OnInteractPressed -= InteractObject;
                inputController.OnTabPressed -= OpenMenu;
            }

            if (inventoryManager != null)
            {
                inventoryManager.OnItemUsed -= HandleItemUsed;
                inventoryManager.OnItemDropped -= DropItemToField;
            }
        }

        // ---------------------------------------------------------
        // 1. フィールド上のアクション (Fキー, クリックなど)
        // ---------------------------------------------------------

        /// <summary>
        /// Fキー: 目の前のオブジェクト（アイテム、宝箱、看板など）を調べる
        /// </summary>
        private void InteractObject()
        {
            // プレイヤーの少し前を中心とした球状の判定を作る
            Vector3 center = transform.position + transform.forward * 0.5f;

            // 範囲内のコライダーをすべて検出する
            Collider[] hitColliders = Physics.OverlapSphere(center, interactRange, interactLayer);

            foreach (var hitCollider in hitColliders)
            {
                // 自分自身は無視する
                if (hitCollider.gameObject == gameObject) continue;

                // そのオブジェクトが「IInteractable（調べられるもの）」を持っているか確認
                IInteractable interactable = hitCollider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    // 「インタラクト実行！」と相手に伝える
                    // 相手がアイテムなら「拾う」、看板なら「読む」を勝手に行う
                    interactable.Interact(gameObject);

                    // 1つ調べたら終了（複数のアイテムを同時に拾わないようにする）
                    return;
                }
            }
        }

        private void UseTool()
        {
            // 左クリック: 道具を使う
            // 例: 手に持っている道具のデータを参照して、耕すなどの処理
            if (stamina != null && stamina.ConsumeStamina(3))
            {
                Debug.Log("道具を使用しました！（クワを振るアニメーション再生など）");
            }
        }

        private void TalkToNPC()
        {
            // 右クリック: NPCと話す
            // InteractObjectと同様にOverlapSphereでNPCを探して会話を開始する処理などを記述
            Debug.Log("NPCと話す処理");
        }

        private void OpenMenu()
        {
            // Tabキー: メニューを開く
            // UIManagerなどのメニュー開閉処理を呼び出す
            Debug.Log("メニュー画面の開閉");
        }


        // ---------------------------------------------------------
        // 2. インベントリ連携 (使う・捨てる)
        // ---------------------------------------------------------

        /// <summary>
        /// インベントリで「使う」が実行された時に呼ばれる処理
        /// </summary>
        private void HandleItemUsed(ItemData item)
        {
            // 回復アイテムならスタミナを回復
            if (item.Type == ItemType.Consumable)
            {
                if (stamina != null && item.StaminaRecoverAmount > 0)
                {
                    stamina.RecoverStamina(item.StaminaRecoverAmount);
                    Debug.Log($"{item.ItemName} を使ってスタミナが回復した！");

                    // ※ここで「食べる音」や「パーティクル」を再生するとより良くなります
                }
            }

            // 将来的に「スキル習得書」や「バフアイテム」などもここで分岐して処理できます
        }

        /// <summary>
        /// インベントリで「捨てる」が実行された時に呼ばれる処理
        /// </summary>
        private void DropItemToField(ItemData item, int amount)
        {
            if (item.DropPrefab == null)
            {
                Debug.LogWarning($"{item.ItemName} にはドロップ時のプレハブが設定されていません。");
                return;
            }

            // プレイヤーの足元より少し前・少し上に生成する
            Vector3 dropPosition = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

            // プレハブを実体化（生成）
            GameObject droppedObj = Instantiate(item.DropPrefab, dropPosition, Quaternion.identity);

            // 生成したオブジェクトに「これは〇〇が×個だよ」とデータを教える
            FieldItem fieldItemScript = droppedObj.GetComponent<FieldItem>();
            if (fieldItemScript != null)
            {
                fieldItemScript.Initialize(item, amount);
            }

            Debug.Log($"{item.ItemName} を地面に落としました。");
        }

        // デバッグ用: シーンビューで判定範囲を表示する
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + transform.forward * 0.5f;
            Gizmos.DrawWireSphere(center, interactRange);
        }
    }
}