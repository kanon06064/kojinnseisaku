using UnityEngine;
using GameCore.TimeSystem;
using GameCore.InventorySystem;
using GameCore.PlayerSystem;

namespace GameCore.FarmingSystem
{
    // --- ★ここが消えていた部分です ---
    /// <summary>
    /// 畑の状態定義
    /// </summary>
    public enum PlotState
    {
        Empty,      // 荒地（何もできない）
        Tilled,     // 耕地（種をまける）
        Growing,    // 育成中（種～収穫前）
        Harvestable,// 収穫可能
        Dead        // 枯れ
    }
    // --------------------------------

    public class FarmPlot : MonoBehaviour, IInteractable
    {
        [Header("Status")]
        [SerializeField] private PlotState currentState = PlotState.Empty;
        [SerializeField] private bool isWatered = false;

        [Header("Crop Info")]
        [SerializeField] private CropData currentCrop;
        [SerializeField] private int growthDays = 0;
        [SerializeField] private int currentStageIndex = 0;

        [Header("Visuals")]
        [SerializeField] private Renderer soilRenderer;
        [SerializeField] private Transform cropParent;

        // 色の設定
        [SerializeField] private Color emptyColor = new Color(0.8f, 0.6f, 0.4f);  // 荒地
        [SerializeField] private Color tilledColor = new Color(0.6f, 0.4f, 0.2f); // 耕地
        [SerializeField] private Color wetColor = new Color(0.3f, 0.2f, 0.1f);    // 水やり後

        private TimeManager timeManager;

        private void Start()
        {
            timeManager = FindAnyObjectByType<TimeManager>();
            if (timeManager != null)
            {
                timeManager.OnDayChanged += HandleDayChanged;
            }
            UpdateSoilVisual();
        }

        private void OnDestroy()
        {
            if (timeManager != null) timeManager.OnDayChanged -= HandleDayChanged;
        }

        // --- プレイヤーからの道具アクション ---
        public bool TryInteract(ItemData toolItem)
        {
            if (toolItem == null) return false;

            // 1. クワで耕す
            if (toolItem.ToolType == ItemToolType.Hoe)
            {
                if (currentState == PlotState.Empty)
                {
                    currentState = PlotState.Tilled;
                    Debug.Log("畑を耕しました！");
                    UpdateSoilVisual();
                    return true;
                }
                else
                {
                    Debug.Log("既に耕されています。（種をまけます）");
                    return false;
                }
            }

            // 2. ジョウロで水やり
            if (toolItem.ToolType == ItemToolType.WateringCan)
            {
                if (currentState != PlotState.Empty && !isWatered)
                {
                    isWatered = true;
                    Debug.Log("水をやりました！");
                    UpdateSoilVisual();
                    return true;
                }
            }

            // 3. カマで刈り取る
            if (toolItem.ToolType == ItemToolType.Sickle)
            {
                if (currentState == PlotState.Dead || currentState == PlotState.Harvestable || currentState == PlotState.Growing)
                {
                    ClearPlot();
                    currentState = PlotState.Empty; // 刈り取った後は荒地にする
                    UpdateSoilVisual();
                    Debug.Log("作物を刈り取りました。");
                    return true;
                }
            }

            // 4. 種をまく
            if (toolItem.ToolType == ItemToolType.Seed)
            {
                if (toolItem.CropToPlant == null)
                {
                    Debug.LogError($"エラー: {toolItem.ItemName} の CropToPlant が設定されていません。");
                    return false;
                }

                if (currentState == PlotState.Tilled && currentCrop == null)
                {
                    Plant(toolItem.CropToPlant);
                    Debug.Log($"{toolItem.CropToPlant.CropName} の種をまきました！");
                    return true;
                }
            }

            return false;
        }

        // --- プレイヤーからのインタラクト（Fキー/収穫） ---
        public void Interact(GameObject interactor)
        {
            // デバッグログ
            string cropName = (currentCrop != null) ? currentCrop.CropName : "なし";
            Debug.Log($"[{gameObject.name}] Interact判定 - 状態:{currentState}, 作物:{cropName}, 経過:{growthDays}日");

            // 収穫可能状態でなければ無視
            if (currentState != PlotState.Harvestable || currentCrop == null)
            {
                Debug.Log($"[{gameObject.name}] まだ収穫できません。（Interact終了）");
                return;
            }

            // インベントリ取得
            InventoryManager inventory = interactor.GetComponent<InventoryManager>();
            if (inventory == null) return;

            // アイテム追加
            int left = inventory.AddItem(currentCrop.HarvestItem, currentCrop.HarvestCount);

            if (left == 0) // 成功
            {
                Debug.Log($"[{gameObject.name}] 収穫しました！: {currentCrop.HarvestItem.ItemName}");

                // ★ここがポイント：一度リセットしてから「耕地」に戻す
                ClearPlot();
                currentState = PlotState.Tilled;
                UpdateSoilVisual();
            }
            else
            {
                Debug.LogWarning("カバンがいっぱいです！");
            }
        }

        // --- 内部ロジック ---

        private void Plant(CropData crop)
        {
            currentCrop = crop;
            currentState = PlotState.Growing;
            growthDays = 0;
            currentStageIndex = 0;
            UpdateCropVisual();
            UpdateSoilVisual(); // 種をまいた直後も土の色を更新（耕地色のまま維持）
        }

        private void ClearPlot()
        {
            currentCrop = null;
            growthDays = 0;
            currentStageIndex = 0;
            isWatered = false;
            currentState = PlotState.Empty; // デフォルトは荒地

            foreach (Transform child in cropParent) Destroy(child.gameObject);
            UpdateSoilVisual();
        }

        private void HandleDayChanged(GameTime time)
        {
            if (currentState == PlotState.Growing && currentCrop != null)
            {
                if (isWatered)
                {
                    growthDays++;

                    int maxStages = currentCrop.GrowthStages.Length;
                    if (maxStages > 0)
                    {
                        float progress = (float)growthDays / currentCrop.DaysToGrow;
                        int nextStage = Mathf.Clamp(Mathf.FloorToInt(progress * maxStages), 0, maxStages - 1);

                        if (nextStage > currentStageIndex)
                        {
                            currentStageIndex = nextStage;
                            UpdateCropVisual();
                        }
                    }

                    if (growthDays >= currentCrop.DaysToGrow)
                    {
                        currentState = PlotState.Harvestable;
                    }
                }
            }

            isWatered = false; // 水は乾く
            UpdateSoilVisual();
        }

        // --- 見た目更新 ---

        private void UpdateSoilVisual()
        {
            if (soilRenderer != null)
            {
                if (isWatered)
                {
                    soilRenderer.material.color = wetColor; // 水やり
                }
                else if (currentState != PlotState.Empty)
                {
                    soilRenderer.material.color = tilledColor; // 耕地・栽培中
                }
                else
                {
                    soilRenderer.material.color = emptyColor; // 荒地
                }
            }
        }

        private void UpdateCropVisual()
        {
            foreach (Transform child in cropParent) Destroy(child.gameObject);

            if (currentCrop != null && currentCrop.GrowthStages.Length > currentStageIndex)
            {
                GameObject prefab = currentCrop.GrowthStages[currentStageIndex];
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, cropParent);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }
            }
        }

        // --- デバッグ用 ---
        public void DebugForceGrow()
        {
            if (currentState == PlotState.Growing && currentCrop != null)
            {
                growthDays++;
                int maxStages = currentCrop.GrowthStages.Length;
                if (maxStages > 0)
                {
                    float progress = (float)growthDays / currentCrop.DaysToGrow;
                    int nextStage = Mathf.Clamp(Mathf.FloorToInt(progress * maxStages), 0, maxStages - 1);

                    if (nextStage > currentStageIndex)
                    {
                        currentStageIndex = nextStage;
                        UpdateCropVisual();
                        Debug.Log($"<color=green>[Debug]</color> 強制成長: {currentCrop.CropName} (Stage {currentStageIndex})");
                    }
                }

                if (growthDays >= currentCrop.DaysToGrow)
                {
                    currentState = PlotState.Harvestable;
                    Debug.Log($"<color=green>[Debug]</color> 強制成長完了: 収穫可能になりました");
                }
            }
            else
            {
                Debug.LogWarning("作物が植えられていないか、成長中ではありません。");
            }
        }
    }
}