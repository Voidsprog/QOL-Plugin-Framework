using LabApi.Features.Wrappers;

namespace QOLFramework.Permissions
{
    /// <summary>
    /// Helper para verificação de permissões. Por defeito todos têm permissão.
    /// Atribui <see cref="CheckPermission"/> para integrar com LabAPI/EXILED/LabExtended (ex.: ExPlayer.HasPermission).
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Delegate usado para verificar se um jogador tem uma permissão.
        /// Por defeito é null e <see cref="HasPermission"/> retorna true (sem restrições).
        /// </summary>
        public static System.Func<Player, string, bool> CheckPermission { get; set; }

        /// <summary>Verifica se o jogador tem a permissão. Se CheckPermission for null, retorna true.</summary>
        public static bool HasPermission(Player player, string permission)
        {
            if (player == null || string.IsNullOrEmpty(permission)) return false;
            return CheckPermission == null || CheckPermission(player, permission);
        }
    }
}
