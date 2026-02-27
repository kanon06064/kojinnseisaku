using UnityEngine;
using GameCore.PlayerSystem;

namespace GameCore.UISystem
{
    public enum MenuTab
    {
        None,           // どのタブも開いていない（セレクト画面の状態）
        Party,
        SkillTeleport,
        Map,
        Inventory,
        Settings
    }

    /// <summary>
    /// UI全体の開閉と、メニューハブ（セレクト画面）の管理を行うクラス
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PlayerInputController inputController;

        [Header("Main Panels")]
        [SerializeField] private GameObject hudPanel;       // ゲーム中のHUD
        [SerializeField] private GameObject menuRootPanel;  // メニュー画面全体の親
        [SerializeField] private GameObject selectHubPanel; // ★追加: ボタンが並ぶセレクト画面

        [Header("Sub Pages")]
        [SerializeField] private GameObject partyPage;
        [SerializeField] private GameObject teleportPage;
        [SerializeField] private GameObject mapPage;
        [SerializeField] private GameObject inventoryPage;
        [SerializeField] private GameObject settingsPage;

        private bool isMenuOpen = false;

        private void Awake()
        {
            // 初期状態設定
            menuRootPanel.SetActive(false);
            hudPanel.SetActive(true);

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
        /// Tabキーが押された時の処理（メニューの開閉）
        /// </summary>
        private void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;

            if (isMenuOpen)
            {
                // メニューを開く（最初はセレクトハブを表示）
                OpenSelectHub();

                // ゲーム内時間を停止＆カーソル表示
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // メニューを閉じてゲームに戻る
                CloseAllMenus();

                // ゲーム内時間を再開＆カーソルロック
                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        /// <summary>
        /// メニューを閉じ、HUDを表示する
        /// </summary>
        private void CloseAllMenus()
        {
            menuRootPanel.SetActive(false);
            hudPanel.SetActive(true);
        }

        /// <summary>
        /// セレクトハブ（ボタン一覧）を表示する
        /// </summary>
        public void OpenSelectHub()
        {
            // メニューのルートを表示
            menuRootPanel.SetActive(true);
            hudPanel.SetActive(false);

            // ハブを表示し、他のページは全て隠す
            selectHubPanel.SetActive(true);

            partyPage.SetActive(false);
            teleportPage.SetActive(false);
            mapPage.SetActive(false);
            inventoryPage.SetActive(false);
            settingsPage.SetActive(false);
        }

        /// <summary>
        /// ハブから特定のページへ遷移する
        /// </summary>
        public void OpenPage(MenuTab tabToOpen)
        {
            // ハブを隠す
            selectHubPanel.SetActive(false);

            // 指定されたページだけを表示
            partyPage.SetActive(tabToOpen == MenuTab.Party);
            teleportPage.SetActive(tabToOpen == MenuTab.SkillTeleport);
            mapPage.SetActive(tabToOpen == MenuTab.Map);
            inventoryPage.SetActive(tabToOpen == MenuTab.Inventory);
            settingsPage.SetActive(tabToOpen == MenuTab.Settings);
        }

        // --- Unity Inspectorのボタン(OnClick)から割り当てる用 ---

        public void OnClickParty() => OpenPage(MenuTab.Party);
        public void OnClickTeleport() => OpenPage(MenuTab.SkillTeleport);
        public void OnClickMap() => OpenPage(MenuTab.Map);
        public void OnClickInventory() => OpenPage(MenuTab.Inventory);
        public void OnClickSettings() => OpenPage(MenuTab.Settings);

        // 各画面から「戻る」ボタンでハブに戻る用
        public void OnClickBackToHub() => OpenSelectHub();

        // 「閉じる」ボタン用
        public void OnClickCloseMenu() => ToggleMenu();
    }
}