using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;

namespace QOLFramework.Tweaks
{
    public abstract class ScpParameterTweak : TweakBase
    {
        public override string Category => "SCP";

        public abstract RoleTypeId TargetScp { get; }

        public virtual float? HealthOverride => null;
        public virtual float? AhpOverride => null;
        public virtual float? WalkSpeedMultiplier => null;

        public virtual float TickInterval => 1f;

        private CoroutineHandle _coroutine;
        private readonly HashSet<Player> _affectedPlayers = new HashSet<Player>();

        protected override void OnApply()
        {
            PlayerEvents.ChangedRole += OnRoleChanged;
            PlayerEvents.Left += OnPlayerLeft;
            ServerEvents.WaitingForPlayers += OnWaiting;

            _coroutine = Timing.RunCoroutine(ParameterTickCoroutine());

            foreach (var player in Player.List)
            {
                if (player.Role == TargetScp)
                    ApplyToPlayer(player);
            }
        }

        protected override void OnRevert()
        {
            PlayerEvents.ChangedRole -= OnRoleChanged;
            PlayerEvents.Left -= OnPlayerLeft;
            ServerEvents.WaitingForPlayers -= OnWaiting;

            Timing.KillCoroutines(_coroutine);
            _affectedPlayers.Clear();
        }

        private void OnRoleChanged(PlayerChangedRoleEventArgs ev)
        {
            try
            {
                if (ev?.Player == null) return;
                if (ev.NewRole?.RoleTypeId == TargetScp)
                {
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (ev.Player != null && !ev.Player.IsDestroyed && ev.Player.Role == TargetScp)
                            ApplyToPlayer(ev.Player);
                    });
                }
                else
                    _affectedPlayers.Remove(ev.Player);
            }
            catch (Exception) { /* evitar propagar */ }
        }

        private void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            try
            {
                if (ev?.Player == null) return;
                _affectedPlayers.Remove(ev.Player);
            }
            catch (Exception) { /* evitar propagar; ev.Player null pode causar falha noutros plugins */ }
        }

        private void OnWaiting()
        {
            _affectedPlayers.Clear();
        }

        private void ApplyToPlayer(Player player)
        {
            _affectedPlayers.Add(player);

            if (HealthOverride.HasValue)
            {
                player.MaxHealth = (int)HealthOverride.Value;
                player.Health = HealthOverride.Value;
            }

            OnApplyToPlayer(player);
        }

        private IEnumerator<float> ParameterTickCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(TickInterval);

                _affectedPlayers.RemoveWhere(p => p.IsDestroyed || p.Role != TargetScp);

                foreach (var player in _affectedPlayers)
                {
                    try
                    {
                        OnTickPlayer(player);
                    }
                    catch (Exception ex)
                    {
                        LabApi.Features.Console.Logger.Error($"[QOL:Tweak:{Name}] Tick error: {ex}");
                    }
                }
            }
        }

        protected virtual void OnApplyToPlayer(Player player) { }
        protected virtual void OnTickPlayer(Player player) { }
    }
}
