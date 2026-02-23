using System;
using LabApi.Features.Wrappers;

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

        protected ModuleManager Manager { get; private set; }

        internal void SetManager(ModuleManager manager) => Manager = manager;

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnPlayerJoined(Player player) { }
        public virtual void OnPlayerLeft(Player player) { }
        public virtual void OnRoundStarted() { }
        public virtual void OnRoundEnded() { }
        public virtual void OnWaitingForPlayers() { }

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
    }

    public abstract class ModuleBase<TConfig> : ModuleBase where TConfig : Config.ModuleConfig, new()
    {
        public TConfig Config { get; internal set; } = new TConfig();
    }
}
