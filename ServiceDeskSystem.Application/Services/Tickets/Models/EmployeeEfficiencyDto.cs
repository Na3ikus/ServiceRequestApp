namespace ServiceDeskSystem.Application.Services.Tickets.Models;

public class EmployeeEfficiencyDto
{
    public string DeveloperName { get; set; } = string.Empty;
    public int TicketsAssigned { get; set; }
    public int TicketsClosed { get; set; }
    public int TotalTimeSpentMinutes { get; set; }
    public double AverageTimePerTicketMinutes { get; set; }
    public double ClosureRatePercentage { get; set; }
}
