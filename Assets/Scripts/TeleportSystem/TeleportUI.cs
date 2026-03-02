using System.Collections; // 追加
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.MapSystem;
using GameCore.UISystem;

namespace GameCore.UISystem
{
    public class TeleportUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private WarpManager warpManager;
        [SerializeField] private UIManager uiManager;

        [Header("UI Parts")]
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform listContent;

        private void OnEnable()
        {
            // 画面が開くたびに更新を試みる
            StartCoroutine(TryRefreshList());
        }

        private void Start()
        {
            if (warpManager == null) warpManager = FindAnyObjectByType<WarpManager>();
            if (uiManager == null) uiManager = FindAnyObjectByType<UIManager>();

            if (warpManager != null)
            {
                // ★修正: 画面が開いている（アクティブな）時だけリストを更新する
                // 閉じている時は無視しても、次に開いた時に OnEnable で更新されるので大丈夫です
                warpManager.OnWarpUnlocked += () =>
                {
                    if (this.gameObject.activeInHierarchy)
                    {
                        StartCoroutine(TryRefreshList());
                    }
                };
            }
        }

        // マネージャーが見つかるまで少し待ってからリスト更新するコルーチン
        private IEnumerator TryRefreshList()
        {
            // 最大0.5秒くらい待つ（マネージャーの初期化待ち）
            int retryCount = 0;
            while (warpManager == null && retryCount < 10)
            {
                warpManager = FindAnyObjectByType<WarpManager>();
                yield return new WaitForSeconds(0.05f);
                retryCount++;
            }

            if (warpManager == null)
            {
                Debug.LogError("TeleportUI: WarpManagerが見つかりませんでした。BootSceneから開始していますか？");
                yield break;
            }

            RefreshList();
        }

        private void RefreshList()
        {
            if (listContent == null || buttonPrefab == null) return;

            // 1. 全削除
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            // 2. データ取得
            List<WarpData> unlockedPoints = warpManager.GetUnlockedPoints();

            Debug.Log($"TeleportUI: 解放済みポイント数 = {unlockedPoints.Count}");

            // 3. 生成
            foreach (var point in unlockedPoints)
            {
                GameObject btnObj = Instantiate(buttonPrefab, listContent);

                TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = point.DisplayName;

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    string targetID = point.ID;
                    btn.onClick.AddListener(() => OnWarpButtonClicked(targetID));
                }
            }
        }

        private void OnWarpButtonClicked(string id)
        {
            Debug.Log($"ワープID: {id} が選択されました");
            warpManager.TeleportTo(id);
            if (uiManager != null) uiManager.OnClickCloseMenu();
        }
    }
}