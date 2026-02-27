using UnityEngine;
using GameCore.InventorySystem;
using GameCore.UISystem; // InventoryViewを使うために必要
using GameCore.FieldSystem; // FieldItemを使うために必要

namespace GameCore.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Fキーや右クリックで届く距離")]
        [SerializeField] private float interactRange = 2.0f;
        [Tooltip("インタラクト可能なオブジェクトのレイヤー")]
        [SerializeField] private LayerMask interactLayer;

        [Header("References")]
        [Tooltip("画面下のホットバーUI (InventoryView) をここにドラッグ＆ドロップしてください")]
        [SerializeField] private InventoryView hotbarView;

        private PlayerInputController inputController;
        private PlayerStamina stamina;
        private InventoryManager inventoryManager;

        private void Awake()
        {
            inputController = GetComponent<PlayerInputController>();
            stamina = GetComponent<PlayerStamina>();
            inventoryManager = GetComponent<InventoryManager>();
        }

        private void Start()
        {
            // 入力イベントの登録
            if (inputController != null)
            {
                inputController.OnToolPressed += UseTool;            // 左クリック
                inputController.OnTalkPressed += OnRightClickAction; // 右クリック (会話 or アイテム使用)
                inputController.OnInteractPressed += InteractObject; // Fキー
                inputController.OnTabPressed += OpenMenu;            // Tabキー

                // ホットバー切り替え (Q / E)
                inputController.OnPreviousHotbarSlot += SelectPreviousSlot;
                inputController.OnNextHotbarSlot += SelectNextSlot;
            }

            // インベントリからの通知（メニュー画面やホットバーで「使った」「捨てた」時用）を受け取る登録
            if (inventoryManager != null)
            {
                inventoryManager.OnItemUsed += HandleItemUsed;
                inventoryManager.OnItemDropped += DropItemToField;
            }
        }

        private void OnDestroy()
        {
            // イベントの登録解除（エラー防止）
            if (inputController != null)
            {
                inputController.OnToolPressed -= UseTool;
                inputController.OnTalkPressed -= OnRightClickAction;
                inputController.OnInteractPressed -= InteractObject;
                inputController.OnTabPressed -= OpenMenu;
                inputController.OnPreviousHotbarSlot -= SelectPreviousSlot;
                inputController.OnNextHotbarSlot -= SelectNextSlot;
            }

            if (inventoryManager != null)
            {
                inventoryManager.OnItemUsed -= HandleItemUsed;
                inventoryManager.OnItemDropped -= DropItemToField;
            }
        }

        // ---------------------------------------------------------
        // 1. ホットバー操作 & 右クリック分岐
        // ---------------------------------------------------------

        private void SelectPreviousSlot()
        {
            if (hotbarView != null)
            {
                int current = hotbarView.SelectedIndex;
                int count = 10; // ホットバーのスロット数（通常10）
                // 1つ戻る（0より小さくなったら9に戻る計算）
                int next = (current - 1 + count) % count;
                hotbarView.SelectSlot(next);
            }
        }

        private void SelectNextSlot()
        {
            if (hotbarView != null)
            {
                int current = hotbarView.SelectedIndex;
                int count = 10;
                // 1つ進む（9を超えたら0に戻る計算）
                int next = (current + 1) % count;
                hotbarView.SelectSlot(next);
            }
        }

        /// <summary>
        /// 右クリック時のアクション分岐
        /// 1. 手に持っている(ホットバーで選択中の)アイテムが「消費アイテム」なら使う
        /// 2. それ以外なら、目の前のNPCやオブジェクトに対して「会話/調べる」を行う
        /// </summary>
        private void OnRightClickAction()
        {
            // 1. アイテム使用の判定
            if (hotbarView != null && inventoryManager != null)
            {
                // UIで選択中のスロット番号を取得（ホットバーの0~9 + 開始インデックス）
                int dataIndex = hotbarView.GetSelectedDataIndex();

                // データの取得
                InventorySlot slot = inventoryManager.GetSlot(dataIndex);

                // アイテムを持っていて、かつ「消費アイテム(Consumable)」なら
                if (slot != null && !slot.IsEmpty && slot.Item.Type == ItemType.Consumable)
                {
                    Debug.Log($"ホットバーのアイテムを使用します: {slot.Item.ItemName}");
                    // アイテムを使う処理を呼び出す（成功すればHandleItemUsedも自動で呼ばれる）
                    inventoryManager.UseItemAt(dataIndex);
                    return; // アイテムを使ったら、会話処理は行わずに終了
                }
            }

            // 2. 会話・調べる判定 (消費アイテムを使わなかった場合)
            TalkToNPC();
        }

        // ---------------------------------------------------------
        // 2. フィールド上のアクション
        // ---------------------------------------------------------

        private void InteractObject()
        {
            // Fキー: 目の前のオブジェクト（アイテム、宝箱、看板など）を調べる
            Vector3 center = transform.position + transform.forward * 0.5f;
            Collider[] hitColliders = Physics.OverlapSphere(center, interactRange, interactLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == gameObject) continue;

                IInteractable interactable = hitCollider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(gameObject);
                    return; // 1つ調べたら終了
                }
            }
        }

        private void UseTool()
        {
            // 左クリック: 道具を使う
            // 例: 手に持っているのがクワなら耕す、剣なら攻撃するなど
            // ここでは簡易的にスタミナ消費のみ実装
            if (stamina != null && stamina.ConsumeStamina(3))
            {
                Debug.Log("道具を使用しました！（アニメーション再生など）");
                // TODO: ここにRaycastなどで畑を耕す処理を追加
            }
        }

        private void TalkToNPC()
        {
            // 右クリック（アイテム未使用時）: NPCと話す
            Vector3 center = transform.position + transform.forward * 0.5f;
            Collider[] hitColliders = Physics.OverlapSphere(center, interactRange, interactLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == gameObject) continue;

                // 対象がIInteractableを持っているなら実行する
                // (NPC専用のコンポーネントがある場合はそれを取得しても良い)
                IInteractable interactable = hitCollider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"対象にアクション: {hitCollider.name}");
                    interactable.Interact(gameObject);
                    return;
                }
            }
            Debug.Log("目の前に話せる相手はいません");
        }

        private void OpenMenu()
        {
            // Tabキー: メニュー操作
            // 実際の開閉処理はUIManagerがInputControllerを監視して行っている場合が多いですが、
            // ここでログを出したりSEを鳴らしたりできます。
            Debug.Log("メニュー操作");
        }

        // ---------------------------------------------------------
        // 3. インベントリ連携 (通知受け取り)
        // ---------------------------------------------------------

        /// <summary>
        /// インベントリで「使う」が実行された時に呼ばれる（ホットバー経由含む）
        /// </summary>
        private void HandleItemUsed(ItemData item)
        {
            // 回復アイテムならスタミナを回復
            if (item.Type == ItemType.Consumable)
            {
                if (stamina != null && item.StaminaRecoverAmount > 0)
                {
                    stamina.RecoverStamina(item.StaminaRecoverAmount);
                    // 音やエフェクトの再生
                }
            }
        }

        /// <summary>
        /// インベントリで「捨てる」が実行された時に呼ばれる
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
            GameObject droppedObj = Instantiate(item.DropPrefab, dropPosition, Quaternion.identity);

            // 生成したオブジェクトに「これは〇〇が×個だよ」とデータを教える
            FieldItem fieldItemScript = droppedObj.GetComponent<FieldItem>();
            if (fieldItemScript != null)
            {
                fieldItemScript.Initialize(item, amount);
            }

            Debug.Log($"{item.ItemName} を地面に落としました。");
        }

        // デバッグ用表示
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + transform.forward * 0.5f;
            Gizmos.DrawWireSphere(center, interactRange);
        }
    }
}