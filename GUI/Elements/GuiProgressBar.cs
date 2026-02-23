using System;

namespace QOLFramework.GUI.Elements
{
    public class GuiProgressBar : GuiElement
    {
        public string Label { get; set; } = "";
        public float Value { get; set; }
        public float MaxValue { get; set; } = 100f;
        public int Width { get; set; } = 20;
        public string FilledColor { get; set; } = "#00FF00";
        public string EmptyColor { get; set; } = "#555555";
        public string FilledChar { get; set; } = "█";
        public string EmptyChar { get; set; } = "░";
        public bool ShowPercentage { get; set; } = true;

        public override string Render()
        {
            var percentage = MaxValue > 0 ? Math.Min(Value / MaxValue, 1f) : 0f;
            var filled = (int)(percentage * Width);
            var empty = Width - filled;

            var bar = $"<color={FilledColor}>{new string('█', filled)}</color>" +
                      $"<color={EmptyColor}>{new string('░', empty)}</color>";

            var result = string.IsNullOrEmpty(Label) ? bar : $"{Label} {bar}";

            if (ShowPercentage)
                result += $" {percentage * 100:F0}%";

            return $"<size=16>{result}</size>";
        }
    }
}
