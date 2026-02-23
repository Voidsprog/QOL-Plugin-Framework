using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Armazena dados temporários por jogador durante uma ronda.
    /// Limpo automaticamente no início de cada ronda.
    /// </summary>
    public static class PlayerDataStore
    {
        private static readonly Dictionary<string, Dictionary<string, object>> _data
            = new Dictionary<string, Dictionary<string, object>>();

        public static void Set<T>(Player player, string key, T value)
        {
            if (player == null) return;
            var userId = GetId(player);
            if (!_data.ContainsKey(userId))
                _data[userId] = new Dictionary<string, object>();
            _data[userId][key] = value;
        }

        public static T Get<T>(Player player, string key, T defaultValue = default)
        {
            if (player == null) return defaultValue;
            var userId = GetId(player);
            if (!_data.ContainsKey(userId)) return defaultValue;
            if (!_data[userId].ContainsKey(key)) return defaultValue;
            try { return (T)_data[userId][key]; }
            catch { return defaultValue; }
        }

        public static bool Has(Player player, string key)
        {
            if (player == null) return false;
            var userId = GetId(player);
            return _data.ContainsKey(userId) && _data[userId].ContainsKey(key);
        }

        public static bool Remove(Player player, string key)
        {
            if (player == null) return false;
            var userId = GetId(player);
            if (!_data.ContainsKey(userId)) return false;
            return _data[userId].Remove(key);
        }

        public static void ClearPlayer(Player player)
        {
            if (player == null) return;
            _data.Remove(GetId(player));
        }

        public static int Increment(Player player, string key, int amount = 1)
        {
            var current = Get(player, key, 0);
            var newVal = current + amount;
            Set(player, key, newVal);
            return newVal;
        }

        public static float IncrementFloat(Player player, string key, float amount = 1f)
        {
            var current = Get(player, key, 0f);
            var newVal = current + amount;
            Set(player, key, newVal);
            return newVal;
        }

        public static bool Toggle(Player player, string key)
        {
            var current = Get(player, key, false);
            var newVal = !current;
            Set(player, key, newVal);
            return newVal;
        }

        /// <summary>Limpa todos os dados (chamar no início da ronda).</summary>
        internal static void ClearAll()
        {
            _data.Clear();
        }

        private static string GetId(Player player)
        {
            try { return player.UserId ?? player.ReferenceHub?.GetInstanceID().ToString() ?? "unknown"; }
            catch { return "unknown"; }
        }
    }
}
