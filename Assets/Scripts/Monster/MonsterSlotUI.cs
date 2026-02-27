using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using GameCore.PartySystem;

namespace GameCore.UISystem
{
    public class MonsterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Parts")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lvText;
        [SerializeField] private TextMeshProUGUI sizeText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider mpSlider;
        [SerializeField] private GameObject emptyStateObject;
        [SerializeField] private GameObject dataStateObject;

        private MonsterData myMonsterData;
        private Transform originalParent;
        private CanvasGroup canvasGroup;

        public MonsterData Data => myMonsterData;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Setup(MonsterData monster)
        {
            myMonsterData = monster;

            if (monster == null)
            {
                if (dataStateObject) dataStateObject.SetActive(false);
                if (emptyStateObject) emptyStateObject.SetActive(true);
                if (iconImage) iconImage.enabled = false;
            }
            else
            {
                if (dataStateObject) dataStateObject.SetActive(true);
                if (emptyStateObject) emptyStateObject.SetActive(false);

                if (iconImage)
                {
                    iconImage.enabled = true;
                    iconImage.sprite = monster.Species.Icon;
                }

                if (nameText) nameText.text = monster.Nickname;
                if (lvText) lvText.text = $"Lv.{monster.Level}";

                if (sizeText)
                {
                    string sizeStr = monster.Species.Size == MonsterSize.Omega ? "Ω" : monster.Species.Size.ToString();
                    sizeText.text = sizeStr;
                }

                if (hpSlider)
                {
                    hpSlider.maxValue = monster.MaxHP;
                    hpSlider.value = monster.CurrentHP;
                }
                if (mpSlider)
                {
                    mpSlider.maxValue = monster.MaxMP;
                    mpSlider.value = monster.CurrentMP;
                }
            }
        }

        // --- ドラッグ処理（入れ替えロジックは今後実装） ---
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (myMonsterData == null) return;
            originalParent = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (myMonsterData == null) return;
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (myMonsterData == null) return;

            canvasGroup.blocksRaycasts = true;
            bool moveSuccess = false; // 移動できたかどうかのフラグ

            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

            if (hitObject != null)
            {
                PartyDropZone zone = hitObject.GetComponentInParent<PartyDropZone>();

                if (zone != null)
                {
                    PartyScreenUI screenUI = GetComponentInParent<PartyScreenUI>();
                    if (screenUI == null) screenUI = FindAnyObjectByType<PartyScreenUI>();

                    if (screenUI != null)
                    {
                        // 移動を試みて、結果を受け取る
                        moveSuccess = screenUI.TryMoveMonster(myMonsterData, zone.ZoneType);
                    }
                }
            }

            // ★ここが修正ポイント
            if (moveSuccess)
            {
                // 移動成功＝画面はリフレッシュ済みなので、この古いカードは用済み。消滅させる。
                Destroy(gameObject);
            }
            else
            {
                // 移動失敗（コストオーバーや場所ミス）なら、元の場所に戻す。
                if (transform.parent == transform.root)
                {
                    transform.SetParent(originalParent);
                    transform.localPosition = Vector3.zero;
                }
            }
        

            // 見た目を元の場所に戻す（移動成功してればあとで再描画されて消える）
            if (transform.parent == transform.root)
            {
                transform.SetParent(originalParent);
                transform.localPosition = Vector3.zero;
            }
        }
    }
}