using System;
using System.Collections.Generic;
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

        [Header("Starting Items")]
        [Tooltip("ニューゲーム開始時に最初から持っているアイテム")]
        [SerializeField] private List<ItemData> startingItems = new List<ItemData>();

        // 実際のアイテムデータ配列
        [SerializeField] private InventorySlot[] slots = new InventorySlot[TotalSlots];

        // --- イベント（通知） ---

        // UIに「カバンの中身が変わったよ！」と知らせるイベント
        public event Action<int, InventorySlot> OnSlotUpdated;

        // カバンが一杯の時に「アイテムを入れ替えて捨てるか？」を聞くためのイベント
        public event Action<ItemData, int> OnInventoryFull;

        // アイテムを使った時の通知（PlayerStaminaなどで受け取る用）
        public event Action<ItemData> OnItemUsed;

        // アイテムを捨てた時の通知（フィールドに3Dモデルを生成する用）
        public event Action<ItemData, int> OnItemDropped;


        private void Awake()
        {
            // ゲーム起動時に40マスをすべて空の状態で初期化
            // (slots配列自体はInspectorで見えるようにSerializeFieldにしているが、中身はnullの可能性があるのでnewする)
            if (slots == null || slots.Length != TotalSlots)
            {
                slots = new InventorySlot[TotalSlots];
            }

            for (int i = 0; i < TotalSlots; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new InventorySlot();
                }
            }
        }

        private void Start()
        {
            // ゲーム開始時に初期アイテムを配る
            // (ロード処理がある場合は、ロード後に上書きされる想定)
            foreach (var item in startingItems)
            {
                if (item != null)
                {
                    // 1個ずつ追加する
                    AddItem(item, 1);
                }
            }
        }

        /// <summary>
        /// アイテムをカバンに追加し、入り切らなかった「余りの個数」を返す
        /// </summary>
        /// <param name="itemToAdd">追加したいアイテム</param>
        /// <param name="amount">追加する個数</param>
        /// <returns>入り切らなかった数（0なら全て入った）</returns>
        public int AddItem(ItemData itemToAdd, int amount)
        {
            // 1. まず、同じアイテムが既にカバンにあり、スタック（重ねる）できるか探す
            for (int i = 0; i < TotalSlots; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item == itemToAdd)
                {
                    int spaceLeft = itemToAdd.MaxStackCount - slots[i].Amount;
                    if (spaceLeft > 0)
                    {
                        // スタックできる分だけ足す
                        int addAmount = Mathf.Min(spaceLeft, amount);
                        slots[i].Amount += addAmount;
                        amount -= addAmount;

                        // データが変わったのでUIへ通知
                        OnSlotUpdated?.Invoke(i, slots[i]);

                        // すべて追加し終えたら終了
                        if (amount <= 0) return 0;
                    }
                }
            }

            // 2. まだ追加しきれていない場合、空いているスロットを探す
            for (int i = 0; i < TotalSlots; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].Item = itemToAdd;
                    int addAmount = Mathf.Min(itemToAdd.MaxStackCount, amount);
                    slots[i].Amount = addAmount;
                    amount -= addAmount;

                    // データが変わったのでUIへ通知
                    OnSlotUpdated?.Invoke(i, slots[i]);

                    // すべて追加し終えたら終了
                    if (amount <= 0) return 0;
                }
            }

            // 3. ここまで来てamountが残っている場合は「カバンが一杯」
            if (amount > 0)
            {
                Debug.LogWarning("インベントリが一杯です！");
                // 「拾えなかったアイテム」と「個数」を通知（UI表示などに使う）
                OnInventoryFull?.Invoke(itemToAdd, amount);
            }

            return amount; // 入り切らなかった余りの数を返す
        }

        /// <summary>
        /// スロットを指定してアイテムを消費・削除する
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
                // (参照の入れ替えではなく、値のコピーを行う)
                ItemData tempItem = fromSlot.Item;
                int tempAmount = fromSlot.Amount;

                fromSlot.Item = toSlot.Item;
                fromSlot.Amount = toSlot.Amount;

                toSlot.Item = tempItem;
                toSlot.Amount = tempAmount;
            }

            // データの変更をUIに通知する（ここで画面のアイコンが一瞬で切り替わる）
            OnSlotUpdated?.Invoke(fromIndex, fromSlot);
            OnSlotUpdated?.Invoke(toIndex, toSlot);

            Debug.Log($"スロット {fromIndex} と {toIndex} を入れ替え/結合しました。");
        }

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