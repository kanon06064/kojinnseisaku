using UnityEngine;

namespace GameCore.PlayerSystem
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private PlayerInputController inputController;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            inputController = GetComponent<PlayerInputController>();
            inputController.OnJumpPressed += Jump;
        }

        private void OnDestroy()
        {
            if (inputController != null) inputController.OnJumpPressed -= Jump;
        }

        private void Update()
        {
            // 1. 移動ベクトルの計算
            Vector3 moveDirection = Vector3.zero;
            Vector2 input = inputController.MoveInput;

            if (cameraTransform != null && input.sqrMagnitude > 0.01f)
            {
                // カメラの「正面」と「右」を取得
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;

                // 上下の傾きを無視して水平にする
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                // 入力とカメラの向きを掛け合わせる
                moveDirection = (forward * input.y + right * input.x).normalized;
            }

            // 2. 重力の計算
            ApplyGravity();

            // 3. 移動実行
            Vector3 finalVelocity = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);
            controller.Move(finalVelocity * Time.deltaTime);

            // ★削除しました: 4. キャラクターの向きを進行方向に向ける処理
            // FPS/TPS操作ではマウスで向きを決めるため、ここで勝手に回転させてはいけません。
            /*
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
            */
        }

        private void Jump()
        {
            if (controller.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0) verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
    }
}