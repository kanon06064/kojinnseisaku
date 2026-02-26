using UnityEngine;
using GameCore.TimeSystem;

namespace GameCore.SaveSystem
{
    /// <summary>
    /// ゲーム全体のセーブとロードを統括する静的(static)クラス。
    /// </summary>
    public static class SaveManager
    {
        // どの保存方法を使うかを保持する。デフォルトではPC用をセット。
        // コンシューマー機への移植時は、別のハンドラーをセットするだけで対応可能。
        private static ISaveHandler saveHandler = new LocalFileSaveHandler();

        /// <summary>
        /// 外部からセーブのハンドラー（保存方法）を切り替えるメソッド
        /// </summary>
        public static void SetSaveHandler(ISaveHandler handler)
        {
            saveHandler = handler;
        }

        /// <summary>
        /// 時間データをJSONにして保存する
        /// </summary>
        public static void SaveTime(GameTime timeData)
        {
            // C#のデータをJSON文字列に変換
            string json = JsonUtility.ToJson(timeData, true);

            // ハンドラーに保存を委譲する
            saveHandler.Save("GameTimeData", json);
        }

        /// <summary>
        /// JSONファイルを読み込んで時間データに戻す
        /// </summary>
        public static GameTime LoadTime()
        {
            // セーブデータが存在しない場合は初期値を返す
            if (!saveHandler.Exists("GameTimeData"))
            {
                Debug.LogWarning("時間データのセーブファイルがありません。初期データを返します。");
                return new GameTime(Season.Spring, 1, 6, 0);
            }

            // ハンドラーからJSON文字列を読み込む
            string json = saveHandler.Load("GameTimeData");

            // JSON文字列をC#のデータに変換して返す
            GameTime loadedTime = JsonUtility.FromJson<GameTime>(json);

            Debug.Log("時間データをロードしました！");
            return loadedTime;
        }
    }
}