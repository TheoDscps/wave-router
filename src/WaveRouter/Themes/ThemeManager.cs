using System.Windows;

namespace WaveRouter.Themes;

public enum AppTheme
{
    Dark,
    Light,
}

/// <summary>
/// Swaps the active color dictionary (index 0 of the app's merged dictionaries) at runtime.
/// Non-color tokens (Generic.xaml) stay merged and untouched.
/// </summary>
public static class ThemeManager
{
    private static readonly Uri DarkColors = new("pack://application:,,,/Themes/Colors.Dark.xaml");
    private static readonly Uri LightColors = new("pack://application:,,,/Themes/Colors.Light.xaml");

    public static void Apply(AppTheme theme)
    {
        var uri = theme == AppTheme.Dark ? DarkColors : LightColors;
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        dictionaries[0] = new ResourceDictionary { Source = uri };
    }
}
