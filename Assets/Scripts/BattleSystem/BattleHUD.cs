using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.PartySystem;
using GameCore.MonsterSystem; // MonsterSpeciesを使うため

namespace GameCore.BattleSystem
{
    public class BattleHUD : MonoBehaviour
    {
        [Header("Log")]
        [SerializeField] private TextMeshProUGUI logText;

        [Header("Enemy Main Display (DQM Style)")]
        [SerializeField] private TextMeshProUGUI enemyNameText; // 名前のみ
        [SerializeField] private Image enemyIconImage;          // ひし形アイコンの中身
        [SerializeField] private TextMeshProUGUI enemySizeText; // "S" "M"マーク

        [Header("Enemy Details Window (Popup)")]
        [SerializeField] private GameObject detailsPanel;       // 詳細ウィンドウ全体
        [SerializeField] private Slider detailsHPSlider;        // 詳細用HPバー
        [SerializeField] private TextMeshProUGUI detailsHPText; // 数値 (100/100)

        [Header("Command Buttons")]
        [SerializeField] private GameObject commandPanel;

        [Header("Party Status")]
        [SerializeField] private Transform partyPanelParent;
        [SerializeField] private GameObject partyMemberPrefab;

        private List<PartyMemberUI> partyMemberUIs = new List<PartyMemberUI>();

        // --- 敵情報の初期化（戦闘開始時） ---
        public void SetupEnemyHUD(MonsterSpecies enemyData)
        {
            if (enemyData == null) return;

            // メイン画面には「名前」と「アイコン」と「サイズ」だけ出す
            if (enemyNameText != null) enemyNameText.text = enemyData.SpeciesName;

            if (enemyIconImage != null)
            {
                enemyIconImage.sprite = enemyData.Icon;
                enemyIconImage.enabled = (enemyData.Icon != null);
            }

            if (enemySizeText != null)
            {
                // Enumを文字列に変換 (Omega -> Ω などの変換が必要ならここで行う)
                string sizeStr = enemyData.Size == MonsterSize.Omega ? "Ω" : enemyData.Size.ToString();
                enemySizeText.text = sizeStr;
            }

            // 詳細ウィンドウは閉じておく
            CloseDetails();
        }

        // --- 詳細ウィンドウの操作 ---
        public void OpenEnemyDetails(MonsterSpecies enemyData, int currentHP, int maxHP)
        {
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(true);

                // HP情報の更新
                UpdateEnemyHP(currentHP, maxHP);
            }
        }

        public void CloseDetails()
        {
            if (detailsPanel != null) detailsPanel.SetActive(false);
        }

        // --- 敵HPの更新（詳細ウィンドウが開いている時用） ---
        public void UpdateEnemyHP(int current, int max)
        {
            if (detailsHPSlider != null)
            {
                detailsHPSlider.maxValue = max;
                detailsHPSlider.value = current;
            }

            if (detailsHPText != null)
            {
                detailsHPText.text = $"{current} / {max}";
            }
        }

        // --- ログ表示 ---
        public void SetLogText(string message)
        {
            if (logText != null) logText.text = message;
        }

        // --- コマンドボタン表示切替 ---
        public void ToggleCommandButtons(bool isActive)
        {
            if (commandPanel != null) commandPanel.SetActive(isActive);
        }

        // --- 味方パーティ表示 ---
        public void SetupPartyHUD(List<MonsterData> party)
        {
            // 既存の表示をクリア
            foreach (Transform child in partyPanelParent) Destroy(child.gameObject);
            partyMemberUIs.Clear();

            // 人数分パネルを作成
            foreach (var monster in party)
            {
                GameObject obj = Instantiate(partyMemberPrefab, partyPanelParent);
                PartyMemberUI ui = obj.GetComponent<PartyMemberUI>();

                if (ui != null)
                {
                    ui.Setup(monster);
                    partyMemberUIs.Add(ui);
                }
            }
        }

        public void UpdateAllyHP(int memberIndex, int current)
        {
            if (memberIndex >= 0 && memberIndex < partyMemberUIs.Count)
            {
                partyMemberUIs[memberIndex].UpdateHP(current);
            }
        }
    }
}