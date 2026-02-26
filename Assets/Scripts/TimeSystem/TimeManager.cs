using System;
using UnityEngine;

namespace GameCore.TimeSystem
{
    /// <summary>
    /// ゲーム内の時間を管理し、進行させるマネージャー。
    /// イベント駆動（Observerパターン）を採用し、他システムとの密結合を防ぎます。
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("現実の1秒で進むゲーム内の分数")][SerializeField] private float gameMinutesPerRealSecond = 1f;

        [Header("Current Time")]
        [SerializeField] private Season currentSeason = Season.Spring;
        [SerializeField] private int currentDay = 1;
        [SerializeField] private int currentHour = 6;
        [SerializeField] private int currentMinute = 0;

        // 内部で時間を計算するためのタイマー
        private float timer = 0f;

        // 1ヶ月の日数設定（ルーンファクトリーに合わせて30日とするなど）
        private const int DaysInMonth = 30;

        // --- イベント（他のスクリプトに通知するための仕組み） ---

        /// <summary>分が変化したときに呼ばれるイベント</summary>
        public event Action<GameTime> OnMinuteChanged;
        /// <summary>時間が変化したときに呼ばれるイベント</summary>
        public event Action<GameTime> OnHourChanged;
        /// <summary>日付が変わったときに呼ばれるイベント</summary>
        public event Action<GameTime> OnDayChanged;
        /// <summary>季節が変わったときに呼ばれるイベント</summary>
        public event Action<GameTime> OnSeasonChanged;

        /// <summary>現在の時間を取得するプロパティ</summary>
        public GameTime CurrentTime => new GameTime(currentSeason, currentDay, currentHour, currentMinute);

        private void Update()
        {
            TickTime();
        }

        /// <summary>
        /// 時間を進行させる処理
        /// </summary>
        private void TickTime()
        {
            timer += Time.deltaTime;

            // 現実の時間が設定した秒数経過したら、ゲーム内の1分を進める
            if (timer >= (1f / gameMinutesPerRealSecond))
            {
                timer = 0f;
                AdvanceMinute();
            }
        }

        /// <summary>
        /// 分を1つ進め、必要に応じて時間、日、季節も進める
        /// </summary>
        private void AdvanceMinute()
        {
            currentMinute++;

            // 分の変更を通知
            OnMinuteChanged?.Invoke(CurrentTime);

            if (currentMinute >= 60)
            {
                currentMinute = 0;
                AdvanceHour();
            }
        }

        private void AdvanceHour()
        {
            currentHour++;

            // 時間の変更を通知
            OnHourChanged?.Invoke(CurrentTime);

            if (currentHour >= 24)
            {
                currentHour = 0;
                AdvanceDay();
            }
        }

        private void AdvanceDay()
        {
            currentDay++;

            // 日付の変更を通知
            OnDayChanged?.Invoke(CurrentTime);

            if (currentDay > DaysInMonth)
            {
                currentDay = 1;
                AdvanceSeason();
            }
        }

        private void AdvanceSeason()
        {
            // 列挙型の次の値に進める (Winterの次はSpringに戻る)
            currentSeason = (Season)(((int)currentSeason + 1) % 4);

            // 季節の変更を通知
            OnSeasonChanged?.Invoke(CurrentTime);

            Debug.Log($"季節が変わりました！ 現在は {currentSeason} です。");
        }
    }
}