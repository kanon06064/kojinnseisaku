using UnityEngine;
using TMPro;
using GameCore.TimeSystem;
using GameCore.PlayerSystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// メイン画面（HUD）の表示を管理するクラス。
    /// TimeManagerやPlayerStaminaを自動取得して表示を更新します。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private PlayerStamina playerStamina;

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI timeText;    // 右上: 季節/日/時間
        [SerializeField] private TextMeshProUGUI weatherText; // 中央上: 天候
        [SerializeField] private TextMeshProUGUI staminaText; // 左上: スタミナ

        private void Start()
        {
            // --- ★修正: マネージャーの自動取得 ---
            if (timeManager == null)
            {
                timeManager = FindAnyObjectByType<TimeManager>();
            }
            if (playerStamina == null)
            {
                playerStamina = FindAnyObjectByType<PlayerStamina>();
            }

            // --- イベント登録と初期表示 ---

            if (timeManager != null)
            {
                timeManager.OnMinuteChanged += UpdateTimeUI;
                // 現在の時間を表示
                UpdateTimeUI(timeManager.CurrentTime);
            }

            if (playerStamina != null)
            {
                playerStamina.OnStaminaChanged += UpdateStaminaUI;
                // 初期値の表示更新はPlayerStamina側で初期化時にイベントが呼ばれるか、
                // ここで手動で取得する処理が必要ですが、一旦イベント待ちとします。
            }

            // 天候は仮置き
            if (weatherText != null) weatherText.text = "☀ 晴れ";
        }

        private void OnDestroy()
        {
            // メモリリーク防止のための登録解除
            if (timeManager != null) timeManager.OnMinuteChanged -= UpdateTimeUI;
            if (playerStamina != null) playerStamina.OnStaminaChanged -= UpdateStaminaUI;
        }

        /// <summary>
        /// 時間が変わるたびに呼ばれる処理
        /// </summary>
        private void UpdateTimeUI(GameTime time)
        {
            if (timeText != null)
            {
                timeText.text = $"{time.Season} {time.Day}日 {time.Hour:D2}:{time.Minute:D2}";
            }
        }

        /// <summary>
        /// スタミナが変化した時に呼ばれる処理
        /// </summary>
        private void UpdateStaminaUI(int current, int max)
        {
            if (staminaText != null)
            {
                staminaText.text = $"Stamina: {current} / {max}";
            }
        }
    }
}