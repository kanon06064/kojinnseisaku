using UnityEngine;
using GameCore.PlayerSystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// メニュー画面に存在するタブの種類
    /// </summary>
    public enum MenuTab
    {
        Party,
        SkillTeleport,
        Map,
        Inventory,
        Settings
    }

    /// <summary>
    /// UI全体の開閉と、メニュー内のタブ切り替えを統括するマネージャー
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PlayerInputController inputController; [Header("UI Panels")]
        [SerializeField] private GameObject hudPanel;  // メイン画面（時計やスタミナ）
        [SerializeField] private GameObject menuPanel; // メニュー画面全体

        [Header("Menu Tab Pages")][SerializeField] private GameObject partyPage;
        [SerializeField] private GameObject teleportPage;
        [SerializeField] private GameObject mapPage;
        [SerializeField] private GameObject inventoryPage;
        [SerializeField] private GameObject settingsPage;

        private bool isMenuOpen = false;
        private MenuTab currentTab = MenuTab.Inventory; // 初期タブをインベントリに設定

        private void Awake()
        {
            // 初期状態はメニューを閉じ、メイン画面を表示
            menuPanel.SetActive(false);
            hudPanel.SetActive(true);

            // Tabキーが押された通知を受け取る
            if (inputController != null)
            {
                inputController.OnTabPressed += ToggleMenu;
            }
        }

        private void OnDestroy()
        {
            if (inputController != null)
            {
                inputController.OnTabPressed -= ToggleMenu;
            }
        }

        /// <summary>
        /// メニューの開閉を切り替える
        /// </summary>
        private void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;

            if (isMenuOpen)
            {
                // メニューを開く
                hudPanel.SetActive(false);
                menuPanel.SetActive(true);

                // ★追加: カーソルを表示し、ロックを解除する（クリックできるようにする）
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // ゲームの時間を止める
                Time.timeScale = 0f;

                SwitchTab(currentTab);
            }
            else
            {
                // メニューを閉じる
                menuPanel.SetActive(false);
                hudPanel.SetActive(true);

                // ★追加: カーソルを消し、中央にロックする（視点移動できるようにする）
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                // ゲームの時間を動かす
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// メニュー内のタブを切り替える（ボタンから呼び出されることを想定）
        /// </summary>
        public void SwitchTab(MenuTab tabToOpen)
        {
            currentTab = tabToOpen;

            // 一旦すべてのページを非表示にする
            partyPage.SetActive(false);
            teleportPage.SetActive(false);
            mapPage.SetActive(false);
            inventoryPage.SetActive(false);
            settingsPage.SetActive(false);

            // 指定されたページだけを表示する
            switch (tabToOpen)
            {
                case MenuTab.Party: partyPage.SetActive(true); break;
                case MenuTab.SkillTeleport: teleportPage.SetActive(true); break;
                case MenuTab.Map: mapPage.SetActive(true); break;
                case MenuTab.Inventory: inventoryPage.SetActive(true); break;
                case MenuTab.Settings: settingsPage.SetActive(true); break;
            }
        }

        // --- 以下、UnityのUIボタンから呼び出すためのヘルパー関数 ---
        public void OnClickPartyTab() => SwitchTab(MenuTab.Party);
        public void OnClickTeleportTab() => SwitchTab(MenuTab.SkillTeleport);
        public void OnClickMapTab() => SwitchTab(MenuTab.Map);
        public void OnClickInventoryTab() => SwitchTab(MenuTab.Inventory);
        public void OnClickSettingsTab() => SwitchTab(MenuTab.Settings);
    }
}