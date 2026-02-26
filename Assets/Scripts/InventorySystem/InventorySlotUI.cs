using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // クリック検知に必須
using TMPro;
using GameCore.InventorySystem;

namespace GameCore.UISystem
{
    // ★追加: IPointerClickHandler を実装する
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")][SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        public int SlotIndex { get; private set; }

        // ★追加: 全体で1つのアクションメニューを参照するための変数
        private ItemActionMenu actionMenu;

        private void Awake()
        {
            // 起動時にシーン内のアクションメニューを探しておく
            actionMenu = FindAnyObjectByType<ItemActionMenu>(FindObjectsInactive.Include);
        }

        public void Initialize(int index)
        {
            SlotIndex = index;
        }

        public void UpdateUI(InventorySlot slotData)
        {
            if (slotData == null || slotData.IsEmpty)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                amountText.text = "";
            }
            else
            {
                iconImage.sprite = slotData.Item.Icon;
                iconImage.enabled = true;
                amountText.text = slotData.Amount > 1 ? slotData.Amount.ToString() : "";
            }
        }

        public void SetGhostEffect(bool isDragging)
        {
            Color color = iconImage.color;
            color.a = isDragging ? 0.5f : 1f;
            iconImage.color = color;
        }

        // --- ★追加: クリックされた瞬間に呼ばれる処理 ---
        public void OnPointerClick(PointerEventData eventData)
        {
            // 右クリックされたかどうかを判定
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Managerからこのマスの最新データをもらう
                InventoryManager manager = FindAnyObjectByType<InventoryManager>();
                InventorySlot myData = manager.GetSlot(SlotIndex);

                // 空っぽなら何もしない
                if (myData == null || myData.IsEmpty) return;

                // アクションメニューを開く！ (位置はマウスカーソルの場所)
                if (actionMenu != null)
                {
                    actionMenu.OpenMenu(SlotIndex, eventData.position, myData.Item);
                }
            }
        }
    }
}