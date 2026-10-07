using System;
using ProjectFantasy.Items;

namespace ProjectFantasy.InventorySystem
{
    // UI가 요청하는 아이템 조작 (플레이어는 PlayerItemHandler가 구현, UI는 플레이어를 모름)
    public interface IItemActions
    {
        ItemInstance EquippedWeapon { get; }
        ItemInstance EquippedOffHand { get; }
        // 보조 장비가 현재 무기와 호환되어 효과가 적용 중인지
        bool IsOffHandActive { get; }

        event Action EquipmentChanged;

        bool IsEquipped(ItemInstance instance);
        // 숙련도가 필요한 장비면 요구 조건 반환
        bool TryGetRequirement(ItemInstance instance, out EquipRequirement requirement);
        bool TryActivateSlot(int slotIndex);
        bool TryActivateQuickSlot(int quickSlotIndex);
        void Unequip(ItemInstance instance);
        void Drop(int slotIndex);
    }
}
