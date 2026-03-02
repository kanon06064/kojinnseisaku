using UnityEngine;

namespace GameCore.MapSystem
{
    /// <summary>
    /// マップ用カメラをプレイヤーの真上に追従させるスクリプト
    /// </summary>
    public class MapCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform targetPlayer;
        [SerializeField] private float height = 50f; // カメラの高さ

        private void LateUpdate()
        {
            if (targetPlayer == null) return;

            // プレイヤーのX, Z座標だけをコピーし、高さ(Y)は固定する
            Vector3 newPos = targetPlayer.position;
            newPos.y = height;

            transform.position = newPos;

            // 回転は固定（北を上に固定する場合）
            // transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // もし「プレイヤーが向いている方を上にしたい」場合は以下を使います
            // Vector3 currentRot = transform.eulerAngles;
            // currentRot.y = targetPlayer.eulerAngles.y;
            // transform.rotation = Quaternion.Euler(90f, currentRot.y, 0f);
        }
    }
}