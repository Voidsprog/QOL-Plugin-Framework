namespace QOLFramework.GUI.Elements
{
    public class GuiLabel : GuiElement
    {
        public string Text { get; set; } = "";
        public int FontSize { get; set; } = 20;
        public string Color { get; set; } = "white";
        public bool Bold { get; set; }
        public bool Italic { get; set; }

        public override string Render()
        {
            var text = Text;
            if (Bold) text = $"<b>{text}</b>";
            if (Italic) text = $"<i>{text}</i>";
            if (Color != "white") text = $"<color={Color}>{text}</color>";
            if (FontSize != 20) text = $"<size={FontSize}>{text}</size>";
            return text;
        }
    }
}
