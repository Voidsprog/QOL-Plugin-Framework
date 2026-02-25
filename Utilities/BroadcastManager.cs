using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MEC;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Sistema de broadcasts (hints) com prioridade e filas.
    /// Evita sobreposição de mensagens e garante que mensagens importantes são vistas.
    /// </summary>
    public static class BroadcastManager
    {
        private static readonly Dictionary<string, List<QueuedBroadcast>> _playerQueues
            = new Dictionary<string, List<QueuedBroadcast>>();

        private static readonly Dictionary<string, DateTime> _playerCooldowns
            = new Dictionary<string, DateTime>();

        private static CoroutineHandle _processHandle;
        private static bool _running;

        internal static void Initialize()
        {
            if (_running) return;
            _running = true;
            _processHandle = Timing.RunCoroutine(ProcessLoop());
        }

        internal static void Shutdown()
        {
            _running = false;
            Timing.KillCoroutines(_processHandle);
            _playerQueues.Clear();
            _playerCooldowns.Clear();
        }

        /// <summary>Enfileira um hint para o jogador (maior priority é enviado primeiro).</summary>
        public static void Send(Player player, string message, float duration = 5f, int priority = 0)
        {
            if (player == null || player.IsDestroyed) return;
            var id = PlayerIdHelper.GetId(player);

            if (!_playerQueues.ContainsKey(id))
                _playerQueues[id] = new List<QueuedBroadcast>();

            _playerQueues[id].Add(new QueuedBroadcast
            {
                Message = message,
                Duration = duration,
                Priority = priority,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>Envia o hint imediatamente e aplica cooldown.</summary>
        public static void SendImmediate(Player player, string message, float duration = 5f)
        {
            if (player == null || player.IsDestroyed) return;
            player.SendHint(message, duration);
            SetCooldown(player, duration);
        }

        /// <summary>Enfileira o mesmo hint para todos os jogadores válidos.</summary>
        public static void SendToAll(string message, float duration = 5f, int priority = 0)
        {
            if (Player.List == null) return;
            foreach (var player in Player.List)
            {
                if (player == null || player.IsDestroyed) continue;
                Send(player, message, duration, priority);
            }
        }

        public static void SendImmediateToAll(string message, float duration = 5f)
        {
            if (Player.List == null) return;
            foreach (var player in Player.List)
            {
                if (player == null || player.IsDestroyed) continue;
                SendImmediate(player, message, duration);
            }
        }

        /// <summary>Remove todos os hints em fila para o jogador.</summary>
        public static void ClearQueue(Player player)
        {
            if (player == null) return;
            _playerQueues.Remove(PlayerIdHelper.GetId(player));
        }

        /// <summary>Limpa todas as filas e cooldowns.</summary>
        public static void ClearAllQueues()
        {
            _playerQueues.Clear();
            _playerCooldowns.Clear();
        }

        private static IEnumerator<float> ProcessLoop()
        {
            while (_running)
            {
                yield return Timing.WaitForSeconds(0.5f);

                var toRemove = new List<string>();

                foreach (var kvp in _playerQueues.ToList())
                {
                    var id = kvp.Key;
                    var list = kvp.Value;
                    if (list == null || list.Count == 0)
                    {
                        toRemove.Add(id);
                        continue;
                    }

                    if (IsOnCooldown(id)) continue;

                    list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                    var broadcast = list[0];
                    list.RemoveAt(0);
                    var player = FindPlayer(id);
                    if (player == null || player.IsDestroyed)
                    {
                        toRemove.Add(id);
                        continue;
                    }

                    try
                    {
                        player.SendHint(broadcast.Message, broadcast.Duration);
                        _playerCooldowns[id] = DateTime.UtcNow.AddSeconds(broadcast.Duration);
                    }
                    catch { }
                }

                foreach (var id in toRemove)
                    _playerQueues.Remove(id);
            }
        }

        private static bool IsOnCooldown(string playerId)
        {
            if (!_playerCooldowns.ContainsKey(playerId)) return false;
            return DateTime.UtcNow < _playerCooldowns[playerId];
        }

        private static void SetCooldown(Player player, float duration)
        {
            var id = PlayerIdHelper.GetId(player);
            _playerCooldowns[id] = DateTime.UtcNow.AddSeconds(duration);
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

        private class QueuedBroadcast
        {
            public string Message;
            public float Duration;
            public int Priority;
            public DateTime Timestamp;
        }
    }
}
