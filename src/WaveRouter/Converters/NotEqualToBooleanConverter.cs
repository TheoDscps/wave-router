using System.Globalization;
using System.Windows.Data;

namespace WaveRouter.Converters;

/// <summary>True when the bound value does NOT equal the parameter — used to disable a "choose this
/// option" button when that option is already the active one.</summary>
public sealed class NotEqualToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !Equals(value, parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
