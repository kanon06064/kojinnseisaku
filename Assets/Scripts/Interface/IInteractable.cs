using UnityEngine;

namespace GameCore.PlayerSystem
{
    /// <summary>
    /// プレイヤーがFキーなどで「干渉できる」オブジェクトの共通ルール
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// インタラクトされた時に実行される処理
        /// </summary>
        /// <param name="interactor">アクションを起こした張本人（プレイヤー）</param>
        void Interact(GameObject interactor);
    }
}