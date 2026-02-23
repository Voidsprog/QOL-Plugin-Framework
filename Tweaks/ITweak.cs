namespace QOLFramework.Tweaks
{
    public interface ITweak
    {
        string Name { get; }
        string Description { get; }
        string Category { get; }
        bool IsEnabled { get; set; }

        void Apply();
        void Revert();
    }
}
