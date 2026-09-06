using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ServiceDeskSystemApp.Converters;

public class FirstLetterConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            return str.Trim()[0].ToString().ToUpperInvariant();
        }
        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
