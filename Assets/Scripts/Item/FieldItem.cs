using UnityEngine;
using GameCore.InventorySystem;
using GameCore.PlayerSystem;

namespace GameCore.FieldSystem
{
    /// <summary>
    /// フィールド上に落ちているアイテムを制御するクラス
    /// </summary>
    public class FieldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;

        /// <summary>
        /// アイテムを捨てた時などに、データと個数を初期化するためのメソッド
        /// </summary>
        public void Initialize(ItemData data, int count)
        {
            itemData = data;
            amount = count;
        }

        /// <summary>
        /// プレイヤーがFキーで調べた瞬間に呼ばれる処理（IInteractableの実装）
        /// </summary>
        public void Interact(GameObject interactor)
        {
            // 調べた人（プレイヤー）のインベントリを取得
            var inventory = interactor.GetComponent<InventoryManager>();

            if (inventory != null && itemData != null)
            {
                // カバンにアイテムを入れる（戻り値として「入り切らなかった数」を受け取る）
                int leftAmount = inventory.AddItem(itemData, amount);

                if (leftAmount == 0)
                {
                    // 全部拾えたら、フィールド上の3Dモデルを消滅させる
                    Destroy(gameObject);
                }
                else
                {
                    // 拾いきれなかった分をフィールドに残す
                    amount = leftAmount;
                    Debug.Log($"持ち物がいっぱいで {amount} 個拾えませんでした。");
                }
            }
        }
    }
}