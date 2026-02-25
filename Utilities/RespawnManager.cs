using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Controlo de respawn customizado. Permite forçar respawn de jogadores
    /// com roles, posições e delays configuráveis.
    /// </summary>
    public static class RespawnManager
    {
        private static readonly List<PendingRespawn> _pendingRespawns = new List<PendingRespawn>();

        public static void RespawnAs(Player player, RoleTypeId role, float delay = 0f)
        {
            if (player == null || player.IsDestroyed) return;

            if (delay <= 0f)
            {
                SetRole(player, role);
                return;
            }

            var pending = new PendingRespawn
            {
                PlayerId = PlayerIdHelper.GetId(player),
                Role = role,
                ScheduledTime = DateTime.UtcNow.AddSeconds(delay)
            };
            _pendingRespawns.Add(pending);

            Timing.CallDelayed(delay, () =>
            {
                _pendingRespawns.Remove(pending);
                var p = FindPlayer(pending.PlayerId);
                if (p != null && !p.IsDestroyed)
                    SetRole(p, role);
            });
        }

        public static void RespawnAsWithLoadout(Player player, RoleTypeId role, IEnumerable<ItemType> items, float delay = 0f)
        {
            if (player == null || player.IsDestroyed) return;

            void DoRespawn()
            {
                SetRole(player, role);
                Timing.CallDelayed(0.5f, () =>
                {
                    if (player == null || player.IsDestroyed) return;
                    InventoryHelper.SetLoadout(player, items);
                });
            }

            if (delay <= 0f)
                DoRespawn();
            else
                Timing.CallDelayed(delay, DoRespawn);
        }

        public static void RespawnAllDead(RoleTypeId role, float delay = 0f)
        {
            if (Player.List == null) return;
            var dead = Player.List.Where(p =>
                p != null && !p.IsDestroyed && p.Team == Team.Dead).ToList();

            foreach (var player in dead)
                RespawnAs(player, role, delay);
        }

        public static bool CancelPendingRespawn(Player player)
        {
            if (player == null) return false;
            var id = PlayerIdHelper.GetId(player);
            return _pendingRespawns.RemoveAll(p => p.PlayerId == id) > 0;
        }

        public static bool HasPendingRespawn(Player player)
        {
            if (player == null) return false;
            var id = PlayerIdHelper.GetId(player);
            return _pendingRespawns.Any(p => p.PlayerId == id);
        }

        /// <summary>Limpa todos os respawns pendentes (chamar no início da ronda).</summary>
        internal static void ClearAll()
        {
            _pendingRespawns.Clear();
        }

        private static void SetRole(Player player, RoleTypeId role)
        {
            try { player?.SetRole(role); }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error($"[QOL:Respawn] Failed to set role: {ex.Message}");
            }
        }

        private static Player FindPlayer(string playerId)
        {
            if (Player.List == null) return null;
            return Player.List.FirstOrDefault(p =>
            {
                try { return p != null && PlayerIdHelper.GetId(p) == playerId; }
                catch { return false; }
            });
        }

        private class PendingRespawn
        {
            public string PlayerId;
            public RoleTypeId Role;
            public DateTime ScheduledTime;
        }
    }
}
