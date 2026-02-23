using System;
using QOLFramework.CustomItems;
using QOLFramework.CustomRoles;
using QOLFramework.CustomSCPs;
using QOLFramework.GUI;
using QOLFramework.Tweaks;
using QOLFramework.Events;
using QOLFramework.Utilities;

namespace QOLFramework.Core
{
    /// <summary>Ponto de entrada do QOL Framework. Singleton que inicializa módulos, roles, tweaks, GUI e eventos.</summary>
    public class QOLFrameworkLoader
    {
        /// <summary>Instância ativa do loader (null após Shutdown).</summary>
        public static QOLFrameworkLoader Instance { get; private set; }
        /// <summary>Versão do framework.</summary>
        public static Version FrameworkVersion { get; } = new Version(2, 0, 0);

        /// <summary>Gestor de módulos (registar e ativar/desativar).</summary>
        public ModuleManager Modules { get; }
        /// <summary>Gestor de roles custom (ex.: Containment Engineer, SCP-035).</summary>
        public CustomRoleManager Roles { get; }
        /// <summary>Gestor de SCPs custom (roles que estendem CustomScp).</summary>
        public CustomScpManager SCPs { get; }
        /// <summary>Gestor de tweaks (alterações a parâmetros de SCPs/jogo).</summary>
        public TweakManager Tweaks { get; }
        /// <summary>Gestor de GUI por jogador (hints, labels, barras).</summary>
        public GuiManager Gui { get; }
        /// <summary>Gestor de itens/triggers custom no mundo.</summary>
        public CustomItemManager CustomItemManagerInstance { get; }

        private bool _isInitialized;

        public QOLFrameworkLoader()
        {
            if (Instance != null)
                throw new InvalidOperationException("QOLFrameworkLoader already initialized. Use Instance instead.");

            Instance = this;

            Modules = new ModuleManager();
            Roles = new CustomRoleManager();
            SCPs = new CustomScpManager(Roles);
            Tweaks = new TweakManager();
            Gui = new GuiManager();
            CustomItemManagerInstance = new CustomItemManager();
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            LabApi.Features.Console.Logger.Info($"[QOL Framework] v{FrameworkVersion} initializing...");

            Roles.Initialize();
            Tweaks.Initialize();
            Gui.Initialize();
            BroadcastManager.Initialize();
            Modules.Initialize();

            LabApi.Features.Console.Logger.Info($"[QOL Framework] Initialized successfully!");
            LabApi.Features.Console.Logger.Info($"[QOL Framework] Modules: {Modules.Modules.Count} | Roles: {Roles.Roles.Count} | Tweaks: {Tweaks.Tweaks.Count}");
        }

        public void Shutdown()
        {
            if (!_isInitialized) return;

            LabApi.Features.Console.Logger.Info("[QOL Framework] Shutting down...");

            Modules.Shutdown();
            Gui.Shutdown();
            BroadcastManager.Shutdown();
            Tweaks.Shutdown();
            Roles.Shutdown();
            CustomItemManagerInstance.DestroyAll();
            QOLEventBus.Clear();

            CoroutineHelper.KillAll();
            CooldownManager.ClearAll();
            DamageModifierSystem.ClearAll();
            PlayerDataStore.ClearAll();
            RespawnManager.ClearAll();

            _isInitialized = false;
            Instance = null;

            LabApi.Features.Console.Logger.Info("[QOL Framework] Shutdown complete.");
        }
    }
}
