using System.Collections.Generic;
using MEC;
using QOLFramework.Hybrid;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Utilitários para anúncios CASSIE simplificados.
    /// Suporta mensagens pré-definidas, sequências e delays.
    /// </summary>
    public static class CassieHelper
    {
        public static void Announce(string message, bool isHeld = false, bool isNoisy = true)
        {
            ExiledBridge.TryCassieMessage(message, isHeld, isNoisy);
        }

        public static void AnnounceDelayed(float delay, string message, bool isHeld = false, bool isNoisy = true)
        {
            Timing.CallDelayed(delay, () => Announce(message, isHeld, isNoisy));
        }

        public static void AnnounceSequence(float intervalBetween, params string[] messages)
        {
            Timing.RunCoroutine(SequenceCoroutine(intervalBetween, messages));
        }

        private static IEnumerator<float> SequenceCoroutine(float interval, string[] messages)
        {
            foreach (var msg in messages)
            {
                Announce(msg);
                yield return Timing.WaitForSeconds(interval);
            }
        }

        public static void ScpEscaped(string scpNumber)
        {
            Announce($"SCP {FormatScpNumber(scpNumber)} has breached containment");
        }

        public static void ScpContained(string scpNumber, string unit = null)
        {
            var msg = $"SCP {FormatScpNumber(scpNumber)} contained successfully";
            if (!string.IsNullOrEmpty(unit))
                msg += $" . containment unit {unit}";
            Announce(msg);
        }

        public static void ScpTerminated(string scpNumber, string by = null)
        {
            var msg = $"SCP {FormatScpNumber(scpNumber)} has been successfully terminated";
            if (!string.IsNullOrEmpty(by))
                msg += $" by {by}";
            Announce(msg);
        }

        public static void FacilityAlert(string message)
        {
            Announce($"ATTENTION ALL PERSONNEL . {message}");
        }

        public static void WarheadCountdown(int seconds)
        {
            Announce($"ALPHA WARHEAD . DETONATION IN T MINUS {seconds} SECONDS");
        }

        public static void LightsOut()
        {
            Announce("FACILITY WIDE ELECTRICAL MALFUNCTION . BACKUP GENERATORS OFFLINE");
        }

        public static void CustomAlert(string prefix, string message)
        {
            Announce($"{prefix} . {message}");
        }

        private static string FormatScpNumber(string number)
        {
            var clean = number.Replace("-", "").Replace("SCP", "").Trim();
            var chars = clean.ToCharArray();
            return string.Join(" ", chars);
        }
    }
}
