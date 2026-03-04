using System.Collections;
using UnityEngine;

namespace GameCore.BattleSystem
{
    /// <summary>
    /// 戦闘中のカメラ演出を管理するクラス。
    /// 全体俯瞰、コマンド選択位置、アクション時のズームなどを制御します。
    /// </summary>
    public class BattleCameraController : MonoBehaviour
    {
        [Header("Fixed Positions")]
        [Tooltip("戦闘開始時の全体俯瞰ポジション（斜め上空など）")]
        [SerializeField] private Transform overviewPos;

        [Tooltip("コマンド選択時の定位置（味方の背後など）")]
        [SerializeField] private Transform commandPos;

        [Header("Action Camera Settings")]
        [Tooltip("アクション時に攻撃者からどれくらい離れるか")]
        [SerializeField] private float actionZoomDistance = 4.0f;

        [Tooltip("アクション時のカメラの高さ")]
        [SerializeField] private float actionHeight = 2.0f;

        [Tooltip("カメラ移動にかける時間（秒）")]
        [SerializeField] private float moveDuration = 0.6f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
        }

        // --- 基本的な移動（定位置へ） ---

        /// <summary>
        /// コマンド選択の定位置へ移動
        /// </summary>
        public IEnumerator MoveToCommandView()
        {
            if (commandPos != null)
            {
                yield return StartCoroutine(MoveToPosition(commandPos.position, commandPos.rotation, moveDuration));
            }
        }

        /// <summary>
        /// 全体俯瞰の位置へ移動
        /// </summary>
        public IEnumerator MoveToOverview()
        {
            if (overviewPos != null)
            {
                // 瞬間移動させたい場合は duration を 0 にする処理を入れても良いですが、今回は滑らかに動かします
                yield return StartCoroutine(MoveToPosition(overviewPos.position, overviewPos.rotation, moveDuration));
            }
        }

        // --- アクションカメラ（動的計算） ---

        /// <summary>
        /// 攻撃するキャラクターと対象を結ぶ線上の、攻撃者後方にカメラを移動させる
        /// </summary>
        /// <param name="attacker">攻撃するキャラ（味方 or 敵）</param>
        /// <param name="lookAtTarget">攻撃対象（敵 or 味方）</param>
        public IEnumerator FocusOnAttacker(Transform attacker, Transform lookAtTarget)
        {
            // カメラの目標位置を計算
            // 「攻撃者」から「対象」への方向ベクトル
            // の逆（対象から攻撃者に向かう方向）を使って、攻撃者の背後にカメラを置く

            Vector3 direction = (attacker.position - lookAtTarget.position).normalized;

            // 攻撃者の位置から、少し後ろへ下がり、高さを足す
            Vector3 targetPos = attacker.position + (direction * actionZoomDistance);
            targetPos.y += actionHeight;

            // カメラの向きを計算（対象の方を見る）
            Quaternion targetRot = Quaternion.LookRotation(lookAtTarget.position - targetPos);

            // 移動実行
            yield return StartCoroutine(MoveToPosition(targetPos, targetRot, moveDuration));
        }

        // --- 内部補間処理 ---

        private IEnumerator MoveToPosition(Vector3 endPos, Quaternion endRot, float duration)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                // SmoothStep (t*t*(3-2t)) で、動き出しと停止を滑らかにする
                t = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }

            // 最終的なズレを補正して確定
            transform.position = endPos;
            transform.rotation = endRot;
        }
    }
}