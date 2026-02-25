using LabApi.Features.Wrappers;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Identificador único de jogador usado por PlayerDataStore, CooldownManager, BroadcastManager e outros.
    /// Garante consistência entre todos os sistemas que indexam dados por jogador.
    /// </summary>
    public static class PlayerIdHelper
    {
        /// <summary>Obtém um ID estável para o jogador. Preferência: UserId; fallback: ReferenceHub InstanceID; senão "unknown".</summary>
        public static string GetId(Player player)
        {
            if (player == null) return "unknown";
            try
            {
                return player.UserId ?? player.ReferenceHub?.GetInstanceID().ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
