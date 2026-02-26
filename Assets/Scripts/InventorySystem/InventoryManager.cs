using System;
using UnityEngine;

namespace GameCore.InventorySystem
{
    /// <summary>
    /// カバンの1マス（スロット）のデータ構造
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData Item; // 何のアイテムが入っているか（空ならnull）
        public int Amount;    // 何個入っているか

        public bool IsEmpty => Item == null || Amount <= 0;

        // スロットを空にする
        public void Clear()
        {
            Item = null;
            Amount = 0;
        }
    }

    /// <summary>
    /// プレイヤーのインベントリ（40マス）のデータを管理するクラス。
    /// UI（画面表示）には一切触れず、「データが変化したこと」だけを通知します。
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        // インベントリのサイズ（横10 × 縦4 = 40マス）
        private const int TotalSlots = 40;
        private const int HotbarSlots = 10; // 1段目のホットバーの数

        // 実際のアイテムデータ配列
        [SerializeField] private InventorySlot[] slots = new InventorySlot[TotalSlots];

        // UIに「カバンの中身が変わったよ！」と知らせるイベント
        public event Action<int, InventorySlot> OnSlotUpdated;

        // カバンが一杯の時に「アイテムを入れ替えて捨てるか？」を聞くためのイベント
        public event Action<ItemData, int> OnInventoryFull;

        private void Awake()
        {
            // ゲーム開始時に40マスをすべて空の状態で初期化
            for (int i = 0; i < TotalSlots; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// アイテムをカバンに追加し、入り切らなかった「余りの個数」を返す
        /// </summary>
        public int AddItem(ItemData itemToAdd, int amount)
        {
            // 1. 同じアイテムにスタック
            for (int i = 0; i < TotalSlots; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item == itemToAdd)
                {
                    int spaceLeft = itemToAdd.MaxStackCount - slots[i].Amount;
                    if (spaceLeft > 0)
                    {
                        int addAmount = Mathf.Min(spaceLeft, amount);
                        slots[i].Amount += addAmount;
                        amount -= addAmount;

                        OnSlotUpdated?.Invoke(i, slots[i]);
                        if (amount <= 0) return 0; // ★変更: 全部入ったら 0 を返す
                    }
                }
            }

            // 2. 空いているスロットに入れる
            for (int i = 0; i < TotalSlots; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].Item = itemToAdd;
                    int addAmount = Mathf.Min(itemToAdd.MaxStackCount, amount);
                    slots[i].Amount = addAmount;
                    amount -= addAmount;

                    OnSlotUpdated?.Invoke(i, slots[i]);
                    if (amount <= 0) return 0; // ★変更: 全部入ったら 0 を返す
                }
            }

            // 3. 一杯の場合
            if (amount > 0)
            {
                Debug.LogWarning("インベントリが一杯です！");
                OnInventoryFull?.Invoke(itemToAdd, amount);
            }

            return amount; // ★追加: カバンに入り切らなかった余りの数を返す
        }

        /// <summary>
        /// スロットを指定してアイテムを消費・捨てる
        /// </summary>
        public void RemoveItemAt(int index, int amountToRemove)
        {
            if (index < 0 || index >= TotalSlots || slots[index].IsEmpty) return;

            slots[index].Amount -= amountToRemove;

            if (slots[index].Amount <= 0)
            {
                slots[index].Clear();
            }

            // データが変わったのでUIへ通知
            OnSlotUpdated?.Invoke(index, slots[index]);
        }

        /// <summary>
        /// 外部（UIなど）からスロットのデータを取得する
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= TotalSlots) return null;
            return slots[index];
        }


        /// <summary>
        /// 2つのスロットのアイテムを入れ替える。
        /// 同じアイテム同士なら1つの枠にまとめる（スタックする）。
        /// </summary>
        public void SwapOrMergeSlots(int fromIndex, int toIndex)
        {
            // 範囲外の指定や、同じマス同士なら何もしない
            if (fromIndex < 0 || fromIndex >= TotalSlots || toIndex < 0 || toIndex >= TotalSlots) return;
            if (fromIndex == toIndex) return;

            InventorySlot fromSlot = slots[fromIndex];
            InventorySlot toSlot = slots[toIndex];

            // 1. 両方に同じアイテムが入っている場合は「まとめる」
            if (!fromSlot.IsEmpty && !toSlot.IsEmpty && fromSlot.Item == toSlot.Item)
            {
                // ドロップ先にあと何個入るか計算
                int spaceLeft = toSlot.Item.MaxStackCount - toSlot.Amount;
                int moveAmount = Mathf.Min(spaceLeft, fromSlot.Amount);

                toSlot.Amount += moveAmount;
                fromSlot.Amount -= moveAmount;

                // ドラッグ元が0個になったら空にする
                if (fromSlot.Amount <= 0) fromSlot.Clear();
            }
            // 2. 違うアイテム、または片方が空の場合は「入れ替える」
            else
            {
                // データを丸ごと入れ替える
                InventorySlot temp = new InventorySlot();
                temp.Item = fromSlot.Item;
                temp.Amount = fromSlot.Amount;

                fromSlot.Item = toSlot.Item;
                fromSlot.Amount = toSlot.Amount;

                toSlot.Item = temp.Item;
                toSlot.Amount = temp.Amount;
            }

            // データの変更をUIに通知する（ここで画面のアイコンが一瞬で切り替わる）
            OnSlotUpdated?.Invoke(fromIndex, fromSlot);
            OnSlotUpdated?.Invoke(toIndex, toSlot);

            Debug.Log($"スロット {fromIndex} と {toIndex} を入れ替え/結合しました。");
        }


        // --- InventoryManager.cs に追加 ---

        // アイテムを使った時の通知（PlayerStaminaなどで受け取る用）
        public event Action<ItemData> OnItemUsed;

        // アイテムを捨てた時の通知（フィールドに3Dモデルを生成する用）
        public event Action<ItemData, int> OnItemDropped;

        /// <summary>
        /// 指定したスロットのアイテムを1つ「使う」
        /// </summary>
        public void UseItemAt(int index)
        {
            if (index < 0 || index >= TotalSlots || slots[index].IsEmpty) return;

            ItemData item = slots[index].Item;

            // 消費アイテムでなければ使えない
            if (item.Type != ItemType.Consumable)
            {
                Debug.Log($"{item.ItemName} はここでは使えません。");
                return;
            }

            // アイテムを使ったことを全体に通知（ここでスタミナ回復などをさせる）
            OnItemUsed?.Invoke(item);
            Debug.Log($"{item.ItemName} を使用しました！");

            // 1個減らす
            RemoveItemAt(index, 1);
        }

        /// <summary>
        /// 指定したスロットのアイテムを「すべて捨てる」
        /// </summary>
        public void DropItemAt(int index)
        {
            if (index < 0 || index >= TotalSlots || slots[index].IsEmpty) return;

            ItemData itemToDrop = slots[index].Item;
            int dropAmount = slots[index].Amount;

            // アイテムを捨てたことを全体に通知（ここで目の前に3Dモデルを出す）
            OnItemDropped?.Invoke(itemToDrop, dropAmount);
            Debug.Log($"{itemToDrop.ItemName} を {dropAmount}個 捨てました。");

            // スロットを空にする
            slots[index].Clear();
            OnSlotUpdated?.Invoke(index, slots[index]);
        }


    }

}
