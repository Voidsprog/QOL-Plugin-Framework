using System.ComponentModel;

namespace QOLFramework.Config
{
    public abstract class ModuleConfig
    {
        [Description("Whether this module is enabled")]
        public bool IsEnabled { get; set; } = true;
    }
}
