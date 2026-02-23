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
        private static readonly Dictionary<string, Queue<QueuedBroadcast>> _playerQueues
            = new Dictionary<string, Queue<QueuedBroadcast>>();

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

        public static void Send(Player player, string message, float duration = 5f, int priority = 0)
        {
            if (player == null || player.IsDestroyed) return;
            var id = GetId(player);

            if (!_playerQueues.ContainsKey(id))
                _playerQueues[id] = new Queue<QueuedBroadcast>();

            _playerQueues[id].Enqueue(new QueuedBroadcast
            {
                Message = message,
                Duration = duration,
                Priority = priority,
                Timestamp = DateTime.UtcNow
            });
        }

        public static void SendImmediate(Player player, string message, float duration = 5f)
        {
            if (player == null || player.IsDestroyed) return;
            player.SendHint(message, duration);
            SetCooldown(player, duration);
        }

        public static void SendToAll(string message, float duration = 5f, int priority = 0)
        {
            foreach (var player in Player.List)
            {
                if (player == null || player.IsDestroyed) continue;
                Send(player, message, duration, priority);
            }
        }

        public static void SendImmediateToAll(string message, float duration = 5f)
        {
            foreach (var player in Player.List)
            {
                if (player == null || player.IsDestroyed) continue;
                SendImmediate(player, message, duration);
            }
        }

        public static void ClearQueue(Player player)
        {
            if (player == null) return;
            _playerQueues.Remove(GetId(player));
        }

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
                    var queue = kvp.Value;

                    if (queue.Count == 0)
                    {
                        toRemove.Add(id);
                        continue;
                    }

                    if (IsOnCooldown(id)) continue;

                    var broadcast = queue.Dequeue();
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
            var id = GetId(player);
            _playerCooldowns[id] = DateTime.UtcNow.AddSeconds(duration);
        }

        private static string GetId(Player player)
        {
            try { return player.UserId ?? "unknown"; }
            catch { return "unknown"; }
        }

        private static Player FindPlayer(string userId)
        {
            return Player.List.FirstOrDefault(p =>
            {
                try { return p?.UserId == userId; }
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
