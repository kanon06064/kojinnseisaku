using UnityEngine;
using GameCore.InventorySystem;
using GameCore.UISystem;
using GameCore.FieldSystem;
using GameCore.FarmingSystem;

namespace GameCore.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("判定範囲（球）の大きさ")]
        [SerializeField] private float interactRange = 2.0f;
        [Tooltip("判定の中心をどれくらい前にずらすか（0なら足元、1なら1m前）")]
        [SerializeField] private float detectionOffset = 1.0f; // ★追加: これで前のめりに判定します
        [Tooltip("インタラクト可能なオブジェクトのレイヤー")]
        [SerializeField] private LayerMask interactLayer;

        [Header("Target Cursor")]
        [Tooltip("ターゲットの場所に表示するカーソルのプレハブ")]
        [SerializeField] private GameObject targetCursorPrefab;
        [SerializeField] private Vector3 cursorOffset = new Vector3(0, 0.3f, 0);

        [Header("References")]
        [SerializeField] private InventoryView hotbarView;

        private PlayerInputController inputController;
        private PlayerStamina stamina;
        private InventoryManager inventoryManager;

        private GameObject cursorInstance;
        private IInteractable currentTarget;
        private MonoBehaviour currentTargetObj;

        private void Awake()
        {
            inputController = GetComponent<PlayerInputController>();
            stamina = GetComponent<PlayerStamina>();
            inventoryManager = GetComponent<InventoryManager>();
        }

        private void Start()
        {
            if (inputController != null)
            {
                inputController.OnToolPressed += UseTool;
                inputController.OnTalkPressed += OnRightClickAction;
                inputController.OnInteractPressed += InteractObject;
                inputController.OnTabPressed += OpenMenu;

                inputController.OnPreviousHotbarSlot += SelectPreviousSlot;
                inputController.OnNextHotbarSlot += SelectNextSlot;
            }

            if (inventoryManager != null)
            {
                inventoryManager.OnItemUsed += HandleItemUsed;
                inventoryManager.OnItemDropped += DropItemToField;
            }

            if (targetCursorPrefab != null)
            {
                cursorInstance = Instantiate(targetCursorPrefab);
                cursorInstance.SetActive(false);
            }
        }

        private void Update()
        {
            FindBestTarget();
            UpdateCursorVisual();
        }

        private void OnDestroy()
        {
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
            if (cursorInstance != null) Destroy(cursorInstance);
        }

        // --- ターゲット探索ロジック ---

        private void FindBestTarget()
        {
            // ★修正: 0.5f 固定だったのを detectionOffset 変数に変更
            // これにより、Inspectorで「どれくらい前を判定するか」を調整できます
            Vector3 center = transform.position + transform.forward * detectionOffset;

            Collider[] hitColliders = Physics.OverlapSphere(center, interactRange, interactLayer);

            IInteractable bestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == gameObject) continue;

                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    // 判定の中心（自分の目の前）からの距離で近いものを選ぶ
                    float dist = Vector3.Distance(center, hitCollider.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        bestTarget = interactable;
                    }
                }
            }

            currentTarget = bestTarget;
            currentTargetObj = bestTarget as MonoBehaviour;
        }

        private void UpdateCursorVisual()
        {
            if (cursorInstance == null) return;

            if (currentTargetObj != null)
            {
                if (!cursorInstance.activeSelf) cursorInstance.SetActive(true);

                float bobbing = Mathf.Sin(Time.time * 3f) * 0.1f;
                Vector3 finalPos = currentTargetObj.transform.position + cursorOffset + Vector3.up * bobbing;
                cursorInstance.transform.position = finalPos;
            }
            else
            {
                if (cursorInstance.activeSelf) cursorInstance.SetActive(false);
            }
        }

        // --- アクション ---

        private void InteractObject()
        {
            if (currentTarget != null)
            {
                Debug.Log($"インタラクト実行: {currentTargetObj.name}");
                currentTarget.Interact(gameObject);
            }
        }

        // --- その他メソッド ---

        private void UseTool()
        {
            if (hotbarView == null || inventoryManager == null) return;

            int dataIndex = hotbarView.GetSelectedDataIndex();
            InventorySlot slot = inventoryManager.GetSlot(dataIndex);

            if (slot == null || slot.IsEmpty || slot.Item.ToolType == ItemToolType.None) return;

            if (currentTargetObj != null)
            {
                FarmPlot plot = currentTargetObj.GetComponent<FarmPlot>();
                if (plot != null)
                {
                    if (plot.TryInteract(slot.Item))
                    {
                        if (stamina != null) stamina.ConsumeStamina(2);
                        if (slot.Item.ToolType == ItemToolType.Seed) inventoryManager.RemoveItemAt(dataIndex, 1);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            // デバッグ表示も同じ位置になるように修正
            Vector3 center = transform.position + transform.forward * detectionOffset;
            Gizmos.DrawWireSphere(center, interactRange);
        }

        // (以前と同じ中身のメソッド群)
        private void SelectPreviousSlot() { if (hotbarView != null) { int current = hotbarView.SelectedIndex; int count = 10; int next = (current - 1 + count) % count; hotbarView.SelectSlot(next); } }
        private void SelectNextSlot() { if (hotbarView != null) { int current = hotbarView.SelectedIndex; int count = 10; int next = (current + 1) % count; hotbarView.SelectSlot(next); } }
        private void OnRightClickAction()
        {
            if (hotbarView != null && inventoryManager != null)
            {
                int dataIndex = hotbarView.GetSelectedDataIndex();
                InventorySlot slot = inventoryManager.GetSlot(dataIndex);
                if (slot != null && !slot.IsEmpty && slot.Item.Type == ItemType.Consumable)
                {
                    inventoryManager.UseItemAt(dataIndex);
                    return;
                }
            }
            TalkToNPC();
        }
        private void TalkToNPC()
        {
            if (currentTarget != null)
            {
                Debug.Log($"対象にアクション: {currentTargetObj.name}");
                currentTarget.Interact(gameObject);
            }
            else
            {
                Debug.Log("目の前に話せる相手はいません");
            }
        }
        private void OpenMenu() { Debug.Log("メニュー操作"); }
        private void HandleItemUsed(ItemData item) { if (item.Type == ItemType.Consumable && stamina != null) stamina.RecoverStamina(item.StaminaRecoverAmount); }
        private void DropItemToField(ItemData item, int amount) { if (item.DropPrefab == null) return; Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f; GameObject obj = Instantiate(item.DropPrefab, dropPos, Quaternion.identity); obj.GetComponent<FieldItem>()?.Initialize(item, amount); }
    }
}