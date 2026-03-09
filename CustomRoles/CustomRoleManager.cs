using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;

namespace QOLFramework.CustomRoles
{
    public class CustomRoleManager
    {
        private readonly Dictionary<int, CustomRole> _roles = new Dictionary<int, CustomRole>();
        private CoroutineHandle _tickCoroutine;
        private bool _isRunning;

        public IReadOnlyDictionary<int, CustomRole> Roles => _roles;

        public void Initialize()
        {
            if (_isRunning) return;
            _isRunning = true;

            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Left += OnPlayerLeft;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;

            _tickCoroutine = Timing.RunCoroutine(TickCoroutine());
        }

        public void Shutdown()
        {
            if (!_isRunning) return;

            PlayerEvents.ChangedRole -= OnChangedRole;
            PlayerEvents.Left -= OnPlayerLeft;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;

            Timing.KillCoroutines(_tickCoroutine);
            ClearAll();
            _isRunning = false;
        }

        public void RegisterRole(CustomRole role)
        {
            if (_roles.ContainsKey(role.Id))
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Roles] Role with ID {role.Id} ('{role.Name}') is already registered.");
                return;
            }

            _roles[role.Id] = role;
            LabApi.Features.Console.Logger.Info($"[QOL:Roles] Registered custom role '{role.Name}' (ID: {role.Id})");
        }

        public void UnregisterRole(int id)
        {
            if (_roles.TryGetValue(id, out var role))
            {
                role.ClearInternal();
                _roles.Remove(id);
            }
        }

        public bool AssignRole(Player player, int roleId)
        {
            if (!_roles.TryGetValue(roleId, out var role))
                return false;

            RemoveFromAllRoles(player);
            return role.AssignInternal(player);
        }

        public bool AssignRole<T>(Player player) where T : CustomRole
        {
            var role = _roles.Values.FirstOrDefault(r => r is T);
            if (role == null) return false;

            RemoveFromAllRoles(player);
            return role.AssignInternal(player);
        }

        public CustomRole GetPlayerRole(Player player)
        {
            return _roles.Values.FirstOrDefault(r => r.HasPlayer(player));
        }

        public T GetRole<T>() where T : CustomRole
        {
            return _roles.Values.FirstOrDefault(r => r is T) as T;
        }

        public CustomRole GetRole(int id)
        {
            return _roles.TryGetValue(id, out var role) ? role : null;
        }

        public bool HasCustomRole(Player player)
        {
            return _roles.Values.Any(r => r.HasPlayer(player));
        }

        public void RemoveFromAllRoles(Player player)
        {
            foreach (var role in _roles.Values)
                role.RemoveInternal(player);
        }

        public List<Player> GetPlayersWithRole(int roleId)
        {
            if (!_roles.TryGetValue(roleId, out var role))
                return new List<Player>();
            return role.TrackedPlayers.ToList();
        }

        private void ClearAll()
        {
            foreach (var role in _roles.Values)
                role.ClearInternal();
        }

        private void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev?.Player == null) return;
            var customRole = GetPlayerRole(ev.Player);
            if (customRole != null && ev.NewRole?.RoleTypeId != customRole.BaseRole)
            {
                customRole.RemoveInternal(ev.Player);
            }
            if (ev.Player.Team == Team.Dead)
            {
                try { RemoveFromAllRoles(ev.Player); } catch (ArgumentNullException) { }
            }
        }

        private void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (ev?.Player == null) return;
            try { RemoveFromAllRoles(ev.Player); } catch (ArgumentNullException) { }
        }

        private void OnWaitingForPlayers()
        {
            ClearAll();
        }

        private IEnumerator<float> TickCoroutine()
        {
            while (_isRunning)
            {
                yield return Timing.WaitForSeconds(0.5f);

                foreach (var role in _roles.Values)
                {
                    try { role.TickInternal(); }
                    catch (Exception ex)
                    {
                        LabApi.Features.Console.Logger.Error($"[QOL:Roles] Tick error for '{role.Name}': {ex}");
                    }
                }
            }
        }
    }
}
