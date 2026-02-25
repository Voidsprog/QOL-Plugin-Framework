using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Sistema de modificadores de dano. Permite registar multiplicadores
    /// e flat bonuses por jogador ou globais, que são aplicados quando
    /// um módulo calcula dano customizado.
    /// </summary>
    public static class DamageModifierSystem
    {
        private static readonly List<DamageModifier> _globalModifiers = new List<DamageModifier>();
        private static readonly Dictionary<string, List<DamageModifier>> _playerModifiers
            = new Dictionary<string, List<DamageModifier>>();

        public static string AddGlobalModifier(string name, float multiplier = 1f, float flatBonus = 0f, int priority = 0)
        {
            var id = Guid.NewGuid().ToString("N").Substring(0, 8);
            _globalModifiers.Add(new DamageModifier
            {
                Id = id,
                Name = name,
                Multiplier = multiplier,
                FlatBonus = flatBonus,
                Priority = priority
            });
            _globalModifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return id;
        }

        public static string AddPlayerModifier(Player player, string name, float multiplier = 1f, float flatBonus = 0f, int priority = 0)
        {
            if (player == null) return null;
            var userId = PlayerIdHelper.GetId(player);
            if (!_playerModifiers.ContainsKey(userId))
                _playerModifiers[userId] = new List<DamageModifier>();

            var id = Guid.NewGuid().ToString("N").Substring(0, 8);
            _playerModifiers[userId].Add(new DamageModifier
            {
                Id = id,
                Name = name,
                Multiplier = multiplier,
                FlatBonus = flatBonus,
                Priority = priority
            });
            _playerModifiers[userId].Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return id;
        }

        public static bool RemoveGlobalModifier(string id)
        {
            return _globalModifiers.RemoveAll(m => m.Id == id) > 0;
        }

        public static bool RemovePlayerModifier(Player player, string id)
        {
            if (player == null) return false;
            var userId = PlayerIdHelper.GetId(player);
            if (!_playerModifiers.ContainsKey(userId)) return false;
            return _playerModifiers[userId].RemoveAll(m => m.Id == id) > 0;
        }

        public static void ClearPlayerModifiers(Player player)
        {
            if (player == null) return;
            _playerModifiers.Remove(PlayerIdHelper.GetId(player));
        }

        /// <summary>
        /// Calcula o dano final aplicando todos os modificadores (globais + do jogador).
        /// </summary>
        public static float CalculateDamage(float baseDamage, Player target)
        {
            float result = baseDamage;

            foreach (var mod in _globalModifiers)
                result = result * mod.Multiplier + mod.FlatBonus;

            if (target != null)
            {
                var userId = PlayerIdHelper.GetId(target);
                if (_playerModifiers.ContainsKey(userId))
                {
                    foreach (var mod in _playerModifiers[userId])
                        result = result * mod.Multiplier + mod.FlatBonus;
                }
            }

            return Math.Max(0f, result);
        }

        /// <summary>Limpa todos os modificadores (chamar no início da ronda).</summary>
        internal static void ClearAll()
        {
            _globalModifiers.Clear();
            _playerModifiers.Clear();
        }
    }

    public class DamageModifier
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Multiplier { get; set; } = 1f;
        public float FlatBonus { get; set; }
        public int Priority { get; set; }
    }
}
