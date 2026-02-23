namespace QOLFramework.GUI
{
    public abstract class GuiElement
    {
        public string Id { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public GuiAlignment Alignment { get; set; } = GuiAlignment.Center;

        public abstract string Render();
    }

    public enum GuiAlignment
    {
        Left,
        Center,
        Right
    }
}
