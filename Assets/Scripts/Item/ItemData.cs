using UnityEngine;
// CropDataクラス（FarmingSystem）を参照するために必要
using GameCore.FarmingSystem;

namespace GameCore.InventorySystem
{
    // --- ★ここが消えていた定義です ---
    /// <summary>
    /// アイテムの基本タイプ
    /// </summary>
    public enum ItemType
    {
        Consumable, // 消費アイテム（回復薬など）
        Material,   // 素材（石、木材など）
        Equipment   // 装備品
    }

    // --- ★農業用に追加した定義 ---
    /// <summary>
    /// 農具や種の種類
    /// </summary>
    public enum ItemToolType
    {
        None,           // 道具ではない
        Hoe,            // クワ（耕す）
        WateringCan,    // ジョウロ（水やり）
        Sickle,         // カマ（刈り取る）
        Seed            // 種（植える）
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "GameCore/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string ItemID;
        public string ItemName;
        public Sprite Icon;
        [TextArea(2, 4)]
        public string Description;
        public int MaxStackCount = 99;
        public GameObject DropPrefab;

        [Header("Item Properties")]
        public ItemType Type = ItemType.Material; // ← これでエラーが消えます
        public int StaminaRecoverAmount = 0;

        [Header("Farming Settings")]
        public ItemToolType ToolType = ItemToolType.None; // 道具の種類
        public CropData CropToPlant; // 種の場合、どの作物が育つか
    }
}