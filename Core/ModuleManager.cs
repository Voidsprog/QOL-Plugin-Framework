using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using QOLFramework.Utilities;

namespace QOLFramework.Core
{
    public class ModuleManager
    {
        private readonly List<IModule> _modules = new List<IModule>();
        private bool _isRunning;

        public IReadOnlyList<IModule> Modules => _modules.AsReadOnly();
        public IEnumerable<IModule> EnabledModules => _modules.Where(m => m.IsEnabled);

        public event Action<IModule> ModuleRegistered;
        public event Action<IModule> ModuleEnabled;
        public event Action<IModule> ModuleDisabled;

        public void Initialize()
        {
            if (_isRunning) return;
            _isRunning = true;

            PlayerEvents.Joined += OnPlayerJoined;
            PlayerEvents.Left += OnPlayerLeft;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;

            foreach (var module in _modules.OrderBy(m => m.Priority))
            {
                if (!module.IsEnabled) continue;

                try
                {
                    if (module is ModuleBase mb)
                        mb.SetManager(this);

                    module.OnEnabled();
                    ModuleEnabled?.Invoke(module);
                    LabApi.Features.Console.Logger.Info($"[QOL] Module '{module.Name}' v{module.Version} enabled.");
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Failed to enable module '{module.Name}': {ex}");
                    module.IsEnabled = false;
                }
            }
        }

        public void Shutdown()
        {
            if (!_isRunning) return;

            PlayerEvents.Joined -= OnPlayerJoined;
            PlayerEvents.Left -= OnPlayerLeft;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;

            foreach (var module in _modules.Where(m => m.IsEnabled).OrderByDescending(m => m.Priority))
            {
                try
                {
                    module.OnDisabled();
                    ModuleDisabled?.Invoke(module);
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Failed to disable module '{module.Name}': {ex}");
                }
            }

            _isRunning = false;
        }

        public T Register<T>() where T : IModule, new()
        {
            var module = new T();
            Register(module);
            return module;
        }

        public void Register(IModule module)
        {
            if (_modules.Any(m => m.GetType() == module.GetType()))
            {
                LabApi.Features.Console.Logger.Warn($"[QOL] Module '{module.Name}' is already registered.");
                return;
            }

            if (module is ModuleBase mb)
                mb.SetManager(this);

            _modules.Add(module);
            ModuleRegistered?.Invoke(module);

            if (_isRunning && module.IsEnabled)
            {
                try
                {
                    module.OnEnabled();
                    ModuleEnabled?.Invoke(module);
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Failed to enable module '{module.Name}': {ex}");
                    module.IsEnabled = false;
                }
            }
        }

        public void Unregister<T>() where T : IModule
        {
            var module = _modules.FirstOrDefault(m => m is T);
            if (module == null) return;

            if (module.IsEnabled && _isRunning)
            {
                module.OnDisabled();
                ModuleDisabled?.Invoke(module);
            }

            _modules.Remove(module);
        }

        public T GetModule<T>() where T : class, IModule
        {
            return _modules.FirstOrDefault(m => m is T) as T;
        }

        public bool IsModuleEnabled<T>() where T : class, IModule
        {
            return GetModule<T>()?.IsEnabled ?? false;
        }

        public void EnableModule<T>() where T : class, IModule
        {
            var module = GetModule<T>();
            if (module == null || module.IsEnabled) return;

            module.IsEnabled = true;
            if (_isRunning)
            {
                module.OnEnabled();
                ModuleEnabled?.Invoke(module);
            }
        }

        public void DisableModule<T>() where T : class, IModule
        {
            var module = GetModule<T>();
            if (module == null || !module.IsEnabled) return;

            if (_isRunning)
            {
                module.OnDisabled();
                ModuleDisabled?.Invoke(module);
            }
            module.IsEnabled = false;
        }

        private void OnPlayerJoined(LabApi.Events.Arguments.PlayerEvents.PlayerJoinedEventArgs ev)
        {
            if (ev?.Player == null || ev.Player.IsDestroyed) return;
            foreach (var module in EnabledModules)
            {
                try { module.OnPlayerJoined(ev.Player); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnPlayerJoined: {ex}");
                }
            }
        }

        private void OnPlayerLeft(LabApi.Events.Arguments.PlayerEvents.PlayerLeftEventArgs ev)
        {
            if (ev?.Player == null) return;
            foreach (var module in EnabledModules)
            {
                try { module.OnPlayerLeft(ev.Player); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnPlayerLeft: {ex}");
                }
            }
        }

        private void OnWaitingForPlayers()
        {
            RoundUtilities.OnWaitingForPlayers();
            PlayerDataStore.ClearAll();
            CooldownManager.ClearAll();
            DamageModifierSystem.ClearAll();
            RespawnManager.ClearAll();
            BroadcastManager.ClearAllQueues();

            foreach (var module in EnabledModules)
            {
                try { module.OnWaitingForPlayers(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnWaitingForPlayers: {ex}");
                }
            }
        }

        private void OnRoundStarted()
        {
            RoundUtilities.OnRoundStarted();

            foreach (var module in EnabledModules)
            {
                try { module.OnRoundStarted(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnRoundStarted: {ex}");
                }
            }
        }

        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            RoundUtilities.OnRoundEnded();

            foreach (var module in EnabledModules)
            {
                try { module.OnRoundEnded(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnRoundEnded: {ex}");
                }
            }
        }
    }
}
