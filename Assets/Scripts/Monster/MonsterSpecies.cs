using UnityEngine;
using GameCore.PartySystem; // MonsterSizeを使うため

namespace GameCore.MonsterSystem
{
    /// <summary>
    /// モンスターの「種族」データ。
    /// 例: スライム、ドラキーなどの基本情報。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMonsterSpecies", menuName = "GameCore/Monster Species")]
    public class MonsterSpecies : ScriptableObject
    {
        public string SpeciesName;   // 種族名
        public Sprite Icon;          // アイコン画像
        public GameObject ModelPrefab; // 3Dモデル（戦闘・フィールド用）

        [Header("Basic Stats")]
        public MonsterSize Size = MonsterSize.S; // サイズ(S, M, L, Omega)
        public int BaseMaxHP = 100;
        public int BaseMaxMP = 50;
        public int BaseAttack = 10;
        public int BaseDefense = 10;

        [TextArea]
        public string Description;   // 図鑑説明文など
    }
}