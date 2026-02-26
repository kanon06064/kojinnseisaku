using System;
using UnityEngine;

namespace GameCore.PlayerSystem
{
    /// <summary>
    /// プレイヤーのスタミナ（農作業等で消費）を管理するクラス
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private int maxStamina = 100;
        private int currentStamina;

        public event Action<int, int> OnStaminaChanged; // UI更新用イベント
        public event Action OnStaminaExhausted; // 倒れた時のイベント

        private void Awake()
        {
            currentStamina = maxStamina;
        }

        /// <summary>
        /// アクション実行時にスタミナを消費する
        /// </summary>
        public bool ConsumeStamina(int amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                return true; // アクション成功
            }
            else
            {
                Debug.Log("スタミナが足りません！");
                OnStaminaExhausted?.Invoke();
                return false; // アクション失敗（倒れるなど）
            }
        }

        /// <summary>
        /// スタミナを回復する
        /// </summary>
        public void RecoverStamina(int amount)
        {
            currentStamina += amount;
            if (currentStamina > maxStamina) currentStamina = maxStamina;

            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            Debug.Log($"スタミナが {amount} 回復しました。現在: {currentStamina} / {maxStamina}");
        }


    }
}