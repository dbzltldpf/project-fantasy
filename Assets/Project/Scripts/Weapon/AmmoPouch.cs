using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 화살 종류별 보유 수 관리 (플레이어/적 공용)
    [DisallowMultipleComponent]
    public sealed class AmmoPouch : MonoBehaviour
    {
        private const int EmptyCount = 0;
        private const int SingleShot = 1;

        [Serializable]
        private struct AmmoStack
        {
            [SerializeField] private AmmoData ammo;
            [SerializeField, Min(0)] private int count;

            public AmmoData Ammo => ammo;
            public int Count => count;
        }

        [SerializeField] private AmmoStack[] startingAmmo = Array.Empty<AmmoStack>();

        private readonly Dictionary<AmmoData, int> counts = new Dictionary<AmmoData, int>();

        public event Action<AmmoData, int> AmmoChanged;

        private void Awake()
        {
            foreach (AmmoStack stack in startingAmmo)
            {
                if (stack.Ammo != null) Add(stack.Ammo, stack.Count);
            }
        }

        public int GetCount(AmmoData ammo) => ammo != null && counts.TryGetValue(ammo, out int count) ? count : EmptyCount;
        public bool HasAmmo(AmmoData ammo) => GetCount(ammo) > EmptyCount;

        public void Add(AmmoData ammo, int amount)
        {
            if (ammo == null || amount <= EmptyCount) return;

            int total = GetCount(ammo) + amount;
            counts[ammo] = total;
            AmmoChanged?.Invoke(ammo, total);
        }

        public bool TryConsume(AmmoData ammo)
        {
            int count = GetCount(ammo);
            if (count < SingleShot) return false;

            int remaining = count - SingleShot;
            counts[ammo] = remaining;
            AmmoChanged?.Invoke(ammo, remaining);
            return true;
        }
    }
}
