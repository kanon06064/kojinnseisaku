using System.Collections.Generic;
using UnityEngine;
// GameCore.PlayerSystemの名前空間は削除しても動くようにGameObjectで検索します

namespace GameCore.MapSystem
{
    [System.Serializable]
    public class WarpData
    {
        public string ID;
        public string DisplayName;
        public Vector3 Coordinate;
        public bool IsUnlocked;
    }

    public class WarpManager : MonoBehaviour
    {
        [Header("Warp Points Database")]
        [SerializeField] private List<WarpData> warpPoints = new List<WarpData>();

        [Header("References (Auto Assigned)")]
        // Inspectorで設定しなくても、コードで自動取得するように変更
        [SerializeField] private GameObject player;
        [SerializeField] private CharacterController playerController;

        public event System.Action OnWarpUnlocked;

        public void UnlockWarpPoint(string id)
        {
            var point = warpPoints.Find(p => p.ID == id);
            if (point != null && !point.IsUnlocked)
            {
                point.IsUnlocked = true;
                Debug.Log($"ワープポイント解放: {point.DisplayName}");
                OnWarpUnlocked?.Invoke();
            }
        }

        public void TeleportTo(string id)
        {
            // --- ★修正: プレイヤーを自動で見つける処理 ---
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerController = player.GetComponent<CharacterController>();
                }
            }

            if (player == null)
            {
                Debug.LogError("エラー: WarpManagerがプレイヤーを見つけられませんでした。PlayerのTagが 'Player' になっているか確認してください。");
                return;
            }
            // ---------------------------------------------

            var point = warpPoints.Find(p => p.ID == id);
            if (point != null && point.IsUnlocked)
            {
                Debug.Log($"テレポート実行: {point.DisplayName} へ移動します");

                if (playerController != null) playerController.enabled = false;

                player.transform.position = point.Coordinate;

                if (playerController != null) playerController.enabled = true;
            }
        }

        public List<WarpData> GetUnlockedPoints()
        {
            return warpPoints.FindAll(p => p.IsUnlocked);
        }

        public void UnlockAll()
        {
            foreach (var p in warpPoints) p.IsUnlocked = true;
            OnWarpUnlocked?.Invoke();
        }
    }
}