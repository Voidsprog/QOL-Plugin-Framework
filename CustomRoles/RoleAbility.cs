using LabApi.Features.Wrappers;

namespace QOLFramework.CustomRoles
{
    public abstract class RoleAbility
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual float Cooldown => 0f;

        private readonly System.Collections.Generic.Dictionary<Player, System.DateTime> _cooldowns
            = new System.Collections.Generic.Dictionary<Player, System.DateTime>();

        public bool IsOnCooldown(Player player)
        {
            if (Cooldown <= 0f) return false;
            if (!_cooldowns.ContainsKey(player)) return false;
            return (System.DateTime.Now - _cooldowns[player]).TotalSeconds < Cooldown;
        }

        public float GetRemainingCooldown(Player player)
        {
            if (!_cooldowns.ContainsKey(player)) return 0f;
            var elapsed = (float)(System.DateTime.Now - _cooldowns[player]).TotalSeconds;
            return System.Math.Max(0f, Cooldown - elapsed);
        }

        public bool TryActivate(Player player)
        {
            if (IsOnCooldown(player)) return false;
            if (!CanActivate(player)) return false;

            Activate(player);
            if (Cooldown > 0f)
                _cooldowns[player] = System.DateTime.Now;
            return true;
        }

        protected virtual bool CanActivate(Player player) => true;
        protected abstract void Activate(Player player);

        public virtual void OnTick(Player player) { }
        public virtual void OnAssigned(Player player) { }
        public virtual void OnRemoved(Player player) { }

        public void ClearCooldown(Player player)
        {
            _cooldowns.Remove(player);
        }

        public void ClearAllCooldowns()
        {
            _cooldowns.Clear();
        }
    }
}
