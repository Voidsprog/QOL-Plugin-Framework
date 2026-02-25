using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Armazena dados temporários por jogador durante uma ronda.
    /// Limpo automaticamente no início de cada ronda e quando o jogador sai.
    /// Usa <see cref="PlayerIdHelper.GetId"/> para identificação consistente.
    /// </summary>
    public static class PlayerDataStore
    {
        private static readonly Dictionary<string, Dictionary<string, object>> _data
            = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>Guarda um valor por jogador e chave.</summary>
        public static void Set<T>(Player player, string key, T value)
        {
            if (player == null) return;
            var userId = PlayerIdHelper.GetId(player);
            if (!_data.ContainsKey(userId))
                _data[userId] = new Dictionary<string, object>();
            _data[userId][key] = value;
        }

        /// <summary>Obtém um valor (ou defaultValue se não existir ou falhar cast).</summary>
        public static T Get<T>(Player player, string key, T defaultValue = default)
        {
            if (player == null) return defaultValue;
            var userId = PlayerIdHelper.GetId(player);
            if (!_data.ContainsKey(userId)) return defaultValue;
            if (!_data[userId].ContainsKey(key)) return defaultValue;
            try { return (T)_data[userId][key]; }
            catch { return defaultValue; }
        }

        /// <summary>Indica se existe valor para a chave.</summary>
        public static bool Has(Player player, string key)
        {
            if (player == null) return false;
            var userId = PlayerIdHelper.GetId(player);
            return _data.ContainsKey(userId) && _data[userId].ContainsKey(key);
        }

        /// <summary>Remove o valor da chave. Retorna true se existia.</summary>
        public static bool Remove(Player player, string key)
        {
            if (player == null) return false;
            var userId = PlayerIdHelper.GetId(player);
            if (!_data.ContainsKey(userId)) return false;
            return _data[userId].Remove(key);
        }

        /// <summary>Remove todos os dados do jogador (chamado ao sair).</summary>
        public static void ClearPlayer(Player player)
        {
            if (player == null) return;
            _data.Remove(PlayerIdHelper.GetId(player));
        }

        /// <summary>Incrementa um inteiro e retorna o novo valor.</summary>
        public static int Increment(Player player, string key, int amount = 1)
        {
            var current = Get(player, key, 0);
            var newVal = current + amount;
            Set(player, key, newVal);
            return newVal;
        }

        /// <summary>Incrementa um float e retorna o novo valor.</summary>
        public static float IncrementFloat(Player player, string key, float amount = 1f)
        {
            var current = Get(player, key, 0f);
            var newVal = current + amount;
            Set(player, key, newVal);
            return newVal;
        }

        /// <summary>Alterna um bool e retorna o novo valor.</summary>
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
    }
}
