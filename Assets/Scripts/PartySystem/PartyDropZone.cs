using UnityEngine;

namespace GameCore.UISystem
{
    public enum PartyZoneType
    {
        Main,
        Sub,
        Storage // «—ˆ—p
    }

    public class PartyDropZone : MonoBehaviour
    {
        public PartyZoneType ZoneType;
    }
}