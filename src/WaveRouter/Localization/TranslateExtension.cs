using System.ComponentModel;
using System.Windows.Markup;
using Binding = System.Windows.Data.Binding;
using BindingMode = System.Windows.Data.BindingMode;

namespace WaveRouter.Localization;

/// <summary>XAML usage: <c>Text="{loc:Translate MainWindow.RulesHeading}"</c>. Binds through a small proxy
/// object rather than returning a plain string, so the bound property updates live when
/// <see cref="LocalizationManager.LanguageChanged"/> fires — no app restart needed to switch language.</summary>
[MarkupExtensionReturnType(typeof(object))]
public sealed class TranslateExtension : MarkupExtension
{
    public TranslateExtension()
    {
    }

    public TranslateExtension(string key) => Key = key;

    [ConstructorArgument("key")]
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding(nameof(TranslationProxy.Value))
        {
            Source = new TranslationProxy(Key),
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }

    private sealed class TranslationProxy : INotifyPropertyChanged
    {
        private readonly string _key;

        public TranslationProxy(string key)
        {
            _key = key;
            // Never unsubscribed: the proxy (a few bytes) is rooted for the app's lifetime once created.
            // Fine for windows created once and reused (MainWindow, SettingsWindow), and negligible even
            // for NewAppPromptWindow's repeated instantiation — a full weak-event pattern isn't worth the
            // extra complexity at this app's scale.
            LocalizationManager.LanguageChanged += OnLanguageChanged;
        }

        public string Value => LocalizationManager.Translate(_key);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnLanguageChanged(object? sender, EventArgs e) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
