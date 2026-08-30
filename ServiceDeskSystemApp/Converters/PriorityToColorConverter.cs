using System.Globalization;
using ServiceDeskSystemApp.Models;

namespace ServiceDeskSystemApp.Converters;

public class PriorityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TicketPriority priority)
        {
            return priority switch
            {
                TicketPriority.Low => Colors.Green,
                TicketPriority.Medium => Colors.Blue,
                TicketPriority.High => Colors.Orange,
                TicketPriority.Critical => Colors.Red,
                _ => Colors.Transparent
            };
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
