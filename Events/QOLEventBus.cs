using System;
using System.Collections.Generic;
using System.Linq;

namespace QOLFramework.Events
{
    public static class QOLEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private static readonly Dictionary<Type, List<(int priority, Delegate handler)>> _priorityHandlers
            = new Dictionary<Type, List<(int, Delegate)>>();

        public static void Subscribe<T>(Action<T> handler, int priority = 0) where T : QOLEventArgs
        {
            var type = typeof(T);
            if (!_priorityHandlers.ContainsKey(type))
                _priorityHandlers[type] = new List<(int, Delegate)>();

            _priorityHandlers[type].Add((priority, handler));
            _priorityHandlers[type].Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : QOLEventArgs
        {
            var type = typeof(T);
            if (!_priorityHandlers.ContainsKey(type)) return;

            _priorityHandlers[type].RemoveAll(h => h.handler == (Delegate)handler);
        }

        public static void Publish<T>(T eventArgs) where T : QOLEventArgs
        {
            var type = typeof(T);
            if (!_priorityHandlers.ContainsKey(type)) return;

            foreach (var (_, handler) in _priorityHandlers[type].ToList())
            {
                try
                {
                    ((Action<T>)handler)(eventArgs);
                    if (eventArgs.IsCancelled) break;
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Events] Error handling {type.Name}: {ex}");
                }
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
            _priorityHandlers.Clear();
        }
    }

    public class QOLEventArgs : EventArgs
    {
        public bool IsCancelled { get; set; }
    }

    public class CustomRoleAssignedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Player { get; set; }
        public CustomRoles.CustomRole Role { get; set; }
    }

    public class CustomRoleRemovedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Player { get; set; }
        public CustomRoles.CustomRole Role { get; set; }
    }

    public class TweakAppliedEvent : QOLEventArgs
    {
        public Tweaks.ITweak Tweak { get; set; }
    }

    public class ModuleEnabledEvent : QOLEventArgs
    {
        public Core.IModule Module { get; set; }
    }

    public class ModuleDisabledEvent : QOLEventArgs
    {
        public Core.IModule Module { get; set; }
    }

    // --- Eventos de jogo (publicar a partir de módulos ou do plugin quando subscreveres LabAPI) ---

    /// <summary>Publicar quando um jogador entra no servidor (alternativa a IModule.OnPlayerJoined).</summary>
    public class PlayerJoinedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Player { get; set; }
    }

    /// <summary>Publicar quando um jogador sai do servidor.</summary>
    public class PlayerLeftEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Player { get; set; }
    }

    /// <summary>Publicar quando um jogador recebe dano. Definir IsCancelled = true para cancelar o dano (se o handler do jogo o suportar).</summary>
    public class PlayerDamagedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Target { get; set; }
        public LabApi.Features.Wrappers.Player Attacker { get; set; }
        public float Amount { get; set; }
        public string DamageType { get; set; }
    }

    /// <summary>Publicar quando um jogador morre.</summary>
    public class PlayerDiedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Target { get; set; }
        public LabApi.Features.Wrappers.Player Killer { get; set; }
        public string Cause { get; set; }
    }

    /// <summary>Publicar quando um jogador usa um item (ex.: consumível, keycard).</summary>
    public class ItemUsedEvent : QOLEventArgs
    {
        public LabApi.Features.Wrappers.Player Player { get; set; }
        public int ItemTypeId { get; set; }
        public object ItemInstance { get; set; }
    }

    /// <summary>Publicar antes do round iniciar. IsCancelled = true pode ser usado para atrasar/cancelar (conforme suporte do plugin).</summary>
    public class RoundStartingEvent : QOLEventArgs
    {
        // Sem dados extra; cancelar evita avançar (se o publicador respeitar).
    }
}
