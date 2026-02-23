using System;
using LabApi.Features.Wrappers;

namespace QOLFramework.Core
{
    /// <summary>Interface base para módulos do QOL Framework. Ciclo de vida: Enable, round, jogadores, Disable.</summary>
    public interface IModule
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        Version Version { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }

        void OnEnabled();
        void OnDisabled();
        /// <summary>Chamado quando um jogador entra no servidor.</summary>
        void OnPlayerJoined(Player player);
        /// <summary>Chamado quando um jogador sai do servidor.</summary>
        void OnPlayerLeft(Player player);
        void OnRoundStarted();
        void OnRoundEnded();
        void OnWaitingForPlayers();
    }
}
