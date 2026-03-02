using UnityEngine;
using GameCore.SceneManagement; // ★修正: 新しい名前空間を使用

// ★修正: namespaceを変更
namespace GameCore.SceneManagement
{
    /// <summary>
    /// BootSceneで最初に実行され、ゲーム本編（FieldScene）へ遷移させるクラス。
    /// </summary>
    public class BootStrap : MonoBehaviour
    {
        [Tooltip("最初に読み込むシーン名")]
        [SerializeField] private string firstSceneName = "FieldScene";

        private void Start()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(firstSceneName);
            }
            else
            {
                Debug.LogError("SceneLoaderが見つかりません。BootSceneの設定を確認してください。");
            }
        }
    }
}