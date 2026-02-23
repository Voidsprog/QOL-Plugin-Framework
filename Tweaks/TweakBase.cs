namespace QOLFramework.Tweaks
{
    public abstract class TweakBase : ITweak
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string Category => "General";
        public bool IsEnabled { get; set; } = true;

        private bool _isApplied;

        public void Apply()
        {
            if (_isApplied) return;
            OnApply();
            _isApplied = true;
            LabApi.Features.Console.Logger.Info($"[QOL:Tweak] Applied '{Name}'");
        }

        public void Revert()
        {
            if (!_isApplied) return;
            OnRevert();
            _isApplied = false;
            LabApi.Features.Console.Logger.Info($"[QOL:Tweak] Reverted '{Name}'");
        }

        protected abstract void OnApply();
        protected abstract void OnRevert();
    }

    public abstract class TweakBase<TConfig> : TweakBase where TConfig : Config.ModuleConfig, new()
    {
        public TConfig Config { get; set; } = new TConfig();
    }
}
