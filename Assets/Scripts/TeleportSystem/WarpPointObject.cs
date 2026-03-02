using UnityEngine;

namespace GameCore.MapSystem
{
    public class WarpPointObject : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string warpID; // Manager‚É“o˜^‚µ‚½ID‚Æˆê’v‚³‚¹‚é

        private WarpManager warpManager;

        private void Start()
        {
            warpManager = FindAnyObjectByType<WarpManager>();
        }

        // ƒvƒŒƒCƒ„[‚ªG‚ê‚½‚ç‰ğ•ú
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (warpManager != null)
                {
                    warpManager.UnlockWarpPoint(warpID);
                }
            }
        }
    }
}