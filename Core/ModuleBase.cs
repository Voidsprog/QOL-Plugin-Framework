using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using QOLFramework.Config;

namespace QOLFramework.Core
{
    public abstract class ModuleBase : IModule
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string Author => "QOL";
        public virtual Version Version => new Version(1, 0, 0);
        public virtual int Priority => 0;
        public bool IsEnabled { get; set; } = true;

        /// <summary>Tipos de módulos que têm de estar registados e ativos para este módulo poder ativar. Null ou vazio = sem dependências.</summary>
        protected virtual Type[] RequiredModules => null;

        protected ModuleManager Manager { get; private set; }

        internal void SetManager(ModuleManager manager) => Manager = manager;

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnPlayerJoined(Player player) { }
        public virtual void OnPlayerLeft(Player player) { }
        public virtual void OnRoundStarted() { }
        public virtual void OnRoundEnded() { }
        public virtual void OnWaitingForPlayers() { }
        /// <summary>Chamado quando um jogador recebe dano (LabAPI Hurting). Override para reagir.</summary>
        public virtual void OnPlayerHurting(PlayerHurtingEventArgs ev) { }
        /// <summary>Chamado quando um jogador morre (LabAPI Dying). Override para reagir.</summary>
        public virtual void OnPlayerDying(PlayerDyingEventArgs ev) { }

        protected void Log(string message)
        {
            LabApi.Features.Console.Logger.Info($"[QOL:{Name}] {message}");
        }

        protected void LogWarning(string message)
        {
            LabApi.Features.Console.Logger.Warn($"[QOL:{Name}] {message}");
        }

        protected void LogError(string message)
        {
            LabApi.Features.Console.Logger.Error($"[QOL:{Name}] {message}");
        }

        /// <summary>Executa a ação dentro de try-catch; em caso de exceção regista o erro e não propaga.</summary>
        protected void RunSafe(Action action)
        {
            try { action(); }
            catch (Exception ex) { LogError($"RunSafe: {ex.Message}"); }
        }

        /// <summary>Executa a função dentro de try-catch; em caso de exceção regista o erro e retorna o valor de fallback.</summary>
        protected T RunSafe<T>(Func<T> fn, T fallback = default)
        {
            try { return fn(); }
            catch (Exception ex) { LogError($"RunSafe: {ex.Message}"); return fallback; }
        }

        /// <summary>Carrega a config do módulo em JSON (ConfigManager). Chave = Name. Cria e guarda ficheiro se não existir.</summary>
        protected T LoadModuleConfig<T>() where T : class, new() => ConfigManager.LoadOrCreate<T>(Name, true);

        /// <summary>Guarda a config do módulo em JSON (ConfigManager). Chave = Name.</summary>
        protected bool SaveModuleConfig<T>(T config) where T : class => ConfigManager.Save(Name, config);
    }

    public abstract class ModuleBase<TConfig> : ModuleBase where TConfig : Config.ModuleConfig, new()
    {
        public TConfig Config { get; internal set; } = new TConfig();
    }
}
