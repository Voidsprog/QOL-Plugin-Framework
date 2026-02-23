using System.Collections.Generic;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace QOLFramework.CustomSCPs
{
    public abstract class CustomScp : CustomRoles.CustomRole
    {
        public override Team Team => Team.SCPs;

        public abstract string ScpNumber { get; }

        public virtual int BaseHealth => 3000;
        public override int MaxHealth => BaseHealth;

        public virtual float ArtificialHealthMax => 0f;

        public override string SpawnMessage =>
            $"<color=#FF0000>[SCP-{ScpNumber}]</color>\nTu és <b>{Name}</b>!\n<size=18>{Description}</size>";

        public override string CustomInfo => $"SCP-{ScpNumber}";

        public virtual List<ScpAbility> ScpAbilities { get; } = new List<ScpAbility>();

        protected override void OnAssigned(Player player)
        {
            base.OnAssigned(player);

            if (ArtificialHealthMax > 0)
                player.ArtificialHealth = ArtificialHealthMax;

            foreach (var ability in ScpAbilities)
                ability.OnAssigned(player);

            OnScpSpawned(player);
        }

        protected override void OnRemoved(Player player)
        {
            foreach (var ability in ScpAbilities)
            {
                ability.OnRemoved(player);
                ability.ClearCooldown(player);
            }

            OnScpDespawned(player);
            base.OnRemoved(player);
        }

        protected override void OnTick(Player player)
        {
            base.OnTick(player);

            foreach (var ability in ScpAbilities)
                ability.OnTick(player);

            OnScpTick(player);
        }

        protected virtual void OnScpSpawned(Player player) { }
        protected virtual void OnScpDespawned(Player player) { }
        protected virtual void OnScpTick(Player player) { }
    }
}
