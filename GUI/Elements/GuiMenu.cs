using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QOLFramework.GUI.Elements
{
    public class GuiMenu : GuiElement
    {
        public string Title { get; set; } = "";
        public string TitleColor { get; set; } = "#00BFFF";
        public List<GuiMenuItem> Items { get; set; } = new List<GuiMenuItem>();
        public int SelectedIndex { get; set; }
        public string Separator { get; set; } = "─────────────────";

        public override string Render()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Title))
            {
                sb.AppendLine($"<color={TitleColor}><b>{Title}</b></color>");
                sb.AppendLine($"<color=#444444>{Separator}</color>");
            }

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var prefix = i == SelectedIndex ? "▸ " : "  ";
                var color = i == SelectedIndex ? "#FFFF00" : (item.IsEnabled ? "#CCCCCC" : "#666666");
                sb.AppendLine($"<color={color}>{prefix}{item.Text}</color>");
            }

            return sb.ToString().TrimEnd();
        }

        public void SelectNext()
        {
            if (Items.Count == 0) return;
            SelectedIndex = (SelectedIndex + 1) % Items.Count;
        }

        public void SelectPrevious()
        {
            if (Items.Count == 0) return;
            SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count;
        }

        public GuiMenuItem GetSelected()
        {
            return Items.Count > 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
        }
    }

    public class GuiMenuItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsEnabled { get; set; } = true;
        public object Tag { get; set; }
    }
}
