using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

namespace QOLFramework.CustomSCPs
{
    public class CustomScpManager
    {
        private readonly CustomRoles.CustomRoleManager _roleManager;

        public CustomScpManager(CustomRoles.CustomRoleManager roleManager)
        {
            _roleManager = roleManager;
        }

        public void RegisterScp(CustomScp scp)
        {
            _roleManager.RegisterRole(scp);
            LabApi.Features.Console.Logger.Info($"[QOL:SCPs] Registered custom SCP-{scp.ScpNumber} '{scp.Name}'");
        }

        public void UnregisterScp(int id)
        {
            _roleManager.UnregisterRole(id);
        }

        public bool SpawnScp(Player player, int scpId)
        {
            return _roleManager.AssignRole(player, scpId);
        }

        public bool SpawnScp<T>(Player player) where T : CustomScp
        {
            return _roleManager.AssignRole<T>(player);
        }

        public CustomScp GetPlayerScp(Player player)
        {
            return _roleManager.GetPlayerRole(player) as CustomScp;
        }

        public T GetScp<T>() where T : CustomScp
        {
            return _roleManager.GetRole<T>();
        }

        public IEnumerable<CustomScp> GetAllScps()
        {
            return _roleManager.Roles.Values.OfType<CustomScp>();
        }

        public List<Player> GetScpPlayers(int scpId)
        {
            return _roleManager.GetPlayersWithRole(scpId);
        }

        public bool IsCustomScp(Player player)
        {
            return GetPlayerScp(player) != null;
        }
    }
}
