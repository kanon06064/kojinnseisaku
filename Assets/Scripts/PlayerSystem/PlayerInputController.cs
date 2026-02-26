using System;
using UnityEngine;
using UnityEngine.InputSystem; // 新Input Systemを使用

namespace GameCore.PlayerSystem
{
    /// <summary>
    /// プレイヤーの入力を一括して管理するクラス。
    /// 各種ボタンの入力状態を、他のコンポーネントへイベントとして伝達します。
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        // 移動用のベクトル (WASD)
        public Vector2 MoveInput { get; private set; }
        // 視点移動用のベクトル (マウス)
        public Vector2 LookInput { get; private set; }

        // 各種アクションのイベント（押された瞬間に通知）
        public event Action OnJumpPressed;
        public event Action OnTabPressed; // メニュー・テレポート
        public event Action OnTalkPressed; // 右クリック
        public event Action OnToolPressed; // 左クリック
        public event Action OnInteractPressed; // Fキー

        private void Update()
        {
            // WASDの取得 (Input Systemの簡易的な取得方法)
            float moveX = 0f, moveY = 0f;
            if (Keyboard.current.wKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed) moveY -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            MoveInput = new Vector2(moveX, moveY).normalized;

            // マウス移動の取得
            if (Mouse.current != null)
            {
                LookInput = Mouse.current.delta.ReadValue();
            }

            // 各種ボタンの検知とイベント発火
            if (Keyboard.current.spaceKey.wasPressedThisFrame) OnJumpPressed?.Invoke();
            if (Keyboard.current.tabKey.wasPressedThisFrame) OnTabPressed?.Invoke();
            if (Mouse.current.rightButton.wasPressedThisFrame) OnTalkPressed?.Invoke();
            if (Mouse.current.leftButton.wasPressedThisFrame) OnToolPressed?.Invoke();
            if (Keyboard.current.fKey.wasPressedThisFrame) OnInteractPressed?.Invoke();
        }
    }
}