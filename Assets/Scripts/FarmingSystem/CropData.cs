using UnityEngine;
using GameCore.InventorySystem; // 収穫物(ItemData)を参照するため

namespace GameCore.FarmingSystem
{
    [CreateAssetMenu(fileName = "NewCrop", menuName = "GameCore/Farming/Crop Data")]
    public class CropData : ScriptableObject
    {
        public string CropName;         // 作物の名前
        public int DaysToGrow;          // 収穫までにかかる日数
        public ItemData HarvestItem;    // 収穫できるアイテム
        public int HarvestCount = 1;    // 収穫数

        [Header("Growth Visuals")]
        // 成長段階ごとの見た目（プレハブやMesh）
        // 0: 種, 1: 芽, 2: 成長中, 3: 収穫可能 ... とリストで管理
        public GameObject[] GrowthStages;

        // 枯れた時の見た目
        public GameObject DeadVisual;
    }
}