using UnityEngine;
using TMPro; // TextMeshProを使用
using GameCore.TimeSystem;
using GameCore.PlayerSystem;

namespace GameCore.UISystem
{
    /// <summary>
    /// メイン画面（HUD: Head-Up Display）の表示を管理するクラス。
    /// 時間やスタミナが変化した「通知」を受け取って、画面を書き換えます。
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
            // 時間が変わったときの通知を受け取るように登録
            if (timeManager != null)
            {
                timeManager.OnMinuteChanged += UpdateTimeUI;
                // 起動時に現在の時間を表示
                UpdateTimeUI(timeManager.CurrentTime);
            }

            // スタミナが変わったときの通知を受け取るように登録
            if (playerStamina != null)
            {
                playerStamina.OnStaminaChanged += UpdateStaminaUI;
                // ※起動時のスタミナ表示はPlayerStamina側から初期値を取得する想定
            }

            // 天候は一旦仮置き
            if (weatherText != null) weatherText.text = "☀ 晴れ";
        }

        private void OnDestroy()
        {
            // メモリリーク（エラー）を防ぐための登録解除
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
                // 例: "Spring 12日 14:30" のように表示
                timeText.text = $"{time.Season} {time.Day}日 {time.Hour:D2}:{time.Minute:D2}";
            }
        }

        /// <summary>
        /// スタミナが減った（増えた）時に呼ばれる処理
        /// </summary>
        private void UpdateStaminaUI(int current, int max)
        {
            if (staminaText != null)
            {
                staminaText.text = $"Stamina: {current} / {max}";
            }
            // ※将来的にはここでゲージ（画像）の長さを変える処理も追加します
        }
    }
}