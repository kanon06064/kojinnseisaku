namespace GameCore.SaveSystem
{
    /// <summary>
    /// セーブ・ロード機能のインターフェース。
    /// PC用、Switch用などで実装を切り替えられるようにするための枠組みです。
    /// </summary>
    public interface ISaveHandler
    {
        void Save(string key, string jsonData);
        string Load(string key);
        bool Exists(string key);
    }
}