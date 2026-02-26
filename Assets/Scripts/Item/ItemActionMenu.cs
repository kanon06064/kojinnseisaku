using UnityEngine;
using UnityEngine.UI;
using GameCore.InventorySystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// アイテムを右クリックした時に出る「使う」「捨てる」のサブメニュー。
    /// 画面に1つだけ存在し、クリックされた場所に移動して使い回されます。
    /// </summary>
    public class ItemActionMenu : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private InventoryManager inventoryManager; [Header("UI Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button cancelButton;

        private int currentSlotIndex = -1; // 現在操作中のスロット番号

        private void Awake()
        {
            // ボタンが押された時の処理を登録
            useButton.onClick.AddListener(OnUseClicked);
            dropButton.onClick.AddListener(OnDropClicked);
            cancelButton.onClick.AddListener(CloseMenu);

            // 最初は非表示にしておく
            gameObject.SetActive(false);
        }

        /// <summary>
        /// メニューを指定した位置に開く
        /// </summary>
        public void OpenMenu(int slotIndex, Vector2 position, ItemData itemData)
        {
            currentSlotIndex = slotIndex;

            // マウスの位置にメニューを移動させる
            transform.position = position;

            // 消費アイテムでなければ「使う」ボタンを押せなくする
            useButton.interactable = (itemData.Type == ItemType.Consumable);

            // 一番手前に表示してアクティブにする
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            gameObject.SetActive(false);
            currentSlotIndex = -1;
        }

        private void OnUseClicked()
        {
            if (currentSlotIndex != -1)
            {
                inventoryManager.UseItemAt(currentSlotIndex);
            }
            CloseMenu();
        }

        private void OnDropClicked()
        {
            if (currentSlotIndex != -1)
            {
                inventoryManager.DropItemAt(currentSlotIndex);
            }
            CloseMenu();
        }
    }
}