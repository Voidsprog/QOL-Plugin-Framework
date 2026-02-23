using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QOLFramework.GUI
{
    public class GuiScreen
    {
        private readonly Dictionary<string, GuiElement> _elements = new Dictionary<string, GuiElement>();

        public void AddElement(GuiElement element)
        {
            _elements[element.Id] = element;
        }

        public void RemoveElement(string id)
        {
            _elements.Remove(id);
        }

        public GuiElement GetElement(string id)
        {
            return _elements.TryGetValue(id, out var el) ? el : null;
        }

        public T GetElement<T>(string id) where T : GuiElement
        {
            return GetElement(id) as T;
        }

        public bool HasElement(string id) => _elements.ContainsKey(id);

        public void Clear() => _elements.Clear();

        public string Render()
        {
            var visible = _elements.Values
                .Where(e => e.IsVisible)
                .OrderBy(e => e.Order)
                .ToList();

            if (visible.Count == 0) return null;

            var sb = new StringBuilder();
            sb.Append("<size=20>");

            foreach (var element in visible)
            {
                var rendered = element.Render();
                if (string.IsNullOrEmpty(rendered)) continue;

                switch (element.Alignment)
                {
                    case GuiAlignment.Left:
                        sb.Append($"<align=left>{rendered}</align>");
                        break;
                    case GuiAlignment.Right:
                        sb.Append($"<align=right>{rendered}</align>");
                        break;
                    default:
                        sb.Append(rendered);
                        break;
                }
                sb.Append("\n");
            }

            sb.Append("</size>");
            return sb.ToString();
        }
    }
}
