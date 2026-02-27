using System.Collections.Generic;
using UnityEngine;
using GameCore.MonsterSystem; // MonsterSpeciesを使うため

namespace GameCore.PartySystem
{
    public enum MonsterSize
    {
        S = 1,
        M = 2,
        L = 3,
        Omega = 4
    }

    /// <summary>
    /// モンスターの個体データ（セーブ対象）
    /// </summary>
    [System.Serializable]
    public class MonsterData
    {
        public string Nickname;
        public MonsterSpecies Species;

        public int Level = 1;
        public int CurrentHP;
        public int CurrentMP;
        public int MaxHP;
        public int MaxMP;

        public MonsterData(MonsterSpecies species, string nickname = "")
        {
            Species = species;
            Nickname = string.IsNullOrEmpty(nickname) ? species.SpeciesName : nickname;

            Level = 1;
            MaxHP = species.BaseMaxHP;
            MaxMP = species.BaseMaxMP;
            CurrentHP = MaxHP;
            CurrentMP = MaxMP;
        }
    }

    public class PartyManager : MonoBehaviour
    {
        public const int MaxPartyCost = 4;

        public List<MonsterData> MainParty => mainParty;
        public List<MonsterData> SubParty => subParty;
        public List<MonsterData> Storage => storage;

        [Header("Party Data")]
        [SerializeField] private List<MonsterData> mainParty = new List<MonsterData>();
        [SerializeField] private List<MonsterData> subParty = new List<MonsterData>();
        [SerializeField] private List<MonsterData> storage = new List<MonsterData>();

        // データ更新通知
        public event System.Action OnPartyUpdated;

        [Header("Debug / Test Add")]
        public MonsterSpecies testSpecies1; // テスト用: スライムなどをInspectorでセット
        public MonsterSpecies testSpecies2; // テスト用: ドラゴンなどをInspectorでセット

        private void Awake()
        {
            // テスト用データの投入
            if (testSpecies1 != null)
            {
                AddMonster(new MonsterData(testSpecies1, "スラきち"));
                AddMonster(new MonsterData(testSpecies1, "スラりん"));
            }
            if (testSpecies2 != null)
            {
                AddMonster(new MonsterData(testSpecies2, "ドラさん"));
            }
        }

        public int GetTotalCost(List<MonsterData> party)
        {
            int total = 0;
            foreach (var monster in party)
            {
                if (monster?.Species != null)
                {
                    total += (int)monster.Species.Size;
                }
            }
            return total;
        }

        public void AddMonster(MonsterData newMonster)
        {
            if (GetTotalCost(mainParty) + (int)newMonster.Species.Size <= MaxPartyCost)
            {
                mainParty.Add(newMonster);
            }
            else if (GetTotalCost(subParty) + (int)newMonster.Species.Size <= MaxPartyCost)
            {
                subParty.Add(newMonster);
            }
            else
            {
                storage.Add(newMonster);
            }

            OnPartyUpdated?.Invoke();
        }
    }
}