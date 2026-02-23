using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace QOLFramework.CustomRoles
{
    public abstract class CustomRole
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract RoleTypeId BaseRole { get; }
        public abstract Team Team { get; }

        public virtual int MaxHealth => -1;
        public virtual float WalkSpeed => -1f;
        public virtual string SpawnMessage => $"<color=#00BFFF>[QOL]</color> Tu és agora <b>{Name}</b>!\n{Description}";
        public virtual float SpawnMessageDuration => 6f;
        public virtual int MaxPlayers => -1;
        public virtual string CustomInfo => null;

        public virtual List<ItemType> SpawnInventory => null;
        public virtual List<RoleAbility> Abilities { get; } = new List<RoleAbility>();

        private readonly HashSet<Player> _trackedPlayers = new HashSet<Player>();
        public IReadOnlyCollection<Player> TrackedPlayers => _trackedPlayers;

        public bool HasPlayer(Player player) => _trackedPlayers.Contains(player);

        /// <summary>Se true, o jogador é teleportado para o spawnpoint do BaseRole. Se false, mantém a posição atual.</summary>
        public virtual bool UseSpawnpoint => false;

        internal bool AssignInternal(Player player)
        {
            if (MaxPlayers > 0 && _trackedPlayers.Count >= MaxPlayers)
                return false;

            var savedPosition = player.Position;
            var flags = UseSpawnpoint
                ? RoleSpawnFlags.UseSpawnpoint
                : RoleSpawnFlags.None;

            player.SetRole(BaseRole, RoleChangeReason.None, flags);
            _trackedPlayers.Add(player);

            MEC.Timing.CallDelayed(0.6f, () =>
            {
                if (player.IsDestroyed) return;

                if (!UseSpawnpoint)
                    player.Position = savedPosition;

                if (MaxHealth > 0)
                {
                    player.MaxHealth = MaxHealth;
                    player.Health = MaxHealth;
                }

                if (CustomInfo != null)
                    player.CustomInfo = CustomInfo;

                if (SpawnInventory != null)
                {
                    player.ClearInventory();
                    foreach (var item in SpawnInventory)
                        player.AddItem(item);
                }

                SetupInventory(player);
                OnAssigned(player);

                foreach (var ability in Abilities)
                    ability.OnAssigned(player);

                if (!string.IsNullOrEmpty(SpawnMessage))
                    player.SendHint(SpawnMessage, SpawnMessageDuration);
            });

            return true;
        }

        internal void RemoveInternal(Player player)
        {
            if (!_trackedPlayers.Remove(player)) return;

            player.CustomInfo = null;

            foreach (var ability in Abilities)
            {
                ability.OnRemoved(player);
                ability.ClearCooldown(player);
            }

            OnRemoved(player);
        }

        internal void TickInternal()
        {
            _trackedPlayers.RemoveWhere(p => p.IsDestroyed);

            foreach (var player in _trackedPlayers)
            {
                try
                {
                    OnTick(player);
                    foreach (var ability in Abilities)
                        ability.OnTick(player);
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Role:{Name}] Tick error: {ex}");
                }
            }
        }

        internal void ClearInternal()
        {
            foreach (var player in _trackedPlayers)
            {
                foreach (var ability in Abilities)
                {
                    ability.OnRemoved(player);
                    ability.ClearCooldown(player);
                }
                OnRemoved(player);
            }
            _trackedPlayers.Clear();
        }

        protected virtual void SetupInventory(Player player) { }
        protected virtual void OnAssigned(Player player) { }
        protected virtual void OnRemoved(Player player) { }
        protected virtual void OnTick(Player player) { }
    }
}
