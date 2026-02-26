using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameCore.InventorySystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// アイテムスロットのドラッグ＆ドロップ操作を検知し、
    /// ゴーストの表示と、InventoryManagerへの入れ替え依頼を行うクラス。
    /// </summary>
    [RequireComponent(typeof(InventorySlotUI))]
    public class InventoryDragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private InventorySlotUI mySlotUI;
        private InventoryManager inventoryManager;

        // ゲーム全体で1つだけ保持する情報（static）
        private static InventorySlotUI currentlyDraggingSlot;
        private static GameObject dragGhostObject; // マウスに追従する分身

        private void Awake()
        {
            mySlotUI = GetComponent<InventorySlotUI>();
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 自分のマスが空ならドラッグを開始しない
            InventorySlot mySlotData = inventoryManager.GetSlot(mySlotUI.SlotIndex);
            if (mySlotData == null || mySlotData.IsEmpty) return;

            // 自分がドラッグされていることを全体に知らせる
            currentlyDraggingSlot = mySlotUI;

            // 元のアイコンを半透明にする
            mySlotUI.SetGhostEffect(true);

            // --- ゴースト（分身）の生成 ---
            dragGhostObject = new GameObject("DragGhost");

            // キャンバスを見つけて、ゴーストを一番手前に表示する
            Canvas canvas = GetComponentInParent<Canvas>();
            dragGhostObject.transform.SetParent(canvas.transform, false);
            dragGhostObject.transform.SetAsLastSibling(); // UIは一番下（Last）にあるほど手前に描画される

            // アイコン画像をセット
            Image ghostImage = dragGhostObject.AddComponent<Image>();
            ghostImage.sprite = mySlotData.Item.Icon;

            // ★超重要: ゴーストがマウスのクリック判定を吸い込まないようにする
            ghostImage.raycastTarget = false;

            // 初期位置をマウスカーソルに合わせる
            dragGhostObject.transform.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentlyDraggingSlot != mySlotUI || dragGhostObject == null) return;

            // ドラッグ中、ゴーストの位置を常にマウス（ポインター）の位置に追従させる
            dragGhostObject.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (currentlyDraggingSlot == mySlotUI)
            {
                // 元のアイコンの半透明を元に戻す
                mySlotUI.SetGhostEffect(false);

                // ドラッグ状態をリセット
                currentlyDraggingSlot = null;

                // ゴーストを消滅させる
                if (dragGhostObject != null)
                {
                    Destroy(dragGhostObject);
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            // 誰かがドラッグ中で、かつ自分自身へのドロップではない場合
            if (currentlyDraggingSlot != null && currentlyDraggingSlot != mySlotUI)
            {
                int fromIndex = currentlyDraggingSlot.SlotIndex; // 持ってきた元の番号
                int toIndex = mySlotUI.SlotIndex;                // ドロップされた自分の番号

                // Managerに入れ替え（または結合）を命令する！
                inventoryManager.SwapOrMergeSlots(fromIndex, toIndex);
            }
        }
    }
}