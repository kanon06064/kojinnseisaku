using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameCore.PartySystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// パーティ編成画面のマネージャー。
    /// BootSceneにあるPartyManagerを自動取得します。
    /// </summary>
    public class PartyScreenUI : MonoBehaviour
    {
        [Header("System")]
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private GameObject monsterSlotPrefab;

        [Header("Main Party Area")]
        [SerializeField] private Transform mainListParent;
        [SerializeField] private TextMeshProUGUI mainCostText;

        [Header("Sub Party Area")]
        [SerializeField] private Transform subListParent;
        [SerializeField] private TextMeshProUGUI subCostText;

        private List<MonsterSlotUI> mainSlots = new List<MonsterSlotUI>();
        private List<MonsterSlotUI> subSlots = new List<MonsterSlotUI>();

        private void Start()
        {
            // --- ★修正: マネージャーの自動取得 ---
            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }

            if (partyManager != null)
            {
                partyManager.OnPartyUpdated += RefreshUI;
                RefreshUI(); // 初回表示
            }
        }

        private void OnDestroy()
        {
            if (partyManager != null)
            {
                partyManager.OnPartyUpdated -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            if (partyManager == null) return;

            UpdateList(mainListParent, partyManager.MainParty, mainSlots);
            UpdateCostText(mainCostText, partyManager.MainParty);

            UpdateList(subListParent, partyManager.SubParty, subSlots);
            UpdateCostText(subCostText, partyManager.SubParty);
        }

        private void UpdateList(Transform parent, List<MonsterData> dataList, List<MonsterSlotUI> uiList)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
            uiList.Clear();

            foreach (var monster in dataList)
            {
                GameObject obj = Instantiate(monsterSlotPrefab, parent);
                MonsterSlotUI slot = obj.GetComponent<MonsterSlotUI>();
                slot.Setup(monster);
                uiList.Add(slot);
            }
        }

        private void UpdateCostText(TextMeshProUGUI textUI, List<MonsterData> list)
        {
            int current = partyManager.GetTotalCost(list);
            int max = PartyManager.MaxPartyCost;
            textUI.text = $"Cost: {current} / {max}";
            textUI.color = current > max ? Color.red : Color.white;
        }

        // --- モンスター移動処理 ---
        public bool TryMoveMonster(MonsterData monster, PartyZoneType targetZone)
        {
            if (partyManager == null) return false;

            List<MonsterData> targetList = null;
            List<MonsterData> currentList = null;

            if (targetZone == PartyZoneType.Main) targetList = partyManager.MainParty;
            else if (targetZone == PartyZoneType.Sub) targetList = partyManager.SubParty;

            if (partyManager.MainParty.Contains(monster)) currentList = partyManager.MainParty;
            else if (partyManager.SubParty.Contains(monster)) currentList = partyManager.SubParty;

            if (targetList == null || currentList == null) return false;

            // 同じリスト内（並び替え）
            if (targetList == currentList)
            {
                currentList.Remove(monster);
                currentList.Add(monster);
                RefreshUI();
                return true;
            }

            // 違うリストへ移動
            int currentCost = partyManager.GetTotalCost(targetList);
            int monsterSize = (int)monster.Species.Size;

            if (currentCost + monsterSize > PartyManager.MaxPartyCost)
            {
                Debug.LogWarning("コストオーバー！");
                return false;
            }

            currentList.Remove(monster);
            targetList.Add(monster);

            RefreshUI();
            return true;
        }
    }
}