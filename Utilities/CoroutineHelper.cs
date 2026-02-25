using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using MEC;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Utilitários para coroutines MEC (More Effective Coroutines).
    /// Facilita criação de delays, loops, repetições e gestão de handles.
    /// </summary>
    public static class CoroutineHelper
    {
        private static readonly Dictionary<string, CoroutineHandle> _namedCoroutines
            = new Dictionary<string, CoroutineHandle>();

        public static CoroutineHandle CallDelayed(float delay, Action action)
        {
            return Timing.CallDelayed(delay, action);
        }

        /// <summary>Executa a ação após o delay apenas se o jogador ainda for válido (não destruído). Evita efeitos em jogadores que já saíram.</summary>
        public static CoroutineHandle CallDelayed(float delay, Player player, Action action)
        {
            return Timing.CallDelayed(delay, () =>
            {
                if (player == null || player.IsDestroyed) return;
                try { action(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Coroutine] CallDelayed(player) error: {ex.Message}");
                }
            });
        }

        public static CoroutineHandle CallRepeating(float interval, Action action, float initialDelay = 0f)
        {
            return Timing.RunCoroutine(RepeatCoroutine(interval, action, initialDelay));
        }

        public static CoroutineHandle CallRepeating(float interval, Func<bool> action, float initialDelay = 0f)
        {
            return Timing.RunCoroutine(RepeatUntilFalseCoroutine(interval, action, initialDelay));
        }

        public static CoroutineHandle CallFor(float duration, float interval, Action<float> action)
        {
            return Timing.RunCoroutine(TimedCoroutine(duration, interval, action));
        }

        public static CoroutineHandle RunNamed(string name, IEnumerator<float> coroutine, bool killExisting = true)
        {
            if (killExisting)
                KillNamed(name);

            var handle = Timing.RunCoroutine(coroutine);
            _namedCoroutines[name] = handle;
            return handle;
        }

        public static bool KillNamed(string name)
        {
            if (!_namedCoroutines.ContainsKey(name)) return false;
            Timing.KillCoroutines(_namedCoroutines[name]);
            _namedCoroutines.Remove(name);
            return true;
        }

        public static void KillAll()
        {
            foreach (var handle in _namedCoroutines.Values)
                Timing.KillCoroutines(handle);
            _namedCoroutines.Clear();
        }

        public static bool IsRunning(string name)
        {
            return _namedCoroutines.ContainsKey(name) &&
                   Timing.IsRunning(_namedCoroutines[name]);
        }

        public static CoroutineHandle Sequence(params (float delay, Action action)[] steps)
        {
            return Timing.RunCoroutine(SequenceCoroutine(steps));
        }

        private static IEnumerator<float> RepeatCoroutine(float interval, Action action, float initialDelay)
        {
            if (initialDelay > 0f)
                yield return Timing.WaitForSeconds(initialDelay);

            while (true)
            {
                try { action(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Coroutine] Repeat error: {ex.Message}");
                }
                yield return Timing.WaitForSeconds(interval);
            }
        }

        private static IEnumerator<float> RepeatUntilFalseCoroutine(float interval, Func<bool> action, float initialDelay)
        {
            if (initialDelay > 0f)
                yield return Timing.WaitForSeconds(initialDelay);

            while (true)
            {
                bool continueRunning;
                try { continueRunning = action(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Coroutine] RepeatUntilFalse error: {ex.Message}");
                    continueRunning = false;
                }
                if (!continueRunning) yield break;
                yield return Timing.WaitForSeconds(interval);
            }
        }

        private static IEnumerator<float> TimedCoroutine(float duration, float interval, Action<float> action)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                try { action(elapsed); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Coroutine] Timed error: {ex.Message}");
                }
                yield return Timing.WaitForSeconds(interval);
                elapsed += interval;
            }
        }

        private static IEnumerator<float> SequenceCoroutine((float delay, Action action)[] steps)
        {
            foreach (var (delay, action) in steps)
            {
                if (delay > 0f)
                    yield return Timing.WaitForSeconds(delay);
                try { action(); }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Error($"[QOL:Coroutine] Sequence error: {ex.Message}");
                }
            }
        }
    }
}
