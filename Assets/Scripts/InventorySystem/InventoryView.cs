using UnityEngine;
using GameCore.InventorySystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// InventoryManager（データ）の変更を監視し、該当するUIスロットを更新するクラス。
    /// ホットバーと全体メニューの両方で「使い回せる」ように設計しています。
    /// </summary>
    public class InventoryView : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private InventoryManager inventoryManager; [Header("UI Settings")]
        [Tooltip("このViewが管理するUIスロットの配列（Inspectorで割り当てる）")]
        [SerializeField] private InventorySlotUI[] slotUIs; [Tooltip("InventoryManagerの何番目のスロットから監視を開始するか")]
        [SerializeField] private int startIndex = 0;

        private void Start()
        {
            if (inventoryManager != null)
            {
                // データが更新されたら、HandleSlotUpdatedを呼ぶように登録
                inventoryManager.OnSlotUpdated += HandleSlotUpdated;

                // ゲーム開始時に、現在のデータでUIをすべて更新する
                RefreshAllSlots();
            }
        }

        private void OnDestroy()
        {
            // エラー防止のための登録解除
            if (inventoryManager != null)
            {
                inventoryManager.OnSlotUpdated -= HandleSlotUpdated;
            }
        }

        /// <summary>
        /// InventoryManagerのどこかのマスが変化した時に呼ばれる
        /// </summary>
        /// <param name="dataIndex">変化したマスの番号(0〜39)</param>
        /// <param name="slotData">変化後の最新データ</param>
        private void HandleSlotUpdated(int dataIndex, InventorySlot slotData)
        {
            // 通知された番号が、このViewが担当している範囲内かチェックする
            if (dataIndex >= startIndex && dataIndex < startIndex + slotUIs.Length)
            {
                // データの番号(dataIndex)を、UI配列の番号(uiIndex)に変換する
                int uiIndex = dataIndex - startIndex;

                // 該当するUIマスの見た目を更新！
                slotUIs[uiIndex].UpdateUI(slotData);
            }
        }

        /// <summary>
        /// 全てのスロットの見た目を強制的に最新状態にする
        /// </summary>
        private void RefreshAllSlots()
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                int dataIndex = startIndex + i;
                // ★追加: スロットUIに自分の番号を教える
                slotUIs[i].Initialize(dataIndex);

                InventorySlot data = inventoryManager.GetSlot(dataIndex);
                slotUIs[i].UpdateUI(data);
            }
        }
    }
}