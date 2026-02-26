using UnityEngine;
using UnityEngine.InputSystem; // マウススクロール取得用
using GameCore.PlayerSystem;

namespace GameCore.CameraSystem
{
    /// <summary>
    /// FPS/TPS両対応、壁抜け防止機能付きの高機能カメラコントローラー
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("プレイヤーの入力コントローラー")]
        [SerializeField] private PlayerInputController inputController;
        [Tooltip("カメラが追従する中心点（プレイヤーの頭/目）")]
        [SerializeField] private Transform cameraRoot;
        [Tooltip("回転させるプレイヤー本体")]
        [SerializeField] private Transform playerBody;

        [Header("Camera Control")]
        [SerializeField] private float mouseSensitivity = 15f;
        [SerializeField] private float lookUpMin = -70f; // 見上げ限界
        [SerializeField] private float lookDownMax = 80f; // 見下ろし限界

        [Header("Distance (FPS/TPS)")]
        [SerializeField] private float defaultDistance = 3.0f; // 初期距離
        [SerializeField] private float minDistance = 0.0f;     // 0ならFPS
        [SerializeField] private float maxDistance = 6.0f;     // 最大TPS距離
        [SerializeField] private float zoomSpeed = 2.0f;       // スクロールの感度
        [SerializeField] private float smoothTime = 0.1f;      // 動きの滑らかさ

        [Header("Collision (Anti-Clip)")]
        [Tooltip("カメラがめり込まないようにするレイヤー（壁や地面など）")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float cameraRadius = 0.2f;    // カメラの当たり判定サイズ

        // 内部変数
        private float currentX = 0f; // 上下の角度
        private float currentDistance;
        private float targetDistance;
        private Vector3 currentVelocity; // スムーズ移動用

        private void Start()
        {
            // カーソルをロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 初期距離を設定
            targetDistance = defaultDistance;
            currentDistance = targetDistance;

            // 親子関係の解除（重要：カメラがプレイヤーの動きにガタつかないようにするため）
            // ただし、今回はシンプルさを優先しPlayerの子オブジェクトのままで制御します
        }

        private void LateUpdate()
        {
            if (inputController == null || cameraRoot == null) return;

            HandleRotation();
            HandleZoom();
            HandlePositionAndCollision();
        }

        /// <summary>
        /// マウス入力による回転処理
        /// </summary>
        private void HandleRotation()
        {
            Vector2 lookInput = inputController.LookInput * mouseSensitivity * Time.deltaTime;

            // 1. 左右の回転 -> プレイヤーの体ごと回す
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * lookInput.x);
            }

            // 2. 上下の回転 -> カメラの角度だけ制限付きで回す
            currentX -= lookInput.y;
            currentX = Mathf.Clamp(currentX, lookUpMin, lookDownMax);
        }

        /// <summary>
        /// マウススクロールによる距離変更（FPS/TPS切り替え）
        /// </summary>
        private void HandleZoom()
        {
            // Input Systemからのスクロール値取得
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;

            // スクロール値を正規化（-1〜1程度に）して距離に加算
            // scroll > 0 (奥へ転がす) -> 近づく -> FPSへ
            // scroll < 0 (手前へ転がす) -> 離れる -> TPSへ
            if (scroll != 0)
            {
                targetDistance -= Mathf.Sign(scroll) * zoomSpeed;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }
        }

        /// <summary>
        /// カメラ位置の決定と、壁めり込み防止処理
        /// </summary>
        private void HandlePositionAndCollision()
        {
            // プレイヤーの頭の回転（上下）を計算
            Quaternion rotation = Quaternion.Euler(currentX, playerBody.eulerAngles.y, 0);

            // 本来あるべきカメラの位置（壁を考慮しない場合）
            // 頭の位置から、後ろへ targetDistance 分下がった場所
            Vector3 desiredPosition = cameraRoot.position - (rotation * Vector3.forward * targetDistance);

            // --- 壁めり込み防止 (Collision Check) ---
            float finalDistance = targetDistance;

            // 頭(cameraRoot)からカメラ予定地(desiredPosition)に向かって線を引く
            Vector3 direction = desiredPosition - cameraRoot.position;

            // もしTPS視点(距離がある)なら、壁判定を行う
            if (targetDistance > 0.1f)
            {
                // SphereCastを使って、「太さのある線」で壁を検知する（より正確）
                if (Physics.SphereCast(cameraRoot.position, cameraRadius, direction.normalized, out RaycastHit hit, targetDistance, collisionLayers))
                {
                    // 壁に当たったら、その壁の手前までカメラを寄せる
                    finalDistance = hit.distance;
                }
            }

            // --- 実際にカメラを動かす ---

            // FPS視点(距離ほぼ0)のときは、頭の位置に完全に合わせる
            if (finalDistance < 0.1f)
            {
                transform.position = cameraRoot.position;
                transform.rotation = rotation;
            }
            else
            {
                // TPS視点
                Vector3 newPos = cameraRoot.position - (rotation * Vector3.forward * finalDistance);

                // 少しだけ遅延させて滑らかに追従させる（ガタつき防止）
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, smoothTime);
                transform.rotation = rotation;
            }
        }
    }
}