using LabApi.Features.Wrappers;

namespace QOLFramework.CustomSCPs
{
    public abstract class ScpAbility : CustomRoles.RoleAbility
    {
        public virtual bool IsPassive => false;

        public virtual string ActivationHint =>
            $"<color=#FF4444>[{Name}]</color> Habilidade ativada!";

        public virtual string CooldownHint =>
            $"<color=#FF4444>[{Name}]</color> Em cooldown! ({GetRemainingCooldown(null):F1}s)";

        public string GetCooldownHintFor(Player player) =>
            $"<color=#FF4444>[{Name}]</color> Em cooldown! ({GetRemainingCooldown(player):F1}s)";
    }
}
