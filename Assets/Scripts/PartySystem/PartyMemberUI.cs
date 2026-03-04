using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.PartySystem;

namespace GameCore.BattleSystem
{
    public class PartyMemberUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText; // "100/100"ÇÃÇÊÇ§Ç»êîíl

        private int maxHP;

        public void Setup(MonsterData monster)
        {
            if (monster == null) return;

            if (nameText) nameText.text = monster.Nickname;
            if (iconImage) iconImage.sprite = monster.Species.Icon;

            maxHP = monster.MaxHP;
            if (hpSlider)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = monster.CurrentHP;
            }
            UpdateHPText(monster.CurrentHP);
        }

        public void UpdateHP(int current)
        {
            if (hpSlider) hpSlider.value = current;
            UpdateHPText(current);
        }

        private void UpdateHPText(int current)
        {
            if (hpText) hpText.text = $"{current} / {maxHP}";
        }
    }
}