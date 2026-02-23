using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace QOLFramework.Utilities
{
    public static class RoundUtilities
    {
        private static DateTime _roundStartTime;
        private static bool _roundActive;

        public static bool IsRoundActive => _roundActive;
        public static TimeSpan RoundElapsed => _roundActive ? DateTime.UtcNow - _roundStartTime : TimeSpan.Zero;
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

        public static bool HasElapsed(int seconds) => RoundElapsedSeconds >= seconds;
        public static bool HasElapsed(TimeSpan time) => RoundElapsed >= time;

        public static int GetAlivePlayerCount()
        {
            return Player.List.Count(p =>
                p != null && !p.IsDestroyed && p.Team != Team.Dead);
        }

        public static int GetAliveHumanCount()
        {
            return Player.List.Count(p =>
                p != null && !p.IsDestroyed &&
                p.Team != Team.Dead && p.Team != Team.SCPs);
        }

        public static int GetAliveScpCount()
        {
            return Player.List.Count(p =>
                p != null && !p.IsDestroyed && p.Team == Team.SCPs);
        }

        public static IEnumerable<Player> GetAlivePlayers()
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed && p.Team != Team.Dead);
        }

        public static IEnumerable<Player> GetAliveHumans()
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != Team.Dead && p.Team != Team.SCPs);
        }

        public static IEnumerable<Player> GetAliveScps()
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed && p.Team == Team.SCPs);
        }

        public static IEnumerable<Player> GetPlayersByTeam(Team team)
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed && p.Team == team);
        }

        public static IEnumerable<Player> GetPlayersByRole(RoleTypeId role)
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed && p.Role == role);
        }

        public static Player GetRandomAlivePlayer(Team? team = null)
        {
            var candidates = (team.HasValue ? GetPlayersByTeam(team.Value) : GetAlivePlayers()).ToList();
            return candidates.Count == 0 ? null : candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        public static Dictionary<Team, int> GetTeamCounts()
        {
            var counts = new Dictionary<Team, int>();
            foreach (var p in Player.List)
            {
                if (p == null || p.IsDestroyed) continue;
                if (!counts.ContainsKey(p.Team))
                    counts[p.Team] = 0;
                counts[p.Team]++;
            }
            return counts;
        }

        public static string FormatElapsed()
        {
            var elapsed = RoundElapsed;
            return elapsed.TotalHours >= 1
                ? $"{(int)elapsed.TotalHours}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
                : $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }
    }
}
