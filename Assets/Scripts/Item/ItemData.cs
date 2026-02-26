using UnityEngine;

namespace GameCore.InventorySystem
{
    public enum ItemType
    {
        Consumable, // 消費アイテム（使うことができる）
        Material,   // 素材（使うことはできない）
        Equipment   // 装備品（今回は保留）
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "GameCore/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ItemID;
        public string ItemName;
        public Sprite Icon;
        [TextArea(2, 4)]
        public string Description;
        public int MaxStackCount = 99;
        public GameObject DropPrefab; // 捨てた時にフィールドに出す3Dモデル

        [Header("Item Properties")]
        public ItemType Type = ItemType.Material; // アイテムの種類
        public int StaminaRecoverAmount = 0;      // 使うとスタミナがどれくらい回復するか
    }
}