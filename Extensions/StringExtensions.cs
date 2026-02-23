namespace QOLFramework.Extensions
{
    public static class StringExtensions
    {
        public static string Color(this string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }

        public static string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        public static string Italic(this string text)
        {
            return $"<i>{text}</i>";
        }

        public static string Size(this string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        public static string Underline(this string text)
        {
            return $"<u>{text}</u>";
        }

        public static string StrikeThrough(this string text)
        {
            return $"<s>{text}</s>";
        }

        public static string QOLTag(this string text)
        {
            return $"<color=#00BFFF>[QOL]</color> {text}";
        }

        public static string ScpTag(this string text, string scpNumber)
        {
            return $"<color=#FF0000>[SCP-{scpNumber}]</color> {text}";
        }

        public static string WarningTag(this string text)
        {
            return $"<color=#FFD700>[!]</color> {text}";
        }

        public static string ErrorTag(this string text)
        {
            return $"<color=#FF0000>[X]</color> {text}";
        }

        public static string SuccessTag(this string text)
        {
            return $"<color=#00FF00>[✓]</color> {text}";
        }
    }
}
