using System.Collections.Generic;
using UnityEngine;

namespace GameCore.PartySystem
{
    /// <summary>
    /// モンスターのサイズと、それに伴う消費枠（コスト）の定義
    /// </summary>
    public enum MonsterSize
    {
        S = 1,     // 1枠消費
        M = 2,     // 2枠消費
        L = 3,     // 3枠消費
        Omega = 4  // 4枠消費 (Ωサイズ。1体で枠を使い切る)
    }

    /// <summary>
    /// モンスターの簡易データ
    /// </summary>
    [System.Serializable]
    public class MonsterData
    {
        public string Name;
        public MonsterSize Size;
    }

    /// <summary>
    /// メインとサブの編成、および倉庫送りを管理するクラス
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        // メイン、サブそれぞれ最大4枠
        private const int MaxPartyCost = 4;

        [Header("Party Status")]
        [SerializeField] private List<MonsterData> mainParty = new List<MonsterData>();
        [SerializeField] private List<MonsterData> subParty = new List<MonsterData>(); [SerializeField] private List<MonsterData> storage = new List<MonsterData>(); // 倉庫

        /// <summary>
        /// パーティにモンスターを追加するロジック
        /// </summary>
        /// <param name="newMonster">追加したいモンスターのデータ</param>
        public void AddMonster(MonsterData newMonster)
        {
            // 1. メインパーティの空き容量を計算
            int currentMainCost = GetTotalCost(mainParty);
            if (currentMainCost + (int)newMonster.Size <= MaxPartyCost)
            {
                mainParty.Add(newMonster);
                Debug.Log($"{newMonster.Name} ({newMonster.Size}サイズ) をメインパーティに加えました！");
                return;
            }

            // 2. メインに入らなければ、サブパーティの空き容量を計算
            int currentSubCost = GetTotalCost(subParty);
            if (currentSubCost + (int)newMonster.Size <= MaxPartyCost)
            {
                subParty.Add(newMonster);
                Debug.Log($"{newMonster.Name} ({newMonster.Size}サイズ) をサブパーティに加えました！");
                return;
            }

            // 3. どちらも一杯なら倉庫送り
            storage.Add(newMonster);
            Debug.Log($"パーティが一杯のため、{newMonster.Name} ({newMonster.Size}サイズ) を倉庫に送りました。");
        }

        /// <summary>
        /// リスト内のモンスターの合計サイズ(コスト)を計算して返す
        /// </summary>
        private int GetTotalCost(List<MonsterData> party)
        {
            int total = 0;
            foreach (var monster in party)
            {
                total += (int)monster.Size; // Enumに割り当てた数値をそのまま足し合わせる
            }
            return total;
        }
    }
}