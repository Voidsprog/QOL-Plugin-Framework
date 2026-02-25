using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using QOLFramework.Events;
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
            PlayerEvents.Hurting += OnPlayerHurting;
            PlayerEvents.Dying += OnPlayerDying;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;

            foreach (var module in _modules.OrderBy(m => m.Priority))
            {
                if (!module.IsEnabled) continue;
                if (!TryEnableModule(module)) continue;

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
            PlayerEvents.Hurting -= OnPlayerHurting;
            PlayerEvents.Dying -= OnPlayerDying;
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

            if (_isRunning && module.IsEnabled && TryEnableModule(module))
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

        /// <summary>Verifica dependências (RequiredModules). Retorna false se faltar algum módulo obrigatório.</summary>
        private bool TryEnableModule(IModule module)
        {
            if (module is not ModuleBase mb) return true;
            var required = mb.RequiredModules;
            if (required == null || required.Length == 0) return true;

            foreach (var type in required)
            {
                var dep = _modules.FirstOrDefault(m => type.IsAssignableFrom(m.GetType()));
                if (dep == null || !dep.IsEnabled)
                {
                    LabApi.Features.Console.Logger.Warn($"[QOL] Module '{module.Name}' not enabled: required module '{type?.Name ?? "?"}' is missing or disabled.");
                    module.IsEnabled = false;
                    return false;
                }
            }
            return true;
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
            PlayerDataStore.ClearPlayer(ev.Player);
            CooldownManager.ClearPlayer(ev.Player);
            BroadcastManager.ClearQueue(ev.Player);
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

        private void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev?.Target == null) return;
            foreach (var module in EnabledModules)
            {
                if (module is ModuleBase mb)
                {
                    try { mb.OnPlayerHurting(ev); }
                    catch (Exception ex) { LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnPlayerHurting: {ex}"); }
                }
            }
            try
            {
                var qolEv = new PlayerDamagedEvent
                {
                    Target = ev.Player != null ? Player.Get(ev.Player) : null,
                    Attacker = ev.Attacker != null ? Player.Get(ev.Attacker) : null,
                    Amount = ev.DamageHandler?.Damage ?? 0f,
                    DamageType = ev.DamageHandler?.Type.ToString() ?? ""
                };
                QOLEventBus.Publish(qolEv);
            }
            catch { }
        }

        private void OnPlayerDying(PlayerDyingEventArgs ev)
        {
            if (ev?.Player == null) return;
            foreach (var module in EnabledModules)
            {
                if (module is ModuleBase mb)
                {
                    try { mb.OnPlayerDying(ev); }
                    catch (Exception ex) { LabApi.Features.Console.Logger.Error($"[QOL] Module '{module.Name}' error in OnPlayerDying: {ex}"); }
                }
            }
            try
            {
                var qolEv = new PlayerDiedEvent
                {
                    Target = ev.Player != null ? Player.Get(ev.Player) : null,
                    Killer = ev.Attacker != null ? Player.Get(ev.Attacker) : null,
                    Cause = ev.DamageHandler?.Type.ToString() ?? ""
                };
                QOLEventBus.Publish(qolEv);
            }
            catch { }
        }
    }
}
