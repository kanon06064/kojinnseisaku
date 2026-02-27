using System.Collections; // これが必要です
using UnityEngine;
using UnityEngine.UI;
using GameCore.InventorySystem;

namespace GameCore.UISystem
{
    public class InventoryView : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private InventoryManager inventoryManager;

        [Header("UI Settings")]
        [SerializeField] private InventorySlotUI[] slotUIs;
        [SerializeField] private int startIndex = 0;

        [Header("Selection (Hotbar Only)")]
        [SerializeField] private bool isHotbar = false;
        [SerializeField] private RectTransform selectionFrame;

        public int SelectedIndex { get; private set; } = 0;

        // ★修正ポイント: IEnumeratorに変更して待機処理を追加
        private IEnumerator Start()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnSlotUpdated += HandleSlotUpdated;
                RefreshAllSlots();
            }

            // レイアウト整列が終わるまで1フレーム待つ
            yield return null;

            if (isHotbar && selectionFrame != null)
            {
                SelectSlot(0);
            }
        }

        private void OnDestroy()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnSlotUpdated -= HandleSlotUpdated;
            }
        }

        private void HandleSlotUpdated(int dataIndex, InventorySlot slotData)
        {
            if (dataIndex >= startIndex && dataIndex < startIndex + slotUIs.Length)
            {
                int uiIndex = dataIndex - startIndex;
                slotUIs[uiIndex].UpdateUI(slotData);
            }
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                int dataIndex = startIndex + i;
                if (dataIndex < slotUIs.Length) // 安全策
                    slotUIs[i].Initialize(dataIndex);

                InventorySlot data = inventoryManager.GetSlot(dataIndex);
                slotUIs[i].UpdateUI(data);
            }
        }

        public void SelectSlot(int index)
        {
            if (!isHotbar) return;

            SelectedIndex = Mathf.Clamp(index, 0, slotUIs.Length - 1);

            if (selectionFrame != null && slotUIs.Length > SelectedIndex)
            {
                selectionFrame.gameObject.SetActive(true);
                selectionFrame.position = slotUIs[SelectedIndex].transform.position;
            }
        }

        public int GetSelectedDataIndex()
        {
            return startIndex + SelectedIndex;
        }
    }
}
