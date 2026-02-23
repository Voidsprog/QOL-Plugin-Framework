using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Utilitários de gestão de inventário para jogadores.
    /// </summary>
    public static class InventoryHelper
    {
        public static bool HasItem(Player player, ItemType itemType)
        {
            return player?.Items?.Any(i => i.Type == itemType) ?? false;
        }

        public static int CountItem(Player player, ItemType itemType)
        {
            return player?.Items?.Count(i => i.Type == itemType) ?? 0;
        }

        public static bool GiveItem(Player player, ItemType itemType)
        {
            if (player == null || player.IsDestroyed) return false;
            try
            {
                player.AddItem(itemType);
                return true;
            }
            catch { return false; }
        }

        public static int GiveItems(Player player, params ItemType[] items)
        {
            int given = 0;
            foreach (var item in items)
            {
                if (GiveItem(player, item))
                    given++;
            }
            return given;
        }

        public static int GiveItems(Player player, IEnumerable<ItemType> items)
        {
            return GiveItems(player, items.ToArray());
        }

        public static bool RemoveItem(Player player, ItemType itemType)
        {
            if (player == null || player.IsDestroyed) return false;
            try
            {
                var item = player.Items?.FirstOrDefault(i => i.Type == itemType);
                if (item == null) return false;
                player.RemoveItem(item);
                return true;
            }
            catch { return false; }
        }

        public static int RemoveAllOfType(Player player, ItemType itemType)
        {
            if (player == null || player.IsDestroyed) return 0;
            int removed = 0;
            try
            {
                foreach (var item in player.Items.Where(i => i.Type == itemType).ToList())
                {
                    player.RemoveItem(item);
                    removed++;
                }
            }
            catch { }
            return removed;
        }

        public static void ClearInventory(Player player)
        {
            if (player == null || player.IsDestroyed) return;
            try { player.ClearInventory(); }
            catch { }
        }

        public static void SetLoadout(Player player, params ItemType[] items)
        {
            ClearInventory(player);
            GiveItems(player, items);
        }

        public static void SetLoadout(Player player, IEnumerable<ItemType> items)
        {
            SetLoadout(player, items.ToArray());
        }

        public static bool IsInventoryFull(Player player, int maxSlots = 8)
        {
            return (player?.Items?.Count() ?? 0) >= maxSlots;
        }

        public static IEnumerable<ItemType> GetItemTypes(Player player)
        {
            return player?.Items?.Select(i => i.Type) ?? Enumerable.Empty<ItemType>();
        }

        public static bool HasKeycard(Player player)
        {
            if (player?.Items == null) return false;
            return player.Items.Any(i =>
                i.Type == ItemType.KeycardJanitor ||
                i.Type == ItemType.KeycardScientist ||
                i.Type == ItemType.KeycardResearchCoordinator ||
                i.Type == ItemType.KeycardZoneManager ||
                i.Type == ItemType.KeycardGuard ||
                i.Type == ItemType.KeycardMTFPrivate ||
                i.Type == ItemType.KeycardContainmentEngineer ||
                i.Type == ItemType.KeycardMTFOperative ||
                i.Type == ItemType.KeycardMTFCaptain ||
                i.Type == ItemType.KeycardFacilityManager ||
                i.Type == ItemType.KeycardChaosInsurgency ||
                i.Type == ItemType.KeycardO5);
        }

        public static bool HasWeapon(Player player)
        {
            if (player?.Items == null) return false;
            return player.Items.Any(i =>
                i.Type == ItemType.GunCOM15 ||
                i.Type == ItemType.GunCOM18 ||
                i.Type == ItemType.GunRevolver ||
                i.Type == ItemType.GunE11SR ||
                i.Type == ItemType.GunCrossvec ||
                i.Type == ItemType.GunAK ||
                i.Type == ItemType.GunShotgun ||
                i.Type == ItemType.GunLogicer ||
                i.Type == ItemType.GunFSP9 ||
                i.Type == ItemType.GunA7 ||
                i.Type == ItemType.MicroHID ||
                i.Type == ItemType.ParticleDisruptor ||
                i.Type == ItemType.Jailbird);
        }

        public static bool HasMedical(Player player)
        {
            if (player?.Items == null) return false;
            return player.Items.Any(i =>
                i.Type == ItemType.Medkit ||
                i.Type == ItemType.Painkillers ||
                i.Type == ItemType.Adrenaline ||
                i.Type == ItemType.SCP500);
        }
    }
}
