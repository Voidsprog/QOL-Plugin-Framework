using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;
using QOLFramework.Permissions;

namespace QOLFramework.Extensions
{
    public static class PlayerExtensions
    {
        /// <summary>Verifica se o jogador tem a permissão (via <see cref="PermissionHelper.CheckPermission"/>). Por defeito retorna true.</summary>
        public static bool HasQOLPermission(this Player player, string permission)
        {
            return PermissionHelper.HasPermission(player, permission);
        }

        public static void SendQOLHint(this Player player, string message, float duration = 5f)
        {
            player.SendHint($"<size=22><color=#00BFFF>[QOL]</color> {message}</size>", duration);
        }

        public static void SendColoredHint(this Player player, string message, string color, float duration = 5f)
        {
            player.SendHint($"<size=22><color={color}>{message}</color></size>", duration);
        }

        public static bool IsScp(this Player player)
        {
            return player.Team == Team.SCPs;
        }

        public static bool IsHuman(this Player player)
        {
            return player.Team != Team.SCPs && player.Team != Team.Dead;
        }

        public static bool IsAlive(this Player player)
        {
            return player.Team != Team.Dead;
        }

        public static Player GetClosestPlayer(this Player player, bool sameTeam = false)
        {
            Player closest = null;
            float closestDist = float.MaxValue;

            foreach (var other in Player.List)
            {
                if (other == player || other.Team == Team.Dead) continue;
                if (sameTeam && other.Team != player.Team) continue;

                var dist = Vector3.Distance(player.Position, other.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = other;
                }
            }

            return closest;
        }

        public static IEnumerable<Player> GetNearbyPlayers(this Player player, float radius)
        {
            return Player.List.Where(p =>
                p != player &&
                p.Team != Team.Dead &&
                Vector3.Distance(player.Position, p.Position) <= radius);
        }

        public static float GetDistanceTo(this Player player, Player other)
        {
            return Vector3.Distance(player.Position, other.Position);
        }

        public static float GetDistanceTo(this Player player, Vector3 position)
        {
            return Vector3.Distance(player.Position, position);
        }

        public static void HealToMax(this Player player)
        {
            player.Health = player.MaxHealth;
        }

        public static void HealPercent(this Player player, float percent)
        {
            player.Health = Mathf.Min(player.Health + player.MaxHealth * (percent / 100f), player.MaxHealth);
        }

        public static void DamagePercent(this Player player, float percent)
        {
            player.Health -= player.MaxHealth * (percent / 100f);
        }

        public static void BroadcastToAll(string message, float duration = 5f)
        {
            foreach (var player in Player.List)
                player.SendQOLHint(message, duration);
        }
    }
}
