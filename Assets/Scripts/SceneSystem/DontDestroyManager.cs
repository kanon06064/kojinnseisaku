using UnityEngine;

// ★修正: namespaceを変更
namespace GameCore.SceneManagement
{
    /// <summary>
    /// アタッチされたオブジェクトを、シーン遷移しても破壊されないようにするクラス。
    /// GameManagerなどに使用します。
    /// </summary>
    public class DontDestroyManager : MonoBehaviour
    {
        private void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }
}