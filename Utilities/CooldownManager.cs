using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Sistema centralizado de cooldowns para habilidades, itens, ações, etc.
    /// Suporta cooldowns globais e por jogador.
    /// </summary>
    public static class CooldownManager
    {
        private static readonly Dictionary<string, DateTime> _globalCooldowns = new Dictionary<string, DateTime>();
        private static readonly Dictionary<string, Dictionary<string, DateTime>> _playerCooldowns
            = new Dictionary<string, Dictionary<string, DateTime>>();

        public static bool IsOnCooldown(string key)
        {
            if (!_globalCooldowns.ContainsKey(key)) return false;
            if (DateTime.UtcNow >= _globalCooldowns[key])
            {
                _globalCooldowns.Remove(key);
                return false;
            }
            return true;
        }

        public static bool IsOnCooldown(Player player, string key)
        {
            if (player == null) return false;
            var id = GetId(player);
            if (!_playerCooldowns.ContainsKey(id)) return false;
            if (!_playerCooldowns[id].ContainsKey(key)) return false;
            if (DateTime.UtcNow >= _playerCooldowns[id][key])
            {
                _playerCooldowns[id].Remove(key);
                return false;
            }
            return true;
        }

        public static void SetCooldown(string key, float seconds)
        {
            _globalCooldowns[key] = DateTime.UtcNow.AddSeconds(seconds);
        }

        public static void SetCooldown(Player player, string key, float seconds)
        {
            if (player == null) return;
            var id = GetId(player);
            if (!_playerCooldowns.ContainsKey(id))
                _playerCooldowns[id] = new Dictionary<string, DateTime>();
            _playerCooldowns[id][key] = DateTime.UtcNow.AddSeconds(seconds);
        }

        /// <summary>Tenta executar a ação se não estiver em cooldown. Retorna true se executou.</summary>
        public static bool TryUse(string key, float cooldownSeconds, Action action)
        {
            if (IsOnCooldown(key)) return false;
            SetCooldown(key, cooldownSeconds);
            action();
            return true;
        }

        /// <summary>Tenta executar a ação se o jogador não estiver em cooldown. Retorna true se executou.</summary>
        public static bool TryUse(Player player, string key, float cooldownSeconds, Action action)
        {
            if (IsOnCooldown(player, key)) return false;
            SetCooldown(player, key, cooldownSeconds);
            action();
            return true;
        }

        public static float GetRemainingCooldown(string key)
        {
            if (!_globalCooldowns.ContainsKey(key)) return 0f;
            var remaining = (float)(_globalCooldowns[key] - DateTime.UtcNow).TotalSeconds;
            return remaining > 0 ? remaining : 0f;
        }

        public static float GetRemainingCooldown(Player player, string key)
        {
            if (player == null) return 0f;
            var id = GetId(player);
            if (!_playerCooldowns.ContainsKey(id)) return 0f;
            if (!_playerCooldowns[id].ContainsKey(key)) return 0f;
            var remaining = (float)(_playerCooldowns[id][key] - DateTime.UtcNow).TotalSeconds;
            return remaining > 0 ? remaining : 0f;
        }

        public static void RemoveCooldown(string key)
        {
            _globalCooldowns.Remove(key);
        }

        public static void RemoveCooldown(Player player, string key)
        {
            if (player == null) return;
            var id = GetId(player);
            if (_playerCooldowns.ContainsKey(id))
                _playerCooldowns[id].Remove(key);
        }

        public static void ClearPlayer(Player player)
        {
            if (player == null) return;
            _playerCooldowns.Remove(GetId(player));
        }

        internal static void ClearAll()
        {
            _globalCooldowns.Clear();
            _playerCooldowns.Clear();
        }

        private static string GetId(Player player)
        {
            try { return player.UserId ?? "unknown"; }
            catch { return "unknown"; }
        }
    }
}
