using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Handlers;

namespace QOLFramework.Tweaks
{
    public class TweakManager
    {
        private readonly List<ITweak> _tweaks = new List<ITweak>();
        private bool _isRunning;

        public IReadOnlyList<ITweak> Tweaks => _tweaks.AsReadOnly();
        public IEnumerable<ITweak> EnabledTweaks => _tweaks.Where(t => t.IsEnabled);

        public void Initialize()
        {
            if (_isRunning) return;
            _isRunning = true;

            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;

            foreach (var tweak in _tweaks.Where(t => t.IsEnabled))
            {
                try
                {
                    tweak.Apply();
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Tweaks] Failed to apply '{tweak.Name}': {ex}");
                }
            }
        }

        public void Shutdown()
        {
            if (!_isRunning) return;

            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;

            foreach (var tweak in _tweaks.Where(t => t.IsEnabled))
            {
                try
                {
                    tweak.Revert();
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Tweaks] Failed to revert '{tweak.Name}': {ex}");
                }
            }

            _isRunning = false;
        }

        public void Register(ITweak tweak)
        {
            if (_tweaks.Any(t => t.Name == tweak.Name))
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Tweaks] Tweak '{tweak.Name}' already registered.");
                return;
            }

            _tweaks.Add(tweak);
            LabApi.Features.Console.Logger.Info($"[QOL:Tweaks] Registered tweak '{tweak.Name}' [{tweak.Category}]");

            if (_isRunning && tweak.IsEnabled)
            {
                try { tweak.Apply(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Tweaks] Failed to apply '{tweak.Name}': {ex}");
                }
            }
        }

        public void Unregister(string name)
        {
            var tweak = _tweaks.FirstOrDefault(t => t.Name == name);
            if (tweak == null) return;

            if (tweak.IsEnabled && _isRunning)
                tweak.Revert();

            _tweaks.Remove(tweak);
        }

        public ITweak GetTweak(string name)
        {
            return _tweaks.FirstOrDefault(t => t.Name == name);
        }

        public T GetTweak<T>() where T : class, ITweak
        {
            return _tweaks.FirstOrDefault(t => t is T) as T;
        }

        public IEnumerable<ITweak> GetTweaksByCategory(string category)
        {
            return _tweaks.Where(t => t.Category == category);
        }

        public void EnableTweak(string name)
        {
            var tweak = GetTweak(name);
            if (tweak == null || tweak.IsEnabled) return;
            tweak.IsEnabled = true;
            if (_isRunning) tweak.Apply();
        }

        public void DisableTweak(string name)
        {
            var tweak = GetTweak(name);
            if (tweak == null || !tweak.IsEnabled) return;
            if (_isRunning) tweak.Revert();
            tweak.IsEnabled = false;
        }

        private void OnWaitingForPlayers()
        {
            foreach (var tweak in _tweaks.Where(t => t.IsEnabled))
            {
                try
                {
                    tweak.Revert();
                    tweak.Apply();
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Tweaks] Error re-applying '{tweak.Name}': {ex}");
                }
            }
        }
    }
}
