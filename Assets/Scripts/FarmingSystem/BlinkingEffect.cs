using UnityEngine;
using UnityEngine.UI; // Image操作用

namespace GameCore.UISystem
{
    /// <summary>
    /// アタッチしたオブジェクト（ImageまたはMesh）を点滅させる
    /// </summary>
    public class BlinkingEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 3.0f;      // 点滅の速さ
        [SerializeField] private float minAlpha = 0.3f;   // 最も薄い時の透明度
        [SerializeField] private float maxAlpha = 1.0f;   // 最も濃い時の透明度

        private Image targetImage;
        private CanvasGroup canvasGroup;
        private Renderer targetRenderer;

        private void Awake()
        {
            // アタッチされているコンポーネントを自動判別して取得
            targetImage = GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();
            targetRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            // サイン波を使って 0.0 ～ 1.0 の値をいったりきたりさせる
            float wave = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
            // min ～ max の範囲に変換
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

            SetAlpha(alpha);
        }

        private void SetAlpha(float alpha)
        {
            // UI (Image) の場合
            if (targetImage != null)
            {
                Color c = targetImage.color;
                c.a = alpha;
                targetImage.color = c;
            }
            // UI Group の場合
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            // 3Dオブジェクト の場合
            else if (targetRenderer != null)
            {
                Color c = targetRenderer.material.color;
                c.a = alpha;
                targetRenderer.material.color = c;
            }
        }
    }
}