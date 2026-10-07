using ProjectFantasy.Core;
using ProjectFantasy.Mastery;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 처치 보상 → 처치 시점에 장착한 무기(맨손 포함)와 활성 보조 장비 숙련도가 각각 전액 획득 (알비온 방식)
    [RequireComponent(typeof(PlayerLoadout), typeof(WeaponMastery))]
    [DisallowMultipleComponent]
    public sealed class PlayerMasteryRewarder : MonoBehaviour, IKillRewardReceiver
    {
        private PlayerLoadout loadout;
        private WeaponMastery mastery;

        private void Awake()
        {
            loadout = GetComponent<PlayerLoadout>();
            mastery = GetComponent<WeaponMastery>();
        }

        public void ReceiveKillReward(int experience)
        {
            if (MasteryMapping.TryGet(loadout.CurrentWeapon, out MasteryType weaponType)) mastery.AddExperience(weaponType, experience);
            if (MasteryMapping.TryGet(loadout.ActiveOffHand, out MasteryType offHandType)) mastery.AddExperience(offHandType, experience);
        }
    }
}
