using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ★修正: namespaceを System から SceneManagement に変更
namespace GameCore.SceneManagement
{
    /// <summary>
    /// 画面のフェード付きシーン遷移を管理するクラス。
    /// BootSceneで生成され、以降は破棄されずに常駐します。
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("UI Settings")]
        [Tooltip("フェード用の黒いパネルのCanvasGroup")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        [Tooltip("暗転にかかる時間（秒）")]
        [SerializeField] private float fadeDuration = 1.0f;

        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
                StartCoroutine(FadeIn());
            }
        }

        public void LoadScene(string sceneName)
        {
            if (!isTransitioning)
            {
                StartCoroutine(TransitionSequence(sceneName));
            }
        }

        private IEnumerator TransitionSequence(string sceneName)
        {
            isTransitioning = true;

            yield return StartCoroutine(FadeOut());

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
            yield return null;

            yield return StartCoroutine(FadeIn());

            isTransitioning = false;
        }

        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - (timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }
    }
}