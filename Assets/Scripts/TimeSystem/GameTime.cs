using UnityEngine;

namespace GameCore.TimeSystem
{
    /// <summary>
    /// ゲーム内の日時データを保持する構造体。
    /// classではなくstructにすることで、メモリ割り当て(GC)を抑止します。
    /// </summary>
    [System.Serializable]
    public struct GameTime
    {
        public Season Season; // 季節
        public int Day;       // 日 (1〜30などを想定)
        public int Hour;      // 時 (0〜23)
        public int Minute;    // 分 (0〜59)

        public GameTime(Season season, int day, int hour, int minute)
        {
            Season = season;
            Day = day;
            Hour = hour;
            Minute = minute;
        }

        // デバッグ表示やUI表示用に文字列化するメソッド
        public override string ToString()
        {
            return $"{Season}の月 {Day}日 {Hour:D2}:{Minute:D2}";
        }
    }
}