using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WaveRouter.Converters;

/// <summary>Visible when the bound value is non-null (or a non-empty string); Collapsed otherwise.
/// Pass ConverterParameter="Invert" to flip it.</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = value is not null && value is not "";
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase))
        {
            hasValue = !hasValue;
        }

        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
