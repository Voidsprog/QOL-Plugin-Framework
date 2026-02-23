using System;
using System.Collections.Generic;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;

namespace QOLFramework.GUI
{
    public class GuiManager
    {
        private readonly Dictionary<Player, GuiScreen> _screens = new Dictionary<Player, GuiScreen>();
        private CoroutineHandle _refreshCoroutine;
        private bool _isRunning;

        public float RefreshRate { get; set; } = 0.5f;

        public void Initialize()
        {
            if (_isRunning) return;
            _isRunning = true;

            PlayerEvents.Left += OnPlayerLeft;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;

            _refreshCoroutine = Timing.RunCoroutine(RefreshCoroutine());
        }

        public void Shutdown()
        {
            if (!_isRunning) return;

            PlayerEvents.Left -= OnPlayerLeft;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;

            Timing.KillCoroutines(_refreshCoroutine);
            _screens.Clear();
            _isRunning = false;
        }

        public GuiScreen GetOrCreateScreen(Player player)
        {
            if (!_screens.TryGetValue(player, out var screen))
            {
                screen = new GuiScreen();
                _screens[player] = screen;
            }
            return screen;
        }

        public void ShowElement(Player player, GuiElement element)
        {
            var screen = GetOrCreateScreen(player);
            screen.AddElement(element);
        }

        public void HideElement(Player player, string elementId)
        {
            if (_screens.TryGetValue(player, out var screen))
            {
                var el = screen.GetElement(elementId);
                if (el != null) el.IsVisible = false;
            }
        }

        public void RemoveElement(Player player, string elementId)
        {
            if (_screens.TryGetValue(player, out var screen))
                screen.RemoveElement(elementId);
        }

        public void ClearScreen(Player player)
        {
            if (_screens.TryGetValue(player, out var screen))
                screen.Clear();
        }

        public void ShowTemporaryHint(Player player, string message, float duration = 5f)
        {
            var id = $"temp_{Guid.NewGuid():N}";
            var label = new Elements.GuiLabel
            {
                Id = id,
                Text = message,
                Order = 999
            };

            ShowElement(player, label);

            Timing.CallDelayed(duration, () => RemoveElement(player, id));
        }

        public void BroadcastHint(string message, float duration = 5f)
        {
            foreach (var player in Player.List)
                ShowTemporaryHint(player, message, duration);
        }

        private void OnPlayerLeft(LabApi.Events.Arguments.PlayerEvents.PlayerLeftEventArgs ev)
        {
            _screens.Remove(ev.Player);
        }

        private void OnWaitingForPlayers()
        {
            _screens.Clear();
        }

        private IEnumerator<float> RefreshCoroutine()
        {
            while (_isRunning)
            {
                yield return Timing.WaitForSeconds(RefreshRate);

                var toRemove = new List<Player>();

                foreach (var kvp in _screens)
                {
                    if (kvp.Key.IsDestroyed)
                    {
                        toRemove.Add(kvp.Key);
                        continue;
                    }

                    var rendered = kvp.Value.Render();
                    if (!string.IsNullOrEmpty(rendered))
                    {
                        try
                        {
                            kvp.Key.SendHint(rendered, RefreshRate + 0.3f);
                        }
                        catch { /* Player disconnected mid-send */ }
                    }
                }

                foreach (var p in toRemove)
                    _screens.Remove(p);
            }
        }
    }
}
