using UnityEngine;
using UnityEngine.InputSystem; // 新Input Systemを使う場合
using GameCore.TimeSystem;
using GameCore.FarmingSystem;

namespace GameCore.DebugSystem
{
    /// <summary>
    /// 開発用チートツール。
    /// キーボードショートカットで時間操作や作物の強制成長を行う。
    /// </summary>
    public class DebugController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private float interactRange = 5.0f; // 遠くからでもデバッグできるように長めに

        private void Update()
        {
            // --- 時間操作 ---

            // [T] + [Shift] : 1日進める
            // [T] のみ      : 1時間進める
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (Keyboard.current.shiftKey.isPressed)
                {
                    timeManager.DebugSkipDay();
                }
                else
                {
                    timeManager.DebugSkipHour();
                }
            }

            // [Y] : 時間の高速化 (押しっぱなしで早送り)
            if (Keyboard.current.yKey.isPressed)
            {
                Time.timeScale = 10.0f; // 10倍速
            }
            else if (Keyboard.current.yKey.wasReleasedThisFrame)
            {
                Time.timeScale = 1.0f; // 等倍に戻す
            }


            // --- 農業デバッグ ---

            // [G] : 目の前の作物を強制成長 (Grow)
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                ForceGrowCropInFront();
            }
        }

        /// <summary>
        /// 目の前の畑を探して強制成長させる
        /// </summary>
        private void ForceGrowCropInFront()
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // レイヤーマスクは「Everything」にして、あらゆる物体を検知できるようにする
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                FarmPlot plot = hit.collider.GetComponentInParent<FarmPlot>();
                if (plot != null)
                {
                    plot.DebugForceGrow();
                }
            }
        }
    }
}