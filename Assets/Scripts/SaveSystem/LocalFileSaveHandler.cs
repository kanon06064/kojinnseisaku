using System.IO;
using UnityEngine;

namespace GameCore.SaveSystem
{
    /// <summary>
    /// PC（Windows/Mac）や開発エディタ上で動く、標準的なファイル保存処理。
    /// ISaveHandlerの設計図通りに実装します。
    /// </summary>
    public class LocalFileSaveHandler : ISaveHandler
    {
        // 保存先のパスを生成するヘルパーメソッド
        private string GetPath(string key) => $"{Application.persistentDataPath}/{key}.json";

        public void Save(string key, string jsonData)
        {
            File.WriteAllText(GetPath(key), jsonData);
            Debug.Log($"[PC用] セーブ完了: {GetPath(key)}");
        }

        public string Load(string key)
        {
            if (Exists(key))
            {
                return File.ReadAllText(GetPath(key));
            }
            return null;
        }

        public bool Exists(string key)
        {
            return File.Exists(GetPath(key));
        }
    }
}