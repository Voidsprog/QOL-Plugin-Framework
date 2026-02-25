using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace QOLFramework.Utilities
{
    /// <summary>Utilitários de ronda: tempo decorrido, contagens de jogadores por equipa/role.</summary>
    public static class RoundUtilities
    {
        private static DateTime _roundStartTime;
        private static bool _roundActive;

        /// <summary>True se a ronda já começou e ainda não terminou.</summary>
        public static bool IsRoundActive => _roundActive;
        /// <summary>Tempo decorrido desde o início da ronda (Zero se não ativa).</summary>
        public static TimeSpan RoundElapsed => _roundActive ? DateTime.UtcNow - _roundStartTime : TimeSpan.Zero;
        /// <summary>Segundos decorridos desde o início da ronda.</summary>
        public static int RoundElapsedSeconds => (int)RoundElapsed.TotalSeconds;

        internal static void OnRoundStarted()
        {
            _roundStartTime = DateTime.UtcNow;
            _roundActive = true;
        }

        internal static void OnRoundEnded()
        {
            _roundActive = false;
        }

        internal static void OnWaitingForPlayers()
        {
            _roundActive = false;
            _roundStartTime = DateTime.UtcNow;
        }

        /// <summary>True se já passaram pelo menos os segundos indicados desde o início da ronda.</summary>
        public static bool HasElapsed(int seconds) => RoundElapsedSeconds >= seconds;
        /// <summary>True se o tempo indicado já passou desde o início da ronda.</summary>
        public static bool HasElapsed(TimeSpan time) => RoundElapsed >= time;

        /// <summary>Número de jogadores vivos (0 se Player.List for null).</summary>
        public static int GetAlivePlayerCount()
        {
            return Player.List?.Count(p =>
                p != null && !p.IsDestroyed && p.Team != Team.Dead) ?? 0;
        }

        public static int GetAliveHumanCount()
        {
            return Player.List?.Count(p =>
                p != null && !p.IsDestroyed &&
                p.Team != Team.Dead && p.Team != Team.SCPs) ?? 0;
        }

        public static int GetAliveScpCount()
        {
            return Player.List?.Count(p =>
                p != null && !p.IsDestroyed && p.Team == Team.SCPs) ?? 0;
        }

        public static IEnumerable<Player> GetAlivePlayers()
        {
            return Player.List?.Where(p =>
                p != null && !p.IsDestroyed && p.Team != Team.Dead) ?? Enumerable.Empty<Player>();
        }

        public static IEnumerable<Player> GetAliveHumans()
        {
            return Player.List?.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != Team.Dead && p.Team != Team.SCPs) ?? Enumerable.Empty<Player>();
        }

        public static IEnumerable<Player> GetAliveScps()
        {
            return Player.List?.Where(p =>
                p != null && !p.IsDestroyed && p.Team == Team.SCPs) ?? Enumerable.Empty<Player>();
        }

        public static IEnumerable<Player> GetPlayersByTeam(Team team)
        {
            return Player.List?.Where(p =>
                p != null && !p.IsDestroyed && p.Team == team) ?? Enumerable.Empty<Player>();
        }

        public static IEnumerable<Player> GetPlayersByRole(RoleTypeId role)
        {
            return Player.List?.Where(p =>
                p != null && !p.IsDestroyed && p.Role == role) ?? Enumerable.Empty<Player>();
        }

        public static Player GetRandomAlivePlayer(Team? team = null)
        {
            var candidates = (team.HasValue ? GetPlayersByTeam(team.Value) : GetAlivePlayers()).ToList();
            return candidates.Count == 0 ? null : candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        public static Dictionary<Team, int> GetTeamCounts()
        {
            var counts = new Dictionary<Team, int>();
            if (Player.List == null) return counts;
            foreach (var p in Player.List)
            {
                if (p == null || p.IsDestroyed) continue;
                if (!counts.ContainsKey(p.Team))
                    counts[p.Team] = 0;
                counts[p.Team]++;
            }
            return counts;
        }

        /// <summary>Formata o tempo decorrido (ex.: "05:30" ou "1:02:15").</summary>
        public static string FormatElapsed()
        {
            var elapsed = RoundElapsed;
            return elapsed.TotalHours >= 1
                ? $"{(int)elapsed.TotalHours}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
                : $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }
    }
}
