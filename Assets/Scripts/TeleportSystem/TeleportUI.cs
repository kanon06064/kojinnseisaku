using System.Collections;
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
                // ★修正: ラムダ式ではなく、名前付きメソッドを登録する
                warpManager.OnWarpUnlocked += OnWarpUnlockedHandler;
            }
        }

        private void OnDestroy()
        {
            // ★追加: UIが消える時に、イベントの登録を解除する（超重要）
            if (warpManager != null)
            {
                warpManager.OnWarpUnlocked -= OnWarpUnlockedHandler;
            }
        }

        // ★追加: イベントから呼ばれる専用のメソッド
        private void OnWarpUnlockedHandler()
        {
            // 自分がまだ存在していて、かつアクティブな時だけ処理する
            if (this != null && this.gameObject.activeInHierarchy)
            {
                StartCoroutine(TryRefreshList());
            }
        }

        private IEnumerator TryRefreshList()
        {
            int retryCount = 0;
            while (warpManager == null && retryCount < 10)
            {
                warpManager = FindAnyObjectByType<WarpManager>();
                yield return new WaitForSeconds(0.05f);
                retryCount++;
            }

            if (warpManager == null)
            {
                yield break;
            }

            RefreshList();
        }

        private void RefreshList()
        {
            if (listContent == null || buttonPrefab == null) return;

            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            List<WarpData> unlockedPoints = warpManager.GetUnlockedPoints();

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
            warpManager.TeleportTo(id);
            if (uiManager != null) uiManager.OnClickCloseMenu();
        }
    }
}