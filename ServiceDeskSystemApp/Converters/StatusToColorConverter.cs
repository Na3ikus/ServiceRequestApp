using System.Globalization;
using ServiceDeskSystemApp.Models;

namespace ServiceDeskSystemApp.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Open => Colors.LightBlue,
                TicketStatus.InProgress => Colors.Orange,
                TicketStatus.Resolved => Colors.LightGreen,
                TicketStatus.Closed => Colors.Gray,
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
